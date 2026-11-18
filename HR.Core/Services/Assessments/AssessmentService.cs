using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using HR.Services.Data;
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

        public static readonly IReadOnlyDictionary<string, string> CategoryByCode = new Dictionary<string, string>
        {
            ["big-five-short"] = "شخصیتی",
            ["mbti-style"] = "شخصیتی",
            ["disc-short"] = "شخصیتی",
            ["eq-short"] = "شخصیتی",
            ["riasec-short"] = "رغبت شغلی",
            ["manager-eval"] = "مهارتی",
            ["self-sliders"] = "خودارزیابی"
        };

        public async Task EnsurePersonalityAsync()
        {
            foreach (var definition in PersonalityAssessmentCatalog.All())
            {
                if (await _db.Assessments.AnyAsync(item => item.Code == definition.Code))
                    continue;
                _db.Assessments.Add(ToEntity(definition, AuthSession.ActorName));
            }

            var rows = await _db.Assessments.ToListAsync();
            foreach (var row in rows)
            {
                if (CategoryByCode.TryGetValue(row.Code, out var category) && row.Category != category)
                    row.Category = category;
                else if (string.IsNullOrWhiteSpace(row.Category))
                    row.Category = "عمومی";
            }

            await _db.SaveChangesAsync();
        }

        public async Task EnsureSelfAssessmentAsync()
        {
            if (await _db.Assessments.AnyAsync(item => item.Code == SelfAssessmentCatalog.Code))
                return;

            _db.Assessments.Add(ToEntity(SelfAssessmentCatalog.Create(), AuthSession.ActorName));
            await _db.SaveChangesAsync();
        }

        public async Task RecordSelfAssessmentAsync(Guid employeeId, SelfAssessmentScores scores, DateTime? when = null)
        {
            await EnsureSelfAssessmentAsync();
            var attempt = SelfAssessmentCatalog.ToAttempt(employeeId, scores, when ?? DateTime.UtcNow);
            var latest = await _db.AssessmentAttempts
                .Include(item => item.Answers).ThenInclude(answer => answer.Question)
                .Where(item => item.EmployeeId == employeeId && item.Assessment.Code == SelfAssessmentCatalog.Code)
                .OrderByDescending(item => item.SubmittedAt)
                .FirstOrDefaultAsync();

            if (latest != null && SameScores(latest, attempt))
                return;

            await SaveAttemptAsync(attempt);
        }

        private static bool SameScores(AssessmentAttempt latest, AttemptSave incoming)
        {
            if (latest.Answers.Count != incoming.Answers.Count) return false;
            foreach (var answer in incoming.Answers)
            {
                var stored = latest.Answers.FirstOrDefault(item => item.Question.Code == answer.QuestionCode);
                if (stored == null || stored.NumericValue != answer.NumericValue) return false;
            }

            return true;
        }

        public async Task<IReadOnlyList<AssessmentRecord>> ListAsync()
        {
            await EnsureManagerSeedAsync();
            await EnsureSelfAssessmentAsync();
            await EnsurePersonalityAsync();
            var rows = await Query()
                .Where(item => item.IsActive && item.Code != SelfAssessmentCatalog.Code)
                .OrderBy(item => item.Title)
                .ToListAsync();
            return rows.Select(ToRecord).ToList();
        }

        public async Task<AssessmentRecord?> GetByCodeAsync(string code)
        {
            await EnsureManagerSeedAsync();
            var row = await Query().FirstOrDefaultAsync(item => item.Code == code);
            return row == null ? null : ToRecord(row);
        }

        public async Task AssignAsync(Guid employeeId, string assessmentCode, DateTime dueDate)
        {
            AccessGuard.RequirePeopleManagement();
            await EnsureManagerSeedAsync();
            await EnsurePersonalityAsync();
            var assessment = await _db.Assessments.FirstOrDefaultAsync(item => item.Code == assessmentCode)
                ?? throw new InvalidOperationException("آزمون پیدا نشد.");
            if (!await _db.Employees.AnyAsync(item => item.EmployeeId == employeeId && !item.IsDeleted))
                throw new InvalidOperationException("کارمند یافت نشد.");

            var existing = await _db.AssessmentAssignments.FirstOrDefaultAsync(item => item.EmployeeId == employeeId && item.AssessmentId == assessment.AssessmentId);
            if (existing == null)
            {
                _db.AssessmentAssignments.Add(new AssessmentAssignment
                {
                    AssignmentId = Guid.NewGuid(),
                    EmployeeId = employeeId,
                    AssessmentId = assessment.AssessmentId,
                    DueDate = dueDate.Date,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = AuthSession.ActorName
                });
            }
            else
            {
                existing.DueDate = dueDate.Date;
            }

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<AttemptSummary>> PendingAsync(Guid employeeId, DateTime? today = null)
        {
            var day = (today ?? DateTime.Today).Date;
            var rows = await _db.AssessmentAssignments.AsNoTracking()
                .Where(item => item.EmployeeId == employeeId && !item.Employee.IsDeleted)
                .Select(item => new
                {
                    item.DueDate,
                    Code = item.Assessment.Code,
                    item.Assessment.Title,
                    Done = item.Assessment.Attempts.Any(attempt => attempt.EmployeeId == employeeId && attempt.SubmittedAt != null)
                })
                .ToListAsync();

            return rows.Where(item => !item.Done).Select(item => new AttemptSummary
            {
                AssessmentCode = item.Code,
                Title = item.Title,
                EmployeeId = employeeId,
                SubmittedAt = null,
                Status = item.DueDate.Date < day ? "Overdue" : "Pending",
                LevelLabel = item.DueDate.ToString("yyyy/MM/dd")
            }).ToList();
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

        public async Task<IReadOnlyList<AttemptSummary>> HistoryAsync(Guid employeeId, bool includeSelfAssessment = false)
        {
            var query = _db.AssessmentAttempts
                .AsNoTracking()
                .Where(item => item.EmployeeId == employeeId && !item.Employee.IsDeleted);
            if (!includeSelfAssessment)
                query = query.Where(item => item.Assessment.Code != SelfAssessmentCatalog.Code);

            return await query
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
                    Status = item.Status,
                    SubmittedAt = item.SubmittedAt
                })
                .ToListAsync();
        }

        public async Task<IReadOnlyDictionary<Guid, double>> ScoreAveragesAsync()
        {
            var rows = await _db.AssessmentAttempts.AsNoTracking()
                .Where(item => !item.Employee.IsDeleted
                    && item.SubmittedAt != null
                    && item.TotalScore != null
                    && item.Assessment.Code != SelfAssessmentCatalog.Code)
                .Select(item => new { item.EmployeeId, item.TotalScore })
                .ToListAsync();
            return rows
                .GroupBy(item => item.EmployeeId)
                .ToDictionary(group => group.Key, group => group.Average(item => item.TotalScore!.Value));
        }

        public async Task<IReadOnlyList<AttemptSummary>> RecentAsync(int take)
        {
            return await _db.AssessmentAttempts
                .AsNoTracking()
                .Where(item => !item.Employee.IsDeleted && item.Assessment.Code != SelfAssessmentCatalog.Code)
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
                    Status = item.Status,
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
