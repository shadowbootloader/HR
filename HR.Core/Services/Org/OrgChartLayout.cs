namespace HR.Services.Org
{
    public sealed class OrgDepartmentInput
    {
        public short Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public short? ParentId { get; init; }
        public string? LogoPath { get; init; }
        public Guid? ManagerId { get; init; }
        public string? ManagerName { get; init; }
    }

    public sealed class OrgPersonInput
    {
        public Guid EmployeeId { get; init; }
        public short? DepartmentId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Position { get; init; }
        public string? PhotoPath { get; init; }
        public bool IsDeleted { get; init; }
    }

    public sealed class OrgBox
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Kind { get; set; } = "department";
        public string? LogoPath { get; set; }
        public short? DepartmentId { get; init; }
        public Guid? EmployeeId { get; init; }
        public List<OrgBox> Children { get; } = new();
    }

    /// <summary>
    /// چارت بالا به پایین: همیشه یک ریشهٔ «ستاد مرکزی».
    /// هر دپارتمان یک گره است و نیروهایش (مدیر اول) زیر همان گره می‌آیند، بعد زیرمجموعه‌ها.
    /// </summary>
    public static class OrgChartLayout
    {
        public const string HeadquartersTitle = "ستاد مرکزی";

        public static OrgBox Build(
            IEnumerable<OrgDepartmentInput> departments,
            IEnumerable<OrgPersonInput>? people = null,
            short? focusDepartmentId = null)
        {
            var list = departments.ToList();
            var staff = (people ?? Array.Empty<OrgPersonInput>()).Where(person => !person.IsDeleted).ToList();
            var placed = new HashSet<short>();

            if (focusDepartmentId is short focus)
            {
                var department = list.FirstOrDefault(item => item.Id == focus);
                if (department == null)
                    return new OrgBox { Title = "دپارتمان", Subtitle = "یافت نشد", Kind = "empty" };
                return DepartmentBox(department, list, staff, placed);
            }

            var headquarters = list.FirstOrDefault(item => item.Title == HeadquartersTitle && item.ParentId == null)
                ?? list.FirstOrDefault(item => item.Title == HeadquartersTitle);

            OrgBox root;
            if (headquarters == null)
            {
                root = new OrgBox
                {
                    Title = HeadquartersTitle,
                    Subtitle = "ریشه سازمان",
                    Kind = "root"
                };
            }
            else
            {
                root = DepartmentBox(headquarters, list, staff, placed);
            }

            foreach (var department in list.OrderBy(item => item.Title))
            {
                if (placed.Contains(department.Id)) continue;
                if (department.ParentId is short parent && list.Any(item => item.Id == parent))
                    continue;
                root.Children.Add(DepartmentBox(department, list, staff, placed));
            }

            return root;
        }

        private static OrgBox DepartmentBox(
            OrgDepartmentInput department,
            IReadOnlyList<OrgDepartmentInput> all,
            IReadOnlyList<OrgPersonInput> people,
            HashSet<short> placed)
        {
            var node = new OrgBox
            {
                Title = department.Title,
                Kind = department.Title == HeadquartersTitle ? "root" : "department",
                LogoPath = department.LogoPath,
                DepartmentId = department.Id
            };
            if (!placed.Add(department.Id))
            {
                node.Subtitle = "حلقه در ساختار";
                return node;
            }

            var staff = people.Where(person => person.DepartmentId == department.Id).OrderBy(person => person.Name).ToList();
            var manager = department.ManagerId is Guid managerId
                ? staff.FirstOrDefault(person => person.EmployeeId == managerId)
                : null;
            if (manager != null)
                node.Children.Add(PersonBox(manager, "مدیر"));
            else if (!string.IsNullOrWhiteSpace(department.ManagerName))
                node.Children.Add(new OrgBox { Title = department.ManagerName.Trim(), Subtitle = "مدیر", Kind = "person" });

            foreach (var person in staff.Where(person => manager == null || person.EmployeeId != manager.EmployeeId))
                node.Children.Add(PersonBox(person, string.IsNullOrWhiteSpace(person.Position) ? "کارمند" : person.Position.Trim()));

            foreach (var child in all.Where(item => item.ParentId == department.Id && item.Id != department.Id).OrderBy(item => item.Title))
                node.Children.Add(DepartmentBox(child, all, people, placed));

            var managerLabel = string.IsNullOrWhiteSpace(department.ManagerName) ? "بدون مدیر" : "مدیر: " + department.ManagerName.Trim();
            node.Subtitle = managerLabel + " · " + staff.Count + " نفر";
            return node;
        }

        private static OrgBox PersonBox(OrgPersonInput person, string subtitle)
            => new()
            {
                Title = person.Name,
                Subtitle = subtitle,
                Kind = "person",
                LogoPath = person.PhotoPath,
                EmployeeId = person.EmployeeId
            };
    }
}
