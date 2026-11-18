using HR.Data;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Data
{
    public sealed class DashboardService : IDisposable
    {
        private readonly PersonnelDbContext _db;
        private readonly bool _ownsContext;

        public DashboardService(PersonnelDbContext? db = null)
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

        public async Task<DashboardSnapshot> GetAsync(DateTime? today = null)
        {
            var day = (today ?? DateTime.Today).Date;
            var rows = await _db.Employees.AsNoTracking()
                .Select(employee => new DashboardPerson
                {
                    EmployeeId = employee.EmployeeId,
                    FullName = (employee.FirstName + " " + employee.LastName).Trim(),
                    PhotoPath = employee.PersonFiles
                        .Where(file => file.FileType.Title.Contains("عکس"))
                        .Select(file => file.FileAddress)
                        .FirstOrDefault(),
                    IsDeleted = employee.IsDeleted,
                    Gender = employee.Gender.Title,
                    Department = employee.Supplement != null && employee.Supplement.Department != null
                        ? employee.Supplement.Department.Title
                        : null,
                    BirthDate = employee.BirthDate,
                    StartDate = employee.Supplement != null ? employee.Supplement.StartDate : null,
                    EndDate = employee.Supplement != null ? employee.Supplement.EndDate : null,
                    OnLeave = employee.Leaves.Any(leave => !leave.IsCancelled && leave.StartDate <= day && leave.EndDate >= day),
                    LeaveType = employee.Leaves
                        .Where(leave => !leave.IsCancelled && leave.StartDate <= day && leave.EndDate >= day)
                        .Select(leave => leave.LeaveType)
                        .FirstOrDefault(),
                    LeaveEnd = employee.Leaves
                        .Where(leave => !leave.IsCancelled && leave.StartDate <= day && leave.EndDate >= day)
                        .Select(leave => (DateTime?)leave.EndDate)
                        .FirstOrDefault()
                })
                .ToListAsync();

            var departments = await _db.Departments.AsNoTracking()
                .Select(department => new DashboardDepartment
                {
                    Name = department.Title,
                    Capacity = department.ApprovedCapacity ?? 0,
                    IsActive = department.IsActive
                })
                .ToListAsync();
            var leaveStarts = await _db.LeaveRecords.AsNoTracking()
                .Where(leave => !leave.IsCancelled)
                .Select(leave => leave.StartDate)
                .ToListAsync();
            var attempts = await _db.AssessmentAttempts.AsNoTracking()
                .Where(attempt => !attempt.Employee.IsDeleted)
                .Select(attempt => new DashboardAttempt
                {
                    Code = attempt.Assessment.Code,
                    Title = attempt.Assessment.Title,
                    EmployeeName = (attempt.Employee.FirstName + " " + attempt.Employee.LastName).Trim(),
                    Score = attempt.TotalScore,
                    Level = attempt.LevelLabel,
                    SubmittedAt = attempt.SubmittedAt
                })
                .ToListAsync();

            var snapshot = DashboardAggregator.Build(rows, day, departments, leaveStarts, attempts);
            var reports = await new ProfileCompletenessReader(_db).ReadAllAsync();
            var names = rows.Where(person => !person.IsDeleted).ToDictionary(person => person.EmployeeId, person => person.FullName);
            var actual = rows.Where(person => !person.IsDeleted && person.Department != null)
                .GroupBy(person => person.Department!)
                .ToDictionary(group => group.Key, group => group.Count());
            var departmentRows = await _db.Departments.AsNoTracking()
                .Select(department => new
                {
                    department.DepartmentId,
                    department.Title,
                    HasManager = department.ManagerEmployeeId != null,
                    HasDeputy = department.DeputyEmployeeId != null,
                    HasLogo = department.LogoPath != null && department.LogoPath != "",
                    HasParent = department.ParentDepartmentId != null,
                    Actual = 0,
                    Capacity = department.ApprovedCapacity ?? 0
                })
                .ToListAsync();
            var overdue = await _db.AssessmentAssignments.AsNoTracking()
                .Where(item => !item.Employee.IsDeleted && item.DueDate < day && !item.Assessment.Attempts.Any(attempt => attempt.EmployeeId == item.EmployeeId && attempt.SubmittedAt != null))
                .Select(item => new
                {
                    item.Assessment.Title,
                    Name = (item.Employee.FirstName + " " + item.Employee.LastName).Trim(),
                    item.EmployeeId,
                    item.DueDate
                })
                .ToListAsync();
            snapshot.Issues = ProfileIssues.Build(
                reports.Where(pair => names.ContainsKey(pair.Key)).Select(pair => (pair.Key, names[pair.Key], pair.Value)),
                departmentRows.Select(department => (department.DepartmentId, department.Title, department.HasManager, department.HasDeputy, department.HasLogo, department.HasParent, department.Title == "ستاد مرکزی", actual.TryGetValue(department.Title, out var count) ? count : 0, department.Capacity)),
                overdue.Select(item => (item.Title, item.Name, item.EmployeeId, item.DueDate))).ToList();
            return snapshot;
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
