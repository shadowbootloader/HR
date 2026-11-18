using HR.Services.Org;

namespace HR.Tests;

public class OrgChartLayoutTests
{
    [Fact]
    public void AlwaysOneHeadquarters_AndPeopleSitUnderTheirDepartment()
    {
        var tree = OrgChartLayout.Build(
            new[]
            {
                new OrgDepartmentInput { Id = 2, Title = "مالی", ManagerId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), ManagerName = "نگار کریمی" },
                new OrgDepartmentInput { Id = 3, Title = "فناوری", ParentId = 2, ManagerName = "رضا" }
            },
            new[]
            {
                new OrgPersonInput { EmployeeId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), DepartmentId = 2, Name = "نگار کریمی", Position = "مدیر مالی" },
                new OrgPersonInput { EmployeeId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), DepartmentId = 2, Name = "آیدا", Position = "کارشناس" },
                new OrgPersonInput { EmployeeId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), DepartmentId = 3, Name = "کیان" },
                new OrgPersonInput { EmployeeId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), DepartmentId = 2, Name = "حذف", IsDeleted = true }
            });

        Assert.Equal(OrgChartLayout.HeadquartersTitle, tree.Title);
        Assert.Equal("root", tree.Kind);
        var finance = Assert.Single(tree.Children);
        Assert.Equal("مالی", finance.Title);
        Assert.Equal("نگار کریمی", finance.Children[0].Title);
        Assert.Equal("مدیر", finance.Children[0].Subtitle);
        Assert.Equal("آیدا", finance.Children[1].Title);
        Assert.Equal("فناوری", finance.Children[2].Title);
        Assert.Equal("رضا", finance.Children[2].Children[0].Title);
        Assert.Equal("کیان", finance.Children[2].Children[1].Title);
    }

    [Fact]
    public void Focus_ShowsOnlyThatDepartmentTree()
    {
        var tree = OrgChartLayout.Build(
            new[]
            {
                new OrgDepartmentInput { Id = 1, Title = OrgChartLayout.HeadquartersTitle },
                new OrgDepartmentInput { Id = 4, Title = "بازرگانی", ParentId = 1 }
            },
            focusDepartmentId: 4);

        Assert.Equal("بازرگانی", tree.Title);
        Assert.Empty(tree.Children);
    }
}
