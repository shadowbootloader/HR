using System.Globalization;

namespace HR.Services.Data
{
    public sealed class DashboardPerson
    {
        public bool IsDeleted { get; init; }
        public string? Gender { get; init; }
        public string? Department { get; init; }
        public DateTime? StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public bool OnLeave { get; init; }
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
        public IReadOnlyList<NamedCount> ByDepartment { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<NamedCount> ByGender { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<NamedCount> ByStatus { get; init; } = Array.Empty<NamedCount>();
        public IReadOnlyList<MonthCount> Hires { get; init; } = Array.Empty<MonthCount>();
    }

    public static class RosterStatus
    {
        public const string Active = "فعال";
        public const string Inactive = "غیرفعال";
        public const string Deleted = "حذف‌شده";
        public const string OnLeave = "مرخصی";

        public static string Resolve(bool isDeleted, DateTime? endDate, bool onLeave, DateTime today)
        {
            if (isDeleted) return Deleted;
            if (endDate is DateTime end && end.Date < today.Date) return Inactive;
            if (onLeave) return OnLeave;
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

        public static DashboardSnapshot Build(IEnumerable<DashboardPerson> people, DateTime today)
        {
            var rows = people.ToList();
            var roster = rows.Where(person => !person.IsDeleted).ToList();
            var statuses = new[] { RosterStatus.Active, RosterStatus.OnLeave, RosterStatus.Inactive, RosterStatus.Deleted };

            return new DashboardSnapshot
            {
                Headcount = roster.Count,
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
                ByStatus = statuses
                    .Select(name => new NamedCount
                    {
                        Name = name,
                        Count = rows.Count(person => RosterStatus.Resolve(person.IsDeleted, person.EndDate, person.OnLeave, today) == name)
                    })
                    .ToList(),
                Hires = HireMonths(roster.Select(person => person.StartDate).OfType<DateTime>(), today)
            };
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
