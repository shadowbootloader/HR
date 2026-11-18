namespace HR.Services.Data
{
    public static class ProfileIssues
    {
        public static IReadOnlyList<DashboardListItem> Build(
            IEnumerable<(Guid Id, string Name, ProfileReport Report)> people,
            IEnumerable<(short Id, string Name, bool HasManager, bool HasDeputy, bool HasLogo, bool HasParent, bool IsHeadquarters, int Actual, int Capacity)> departments,
            IEnumerable<(string Title, string EmployeeName, Guid EmployeeId, DateTime Due)> overdue)
        {
            var items = new List<DashboardListItem>();
            foreach (var person in people)
            {
                if (person.Report.Percent >= 80
                    && !person.Report.Missing.Contains("عکس")
                    && !person.Report.Missing.Contains("رزومه")
                    && !person.Report.Missing.Contains("دپارتمان")
                    && !person.Report.Missing.Contains("مدیر مستقیم"))
                    continue;
                items.Add(new DashboardListItem
                {
                    EmployeeId = person.Id,
                    Title = person.Name,
                    Subtitle = "تکمیل " + person.Report.Percent + "٪ — " + string.Join("، ", person.Report.Missing.Take(4))
                });
            }

            foreach (var item in overdue)
            {
                items.Add(new DashboardListItem
                {
                    EmployeeId = item.EmployeeId,
                    Title = item.EmployeeName,
                    Subtitle = "آزمون عقب‌افتاده: " + item.Title + " — موعد " + item.Due.ToString("yyyy/MM/dd")
                });
            }

            foreach (var department in departments)
            {
                var gaps = new List<string>();
                if (!department.HasManager) gaps.Add("بدون مدیر");
                if (!department.HasDeputy) gaps.Add("بدون جانشین");
                if (department.Capacity > 0 && department.Actual > department.Capacity) gaps.Add("بیش از ظرفیت");
                if (!department.HasLogo) gaps.Add("بدون لوگو");
                if (!department.HasParent && !department.IsHeadquarters) gaps.Add("بدون والد");
                if (gaps.Count == 0) continue;
                items.Add(new DashboardListItem
                {
                    DepartmentId = department.Id,
                    Title = department.Name,
                    Subtitle = string.Join("، ", gaps)
                });
            }

            return items;
        }
    }
}
