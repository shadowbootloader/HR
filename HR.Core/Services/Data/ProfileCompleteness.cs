using HR.Data;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Data
{
    public sealed class ProfileSignals
    {
        public bool Identity { get; init; }
        public bool Phone { get; init; }
        public bool PostalCode { get; init; }
        public bool Photo { get; init; }
        public bool Department { get; init; }
        public bool Position { get; init; }
        public bool DirectManager { get; init; }
        public bool Education { get; init; }
        public bool Course { get; init; }
        public bool Language { get; init; }
        public bool Software { get; init; }
        public bool Resume { get; init; }
        public bool NationalCard { get; init; }
        public bool SelfAssessment { get; init; }
        public bool EmergencyContact { get; init; }
    }

    public sealed class ProfileReport
    {
        public int Percent { get; init; }
        public int RemainingPercent { get; init; }
        public int Score { get; init; }
        public int MaxScore { get; init; }
        public IReadOnlyList<string> Missing { get; init; } = Array.Empty<string>();
    }

    public static class ProfileCompleteness
    {
        private static readonly (string Title, int Weight, Func<ProfileSignals, bool> Done)[] Sections =
        {
            ("هویت", 12, s => s.Identity),
            ("تلفن", 6, s => s.Phone),
            ("کد پستی", 6, s => s.PostalCode),
            ("عکس", 8, s => s.Photo),
            ("دپارتمان", 8, s => s.Department),
            ("سمت", 8, s => s.Position),
            ("مدیر مستقیم", 6, s => s.DirectManager),
            ("تحصیلات", 8, s => s.Education),
            ("دوره", 5, s => s.Course),
            ("زبان", 5, s => s.Language),
            ("نرم‌افزار", 5, s => s.Software),
            ("رزومه", 6, s => s.Resume),
            ("کارت ملی", 6, s => s.NationalCard),
            ("خودارزیابی", 6, s => s.SelfAssessment),
            ("تماس اضطراری", 5, s => s.EmergencyContact)
        };

        public static ProfileReport Score(ProfileSignals signals)
        {
            var max = Sections.Sum(section => section.Weight);
            var earned = 0;
            var missing = new List<string>();
            foreach (var section in Sections)
            {
                if (section.Done(signals)) earned += section.Weight;
                else missing.Add(section.Title);
            }

            var percent = max == 0 ? 0 : (int)Math.Round(earned * 100.0 / max);
            return new ProfileReport
            {
                Percent = percent,
                RemainingPercent = 100 - percent,
                Score = earned,
                MaxScore = max,
            Missing = missing
        };
        }

        public static ProfileSignals FromEmployee(Employee employee)
        {
            var files = employee.PersonFiles ?? new List<PersonFile>();
            bool FileOf(string marker) => files.Any(file => (file.FileType?.Title ?? "").Contains(marker));
            var phones = employee.PhoneNumbers ?? new List<PhoneNumber>();
            return new ProfileSignals
            {
                Identity = !string.IsNullOrWhiteSpace(employee.FirstName)
                    && !string.IsNullOrWhiteSpace(employee.LastName)
                    && !string.IsNullOrWhiteSpace(employee.NationalCode)
                    && employee.BirthDate.Year > 1900,
                Phone = phones.Count > 0,
                PostalCode = (employee.Addresses ?? new List<Address>()).Any(address => !string.IsNullOrWhiteSpace(address.PostalCode)),
                Photo = FileOf("عکس"),
                Department = employee.Supplement?.DepartmentId != null,
                Position = !string.IsNullOrWhiteSpace(employee.Supplement?.PositionTitle),
                DirectManager = employee.Supplement?.DirectManagerEmployeeId != null,
                Education = employee.Educations?.Count > 0,
                Course = employee.Courses?.Count > 0,
                Language = employee.Languages?.Count > 0,
                Software = employee.Softwares?.Count > 0,
                Resume = FileOf("رزومه"),
                NationalCard = FileOf("کارت ملی"),
                SelfAssessment = employee.SelfAssessment != null,
                EmergencyContact = phones.Count > 1 || phones.Any(phone => (phone.Type?.Title ?? "").Contains("اضطر"))
            };
        }
    }

    public sealed class ProfileCompletenessReader : IDisposable
    {
        private readonly PersonnelDbContext _db;
        private readonly bool _owns;

        public ProfileCompletenessReader(PersonnelDbContext? db = null)
        {
            if (db == null)
            {
                _db = new PersonnelDbContext();
                _owns = true;
            }
            else
            {
                _db = db;
            }
        }

        public async Task<IReadOnlyDictionary<Guid, ProfileReport>> ReadAllAsync()
        {
            var rows = await _db.Employees.AsNoTracking()
                .Select(employee => new ProfileSignalsRow
                {
                    Id = employee.EmployeeId,
                    Signals = new ProfileSignals
                    {
                        Identity = employee.FirstName != "" && employee.LastName != "" && employee.NationalCode != "" && employee.BirthDate.Year > 1900,
                        Phone = employee.PhoneNumbers.Any(),
                        PostalCode = employee.Addresses.Any(address => address.PostalCode != null && address.PostalCode != ""),
                        Photo = employee.PersonFiles.Any(file => file.FileType.Title.Contains("عکس")),
                        Department = employee.Supplement != null && employee.Supplement.DepartmentId != null,
                        Position = employee.Supplement != null && employee.Supplement.PositionTitle != "",
                        DirectManager = employee.Supplement != null && employee.Supplement.DirectManagerEmployeeId != null,
                        Education = employee.Educations.Any(),
                        Course = employee.Courses.Any(),
                        Language = employee.Languages.Any(),
                        Software = employee.Softwares.Any(),
                        Resume = employee.PersonFiles.Any(file => file.FileType.Title.Contains("رزومه")),
                        NationalCard = employee.PersonFiles.Any(file => file.FileType.Title.Contains("کارت ملی")),
                        SelfAssessment = employee.SelfAssessment != null,
                        EmergencyContact = employee.PhoneNumbers.Count() > 1
                    }
                })
                .ToListAsync();

            return rows.ToDictionary(row => row.Id, row => ProfileCompleteness.Score(row.Signals));
        }

        public void Dispose()
        {
            if (_owns) _db.Dispose();
        }

        private sealed class ProfileSignalsRow
        {
            public Guid Id { get; init; }
            public ProfileSignals Signals { get; init; } = new();
        }
    }
}
