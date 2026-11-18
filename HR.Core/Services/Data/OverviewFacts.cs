using System.Globalization;
using HR.Data.Entities;
using HR.Services.Assessments;

namespace HR.Services.Data
{
    public static class OverviewFacts
    {
        public static (string Title, string Description)[] Build(Employee? employee, IEnumerable<AttemptSummary> attempts, DateTime today)
        {
            if (employee == null)
            {
                return new (string, string)[]
                {
                    ("سابقه کار", "—"),
                    ("سن", "—"),
                    ("دوره‌ها", "—"),
                    ("مهارت نرم‌افزاری", "—"),
                    ("زبان‌ها", "—"),
                    ("مرخصی امسال", "—"),
                    ("آخرین آزمون", "—")
                };
            }

            var courses = employee.Courses?.Count ?? 0;
            var certificates = employee.Courses?.Count(course => course.CertificateFileId != null) ?? 0;
            var software = employee.Softwares?
                .Select(item => item.Software?.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Select(title => title!)
                .Distinct()
                .ToList() ?? new List<string>();
            var languages = employee.Languages?
                .Select(item => item.Language?.Title)
                .Where(title => !string.IsNullOrWhiteSpace(title))
                .Select(title => title!)
                .Distinct()
                .ToList() ?? new List<string>();
            var lastTest = attempts
                .Where(item => item.AssessmentCode != SelfAssessmentCatalog.Code && item.SubmittedAt != null)
                .OrderByDescending(item => item.SubmittedAt)
                .Select(item => item.SubmittedAt)
                .FirstOrDefault();

            return new (string, string)[]
            {
                ("سابقه کار", Span(employee.Supplement?.StartDate, today)),
                ("سن", Span(employee.BirthDate, today)),
                ("دوره‌ها", $"{courses} دوره، {certificates} گواهی"),
                ("مهارت نرم‌افزاری", software.Count == 0 ? "—" : string.Join("، ", software)),
                ("زبان‌ها", languages.Count == 0 ? "—" : string.Join("، ", languages)),
                ("مرخصی امسال", $"{LeavesThisYear(employee.Leaves, today)} مورد"),
                ("آخرین آزمون", PersianDate(lastTest))
            };
        }

        public static string Span(DateTime? from, DateTime today)
        {
            if (from == null || from.Value == default || from.Value.Year < 1700) return "—";
            var start = from.Value.Date;
            var end = today.Date;
            if (end < start) return "—";

            var months = (end.Year - start.Year) * 12 + end.Month - start.Month;
            if (end.Day < start.Day) months--;
            if (months < 0) months = 0;
            var years = months / 12;
            var rest = months % 12;
            if (years == 0 && rest == 0) return "کمتر از یک ماه";
            if (years == 0) return $"{rest} ماه";
            if (rest == 0) return $"{years} سال";
            return $"{years} سال و {rest} ماه";
        }

        public static int LeavesThisYear(IEnumerable<LeaveRecord>? leaves, DateTime today)
        {
            if (leaves == null) return 0;
            var calendar = new PersianCalendar();
            var year = calendar.GetYear(today);
            var start = calendar.ToDateTime(year, 1, 1, 0, 0, 0, 0);
            var end = calendar.ToDateTime(year, 12, calendar.GetDaysInMonth(year, 12), 0, 0, 0, 0);
            return leaves.Count(leave =>
                !leave.IsCancelled &&
                leave.StartDate.Date <= end.Date &&
                leave.EndDate.Date >= start.Date);
        }

        public static string PersianDate(DateTime? date)
        {
            if (date == null || date.Value == default || date.Value.Year < 1700) return "—";
            var calendar = new PersianCalendar();
            var value = date.Value;
            return $"{calendar.GetYear(value):0000}/{calendar.GetMonth(value):00}/{calendar.GetDayOfMonth(value):00}";
        }
    }
}
