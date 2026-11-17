using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Assessments
{
    public sealed class AssessmentService : IDisposable
    {
        private readonly PersonnelDbContext _db;
        private readonly bool _ownsContext;

        public AssessmentService(PersonnelDbContext? db = null)
        {
            if (db == null)
            {
                _db = new PersonnelDbContext();
                _ownsContext = true;
            }
            else
            {
                _db = db;
            }
        }

        public async Task EnsureManagerSeedAsync()
        {
            if (await _db.Assessments.AnyAsync(item => item.Code == ManagerAssessmentSeed.Code))
                return;

            _db.Assessments.Add(ToEntity(ManagerAssessmentSeed.Create(), AuthSession.ActorName));
            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AssessmentRecord>> ListAsync()
        {
            await EnsureManagerSeedAsync();
            var rows = await Query().Where(item => item.IsActive).OrderBy(item => item.Title).ToListAsync();
            return rows.Select(ToRecord).ToList();
        }

        public async Task<AssessmentRecord?> GetByCodeAsync(string code)
        {
            await EnsureManagerSeedAsync();
            var row = await Query().FirstOrDefaultAsync(item => item.Code == code);
            return row == null ? null : ToRecord(row);
        }

        public async Task<AssessmentRecord> CreateAsync(AssessmentRecord draft)
        {
            AccessGuard.RequirePeopleManagement();
            if (string.IsNullOrWhiteSpace(draft.Title))
                throw new InvalidOperationException("عنوان آزمون الزامی است.");
            if (draft.Questions.Count == 0)
                throw new InvalidOperationException("حداقل یک سؤال لازم است. متن سؤال از پیش ساخته نمی‌شود.");
            if (draft.Questions.Any(question => string.IsNullOrWhiteSpace(question.Text)))
                throw new InvalidOperationException("متن همه سؤال‌ها الزامی است.");

            draft.Code = string.IsNullOrWhiteSpace(draft.Code) ? "a-" + Guid.NewGuid().ToString("N") : draft.Code.Trim();
            if (await _db.Assessments.AnyAsync(item => item.Code == draft.Code))
                throw new InvalidOperationException("این شناسه آزمون قبلاً ثبت شده است.");

            if (draft.Levels.Count == 0)
            {
                draft.Levels.Add(new LevelRecord { MinScore = 0, MaxScore = 49.99, Label = "نیاز به توسعه", ColorHex = "#FB7185" });
                draft.Levels.Add(new LevelRecord { MinScore = 50, MaxScore = 74.99, Label = "قابل قبول", ColorHex = "#FBBF24" });
                draft.Levels.Add(new LevelRecord { MinScore = 75, MaxScore = 100, Label = "قوی", ColorHex = "#34D399" });
            }

            var entity = ToEntity(draft, AuthSession.ActorName);
            _db.Assessments.Add(entity);
            await _db.SaveChangesAsync();
            return ToRecord(entity);
        }

        public async Task<Guid> SaveAttemptAsync(AttemptSave save)
        {
            if (save.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("کارمند آزمون مشخص نیست.");
            if (!await _db.Employees.AnyAsync(employee => employee.EmployeeId == save.EmployeeId && !employee.IsDeleted))
                throw new InvalidOperationException("کارمند انتخاب‌شده وجود ندارد.");

            var assessment = await _db.Assessments
                .Include(item => item.Questions).ThenInclude(question => question.Options)
                .FirstOrDefaultAsync(item => item.Code == save.AssessmentCode)
                ?? throw new InvalidOperationException("آزمون پیدا نشد.");

            var attempt = new AssessmentAttempt
            {
                AttemptId = Guid.NewGuid(),
                AssessmentId = assessment.AssessmentId,
                EmployeeId = save.EmployeeId,
                Status = "Completed",
                StartedAt = save.StartedAt == default ? DateTime.UtcNow : save.StartedAt,
                SubmittedAt = save.SubmittedAt == default ? DateTime.UtcNow : save.SubmittedAt,
                TotalScore = save.TotalScore,
                LevelLabel = save.LevelLabel,
                Summary = save.Summary
            };

            foreach (var answer in save.Answers)
            {
                var question = assessment.Questions.FirstOrDefault(item =>
                    string.Equals(item.Code, answer.QuestionCode, StringComparison.OrdinalIgnoreCase));
                if (question == null) continue;
                var option = question.Options.FirstOrDefault(item =>
                    string.Equals(item.Code, answer.OptionCode, StringComparison.OrdinalIgnoreCase));
                attempt.Answers.Add(new AssessmentAnswer
                {
                    AnswerId = Guid.NewGuid(),
                    QuestionId = question.QuestionId,
                    OptionId = option?.OptionId,
                    NumericValue = answer.NumericValue,
                    TextValue = answer.TextValue,
                    Score = answer.Score
                });
            }

            _db.AssessmentAttempts.Add(attempt);
            await _db.SaveChangesAsync();
            return attempt.AttemptId;
        }

        public async Task<IReadOnlyList<AttemptSummary>> HistoryAsync(Guid employeeId)
        {
            return await _db.AssessmentAttempts
                .AsNoTracking()
                .Where(item => item.EmployeeId == employeeId)
                .OrderByDescending(item => item.SubmittedAt)
                .Select(item => new AttemptSummary
                {
                    AttemptId = item.AttemptId,
                    EmployeeId = item.EmployeeId,
                    AssessmentCode = item.Assessment.Code,
                    Title = item.Assessment.Title,
                    Category = item.Assessment.Category,
                    TotalScore = item.TotalScore,
                    LevelLabel = item.LevelLabel,
                    SubmittedAt = item.SubmittedAt
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyList<AttemptSummary>> RecentAsync(int take)
        {
            return await _db.AssessmentAttempts
                .AsNoTracking()
                .OrderByDescending(item => item.SubmittedAt)
                .Take(take)
                .Select(item => new AttemptSummary
                {
                    AttemptId = item.AttemptId,
                    EmployeeId = item.EmployeeId,
                    AssessmentCode = item.Assessment.Code,
                    Title = item.Assessment.Title,
                    Category = item.Assessment.Category,
                    TotalScore = item.TotalScore,
                    LevelLabel = item.LevelLabel,
                    SubmittedAt = item.SubmittedAt
                })
                .ToListAsync();
        }

        private IQueryable<Assessment> Query()
            => _db.Assessments
                .Include(item => item.Dimensions)
                .Include(item => item.Levels)
                .Include(item => item.Questions).ThenInclude(question => question.Options);

        public static Assessment ToEntity(AssessmentRecord record, string? createdBy)
        {
            var assessment = new Assessment
            {
                AssessmentId = record.AssessmentId == Guid.Empty ? Guid.NewGuid() : record.AssessmentId,
                Code = record.Code,
                Title = record.Title.Trim(),
                Description = record.Description,
                Category = string.IsNullOrWhiteSpace(record.Category) ? "عمومی" : record.Category.Trim(),
                ScoringMode = string.IsNullOrWhiteSpace(record.ScoringMode) ? "weightedSum" : record.ScoringMode,
                NormalizeTo100 = record.NormalizeTo100,
                EstimatedMinutes = record.EstimatedMinutes,
                QuestionsPerPage = record.QuestionsPerPage <= 0 ? 2 : record.QuestionsPerPage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            foreach (var dimension in record.Dimensions)
            {
                assessment.Dimensions.Add(new AssessmentDimension
                {
                    DimensionId = Guid.NewGuid(),
                    Key = dimension.Key,
                    Title = dimension.Title,
                    Weight = dimension.Weight
                });
            }

            foreach (var level in record.Levels)
            {
                assessment.Levels.Add(new AssessmentLevel
                {
                    LevelId = Guid.NewGuid(),
                    MinScore = level.MinScore,
                    MaxScore = level.MaxScore,
                    Label = level.Label,
                    ColorHex = level.ColorHex
                });
            }

            var order = 1;
            foreach (var question in record.Questions.OrderBy(item => item.SortOrder))
            {
                var entity = new AssessmentQuestion
                {
                    QuestionId = Guid.NewGuid(),
                    Code = string.IsNullOrWhiteSpace(question.Code) ? "q" + order : question.Code,
                    SortOrder = question.SortOrder == 0 ? order : question.SortOrder,
                    QuestionType = question.QuestionType,
                    Text = question.Text.Trim(),
                    HelpText = question.HelpText,
                    IsRequired = question.IsRequired,
                    CountsTowardTotal = question.CountsTowardTotal,
                    Weight = question.Weight <= 0 ? 1 : question.Weight,
                    DimensionKey = question.DimensionKey,
                    MinValue = question.MinValue,
                    MaxValue = question.MaxValue,
                    MinLabel = question.MinLabel,
                    MaxLabel = question.MaxLabel
                };
                var optionOrder = 1;
                foreach (var option in question.Options)
                {
                    entity.Options.Add(new AssessmentOption
                    {
                        OptionId = Guid.NewGuid(),
                        Code = string.IsNullOrWhiteSpace(option.Code) ? optionOrder.ToString() : option.Code,
                        Text = option.Text.Trim(),
                        Score = option.Score,
                        SortOrder = option.SortOrder == 0 ? optionOrder : option.SortOrder
                    });
                    optionOrder++;
                }

                assessment.Questions.Add(entity);
                order++;
            }

            return assessment;
        }

        public static AssessmentRecord ToRecord(Assessment entity)
        {
            return new AssessmentRecord
            {
                AssessmentId = entity.AssessmentId,
                Code = entity.Code,
                Title = entity.Title,
                Description = entity.Description,
                Category = entity.Category,
                ScoringMode = entity.ScoringMode,
                NormalizeTo100 = entity.NormalizeTo100,
                EstimatedMinutes = entity.EstimatedMinutes,
                QuestionsPerPage = entity.QuestionsPerPage,
                Dimensions = entity.Dimensions.Select(item => new DimensionRecord
                {
                    Key = item.Key,
                    Title = item.Title,
                    Weight = item.Weight
                }).ToList(),
                Levels = entity.Levels.Select(item => new LevelRecord
                {
                    MinScore = item.MinScore,
                    MaxScore = item.MaxScore,
                    Label = item.Label,
                    ColorHex = item.ColorHex
                }).ToList(),
                Questions = entity.Questions.OrderBy(item => item.SortOrder).Select(item => new QuestionRecord
                {
                    Code = item.Code,
                    SortOrder = item.SortOrder,
                    QuestionType = item.QuestionType,
                    Text = item.Text,
                    HelpText = item.HelpText,
                    IsRequired = item.IsRequired,
                    CountsTowardTotal = item.CountsTowardTotal,
                    Weight = item.Weight,
                    DimensionKey = item.DimensionKey,
                    MinValue = item.MinValue,
                    MaxValue = item.MaxValue,
                    MinLabel = item.MinLabel,
                    MaxLabel = item.MaxLabel,
                    Options = item.Options.OrderBy(option => option.SortOrder).Select(option => new OptionRecord
                    {
                        Code = option.Code,
                        Text = option.Text,
                        Score = option.Score,
                        SortOrder = option.SortOrder
                    }).ToList()
                }).ToList()
            };
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
