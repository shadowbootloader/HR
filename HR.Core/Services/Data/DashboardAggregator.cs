using System.Globalization;

namespace HR.Services.Data
{
    public sealed class DashboardPerson
    {
        public Guid EmployeeId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string? PhotoPath { get; init; }
        public bool IsDeleted { get; init; }
        public string? Gender { get; init; }
        public string? Department { get; init; }
        public DateTime? BirthDate { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool OnLeave { get; init; }
        public string? LeaveType { get; init; }
        public DateTime? LeaveEnd { get; init; }
    }

    public sealed class DashboardDepartment
    {
        public string Name { get; init; } = string.Empty;
        public int Capacity { get; init; }
        public bool IsActive { get; init; } = true;
    }

    public sealed class DepartmentLoad
    {
        public string Name { get; init; } = string.Empty;
        public int Actual { get; init; }
        public int Capacity { get; init; }
    }

    public sealed class DashboardAttempt
    {
        public string Code { get; init; } = string.Empty;
        public string Title { get; init; } = string.Empty;
        public string EmployeeName { get; init; } = string.Empty;
        public double? Score { get; init; }
        public string? Level { get; init; }
        public DateTime? SubmittedAt { get; init; }
    }

    public sealed class DashboardListItem
    {
        public string Title { get; init; } = string.Empty;
        public string Subtitle { get; init; } = string.Empty;
        public string? PhotoPath { get; init; }
        public Guid? EmployeeId { get; init; }
        public short? DepartmentId { get; init; }
    }

