using HR.Services.Org;

namespace HR.Tests;

public class OrgChartMapperTests
{
    [Fact]
    public void Build_LinksDepartments_CountsLiveMembers_AndKeepsEmployeesOptional()
    {
        var departments = new[]
        {
            new OrgDepartment { Id = 1, Title = "ستاد" },
            new OrgDepartment { Id = 2, Title = "منابع انسانی", ParentId = 1, ManagerName = "نگار کریمی" },
            new OrgDepartment { Id = 3, Title = "حلقه", ParentId = 3 }
        };
        var members = new[]
        {
            new OrgMember { EmployeeId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), DepartmentId = 2, FullName = "نگار کریمی", Position = "کارشناس" },
            new OrgMember { EmployeeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DepartmentId = 2, FullName = "حذف‌شده", IsDeleted = true }
        };

        var nodes = OrgChartMapper.Build(departments, members, includeEmployees: true);
        var human = Assert.Single(nodes, node => node.EntityId == "2");
        Assert.Equal(OrgChartMapper.DepartmentNodeId(1), human.ParentId);
        Assert.Equal(1, human.MemberCount);
        Assert.Equal("مدیر: نگار کریمی", human.Subtitle);
        Assert.Contains(nodes, node => node.Kind == "employee" && node.Name == "نگار کریمی" && node.ParentId == human.Id);
        Assert.DoesNotContain(nodes, node => node.Name == "حذف‌شده");

        var loop = Assert.Single(nodes, node => node.EntityId == "3");
        Assert.Equal(OrgChartMapper.RootId, loop.ParentId);

        var withoutPeople = OrgChartMapper.Build(departments, members, includeEmployees: false);
        Assert.DoesNotContain(withoutPeople, node => node.Kind == "employee");
        Assert.Equal(1, Assert.Single(withoutPeople, node => node.EntityId == "2").MemberCount);
    }

    [Fact]
    public void ToJson_UsesTheFieldsTheChartReads()
    {
        var json = OrgChartMapper.ToJson(OrgChartMapper.Build(
            new[] { new OrgDepartment { Id = 4, Title = "مالی" } },
            includeEmployees: false));

        Assert.Contains("\"id\":\"org-root\"", json);
        Assert.Contains("\"id\":\"d-4\"", json);
        Assert.Contains("\"parentId\":\"org-root\"", json);
        Assert.Contains("\"name\":\"مالی\"", json);
        Assert.Contains("\"kind\":\"department\"", json);
        Assert.Contains("\"memberCount\":0", json);
    }
}
