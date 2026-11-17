using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HR.Services.Org
{
    public sealed class OrgDepartment
    {
        public short Id { get; init; }
        public string Title { get; init; } = string.Empty;
        public short? ParentId { get; init; }
        public string? ManagerName { get; init; }
    }

    public sealed class OrgMember
    {
        public Guid EmployeeId { get; init; }
        public short DepartmentId { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string? Position { get; init; }
        public bool IsDeleted { get; init; }
    }

    public sealed class OrgChartNode
    {
        public string Id { get; set; } = string.Empty;
        public string? ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public int MemberCount { get; set; }
    }

    public static class OrgChartMapper
    {
        public const string RootId = "org-root";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        public static IReadOnlyList<OrgChartNode> Build(
            IEnumerable<OrgDepartment> departments,
            IEnumerable<OrgMember>? members = null,
            bool includeEmployees = true)
        {
            var list = departments.ToList();
            var byId = list.ToDictionary(item => item.Id);
            var liveMembers = (members ?? Array.Empty<OrgMember>()).Where(item => !item.IsDeleted).ToList();
            var counts = liveMembers.GroupBy(item => item.DepartmentId).ToDictionary(group => group.Key, group => group.Count());

            var nodes = new List<OrgChartNode>
            {
                new()
                {
                    Id = RootId,
                    Name = "سازمان",
                    Subtitle = "ساختار واحدها",
                    Kind = "root",
                    MemberCount = liveMembers.Count
                }
            };

            foreach (var department in list)
            {
                nodes.Add(new OrgChartNode
                {
                    Id = DepartmentNodeId(department.Id),
                    ParentId = ResolveParent(department, byId),
                    Name = department.Title,
                    Subtitle = string.IsNullOrWhiteSpace(department.ManagerName) ? "بدون مدیر" : "مدیر: " + department.ManagerName.Trim(),
                    Kind = "department",
                    EntityId = department.Id.ToString(),
                    MemberCount = counts.TryGetValue(department.Id, out var count) ? count : 0
                });
            }

            if (includeEmployees)
            {
                foreach (var member in liveMembers)
                {
                    if (!byId.ContainsKey(member.DepartmentId)) continue;
                    nodes.Add(new OrgChartNode
                    {
                        Id = "e-" + member.EmployeeId.ToString("N"),
                        ParentId = DepartmentNodeId(member.DepartmentId),
                        Name = member.FullName,
                        Subtitle = string.IsNullOrWhiteSpace(member.Position) ? "کارمند" : member.Position.Trim(),
                        Kind = "employee",
                        EntityId = member.EmployeeId.ToString(),
                        MemberCount = 0
                    });
                }
            }

            return nodes;
        }

        public static string ToJson(IEnumerable<OrgChartNode> nodes)
            => JsonSerializer.Serialize(nodes, JsonOptions);

        public static string DepartmentNodeId(short id) => "d-" + id;

        private static string ResolveParent(OrgDepartment department, IReadOnlyDictionary<short, OrgDepartment> byId)
        {
            if (department.ParentId is not short parentId || parentId == department.Id || !byId.ContainsKey(parentId))
                return RootId;

            var seen = new HashSet<short> { department.Id };
            var cursor = parentId;
            while (true)
            {
                if (!seen.Add(cursor))
                    return RootId;
                if (!byId.TryGetValue(cursor, out var parent) || parent.ParentId is not short next || next == cursor || !byId.ContainsKey(next))
                    return DepartmentNodeId(parentId);
                cursor = next;
            }
        }
    }
}