    public sealed class NamedCount
    {
        public string Name { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public sealed class MonthCount
    {
        public int Year { get; init; }
        public int Month { get; init; }
        public string Label { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public sealed class DashboardSnapshot
    {
        public int Headcount { get; init; }
        public int ActiveRoster { get; init; }
        public int NewHireCount { get; init; }
        public int PreviousNewHireCount { get; init; }
        public int OnLeaveCount { get; init; }
        public int DepartmentCount { get; init; }
        public double CapacityPercent { get; init; }
        public int AssessmentsThisMonth { get; init; }
        public IReadOnlyList<NamedCount> ByDepartment { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<DepartmentLoad> DepartmentLoads { get; init; } = Array.Empty<DepartmentLoad>();
        public IReadOnlyList<NamedCount> ByGender { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<NamedCount> ByAge { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<NamedCount> ByStatus { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<MonthCount> Hires { get; init; } = Array.Empty<MonthCount>();
        public IReadOnlyList<MonthCount> LeaveStarts { get; init; } = Array.Empty<MonthCount>();
        public IReadOnlyList<DashboardListItem> NewHires { get; init; } = Array.Empty<DashboardListItem>();
        public IReadOnlyList<DashboardListItem> OpenLeaves { get; init; } = Array.Empty<DashboardListItem>();
        public IReadOnlyList<DashboardListItem> RecentAttempts { get; init; } = Array.Empty<DashboardListItem>();
        public List<DashboardListItem> Issues { get; set; } = new();
    }

    public static class RosterStatus
    {
        public const string Active = "فعال";
        public const string NewHire = "تازه‌وارد";
        public const string Inactive = "غیرفعال";
        public const string Deleted = "حذف‌شده";
        public const string OnLeave = "مرخصی";

        /// <summary>شروع همکاری در ۳۰ روز اخیر (یا آینده) تازه وارد است، مگر حذف، پایان همکاری یا مرخصی باز.</summary>
        public static string Resolve(bool isDeleted, DateTime? endDate, bool onLeave, DateTime? startDate, DateTime today)
        {
            if (isDeleted) return Deleted;
            if (endDate is DateTime end && end.Date < today.Date) return Inactive;
            if (onLeave) return OnLeave;
            if (startDate is DateTime start && start.Date >= today.Date.AddDays(-30))
                return NewHire;
            return Active;
        }
    }

    public static class DashboardAggregator
    {
        private static readonly string[] Months =
        {
            "", "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
            "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
        };

        private static readonly PersianCalendar Calendar = new();

        public static DashboardSnapshot Build(
            IEnumerable<DashboardPerson> people,
            DateTime today,
            IEnumerable<DashboardDepartment>? departments = null,
            IEnumerable<DateTime>? leaveStarts = null,
            IEnumerable<DashboardAttempt>? attempts = null)
        {
            var rows = people.ToList();
            var roster = rows.Where(person => !person.IsDeleted).ToList();
            var statuses = new[] { RosterStatus.NewHire, RosterStatus.Active, RosterStatus.OnLeave, RosterStatus.Inactive, RosterStatus.Deleted };
            string Status(DashboardPerson person) => RosterStatus.Resolve(person.IsDeleted, person.EndDate, person.OnLeave, person.StartDate, today);
            var activeRoster = roster.Where(person => Status(person) != RosterStatus.Inactive).ToList();
            var units = (departments ?? Array.Empty<DashboardDepartment>()).Where(item => item.IsActive).ToList();
            var loads = units
                .Select(unit => new DepartmentLoad
                {
                    Name = unit.Name,
                    Capacity = Math.Max(0, unit.Capacity),
                    Actual = roster.Count(person => string.Equals(person.Department?.Trim(), unit.Name.Trim(), StringComparison.Ordinal))
                })
                .OrderByDescending(item => item.Actual)
                .ThenBy(item => item.Name)
                .ToList();
            var capacityTotal = loads.Sum(item => item.Capacity);
            var seated = loads.Sum(item => Math.Min(item.Actual, item.Capacity));
            var attemptRows = (attempts ?? Array.Empty<DashboardAttempt>())
                .Where(item => !string.Equals(item.Code, "self-sliders", StringComparison.OrdinalIgnoreCase))
                .ToList();
            var monthStart = MonthStart(today);
            var nextMonth = AddMonths(monthStart, 1);

            return new DashboardSnapshot
            {
                Headcount = roster.Count,
                ActiveRoster = activeRoster.Count,
                NewHireCount = activeRoster.Count(person => Status(person) == RosterStatus.NewHire),
                PreviousNewHireCount = roster.Count(person => person.StartDate is DateTime start && start.Date < today.Date.AddDays(-30) && start.Date >= today.Date.AddDays(-60)),
                OnLeaveCount = roster.Count(person => person.OnLeave && Status(person) == RosterStatus.OnLeave),
                DepartmentCount = units.Count,
                CapacityPercent = capacityTotal == 0 ? 0 : Math.Round(seated * 100.0 / capacityTotal, 1),
                AssessmentsThisMonth = attemptRows.Count(item => item.SubmittedAt is DateTime at && at >= monthStart && at < nextMonth),
                DepartmentLoads = loads,
                ByDepartment = roster
                    .GroupBy(person => string.IsNullOrWhiteSpace(person.Department) ? "بدون دپارتمان" : person.Department.Trim())
                    .Select(group => new NamedCount { Name = group.Key, Count = group.Count() })
                    .OrderByDescending(item => item.Count)
                    .ThenBy(item => item.Name)
                    .ToList(),
                ByGender = roster
                    .GroupBy(person => string.IsNullOrWhiteSpace(person.Gender) ? "نامشخص" : person.Gender.Trim())
                    .Select(group => new NamedCount { Name = group.Key, Count = group.Count() })
                    .OrderByDescending(item => item.Count)
                    .ToList(),
                ByAge = AgeBands(activeRoster, today),
                ByStatus = statuses
                    .Select(name => new NamedCount
                    {
                        Name = name,
                        Count = rows.Count(person => Status(person) == name)
                    })
                    .ToList(),
                Hires = HireMonths(roster.Select(person => person.StartDate).OfType<DateTime>(), today),
                LeaveStarts = HireMonths(leaveStarts ?? Array.Empty<DateTime>(), today),
                NewHires = activeRoster
                    .Where(person => Status(person) == RosterStatus.NewHire)
                    .OrderByDescending(person => person.StartDate)
                    .Take(8)
                    .Select(person => new DashboardListItem
                    {
                        Title = string.IsNullOrWhiteSpace(person.FullName) ? "—" : person.FullName,
                        Subtitle = (person.Department ?? "بدون دپارتمان") + (person.StartDate is DateTime start ? " · " + start.ToString("yyyy/MM/dd") : ""),
                        PhotoPath = person.PhotoPath
                    })
                    .ToList(),
                OpenLeaves = roster
                    .Where(person => person.OnLeave && Status(person) == RosterStatus.OnLeave)
                    .OrderBy(person => person.LeaveEnd)
                    .Take(8)
                    .Select(person => new DashboardListItem
                    {
                        Title = string.IsNullOrWhiteSpace(person.FullName) ? "—" : person.FullName,
                        Subtitle = (person.LeaveType ?? "مرخصی") + (person.LeaveEnd is DateTime end ? " تا " + end.ToString("yyyy/MM/dd") : ""),
                        PhotoPath = person.PhotoPath
                    })
                    .ToList(),
                RecentAttempts = attemptRows
                    .Where(item => item.SubmittedAt != null)
                    .OrderByDescending(item => item.SubmittedAt)
                    .Take(8)
                    .Select(item => new DashboardListItem
                    {
                        Title = item.Title,
                        Subtitle = item.EmployeeName + (item.Score is double score ? " · " + score.ToString("0.#") : "") + (string.IsNullOrWhiteSpace(item.Level) ? "" : " · " + item.Level)
                    })
                    .ToList()
            };
        }

        private static IReadOnlyList<NamedCount> AgeBands(IEnumerable<DashboardPerson> people, DateTime today)
        {
            var bands = new[] { "زیر ۳۰", "۳۰ تا ۳۹", "۴۰ تا ۴۹", "۵۰ و بیشتر", "نامشخص" };
            var counts = bands.ToDictionary(name => name, _ => 0);
            foreach (var person in people)
            {
                if (person.BirthDate is not DateTime birth || birth.Year < 1900)
                {
                    counts["نامشخص"]++;
                    continue;
                }
                var age = today.Year - birth.Year;
                if (birth.Date > today.AddYears(-age)) age--;
                var band = age < 30 ? "زیر ۳۰" : age < 40 ? "۳۰ تا ۳۹" : age < 50 ? "۴۰ تا ۴۹" : "۵۰ و بیشتر";
                counts[band]++;
            }
            return bands.Select(name => new NamedCount { Name = name, Count = counts[name] }).ToList();
        }

        private static DateTime MonthStart(DateTime today)
        {
            var calendar = Calendar;
            return calendar.ToDateTime(calendar.GetYear(today), calendar.GetMonth(today), 1, 0, 0, 0, 0);
        }

        private static DateTime AddMonths(DateTime monthStart, int months)
        {
            var calendar = Calendar;
            return calendar.AddMonths(monthStart, months);
        }

        public static IReadOnlyList<MonthCount> HireMonths(IEnumerable<DateTime> startDates, DateTime today, int months = 12)
        {
            var year = Calendar.GetYear(today);
            var month = Calendar.GetMonth(today);
            var buckets = new List<(int Year, int Month)>();
            for (var offset = months - 1; offset >= 0; offset--)
            {
                var cursorMonth = month - offset;
                var cursorYear = year;
                while (cursorMonth <= 0)
                {
                    cursorMonth += 12;
                    cursorYear--;
                }

                buckets.Add((cursorYear, cursorMonth));
            }

            var counts = startDates
                .GroupBy(date => (Calendar.GetYear(date), Calendar.GetMonth(date)))
                .ToDictionary(group => group.Key, group => group.Count());

            return buckets.Select(bucket => new MonthCount
            {
                Year = bucket.Year,
                Month = bucket.Month,
                Label = Months[bucket.Month],
                Count = counts.TryGetValue(bucket, out var count) ? count : 0
            }).ToList();
        }
    }
}
