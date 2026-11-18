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

            return DashboardAggregator.Build(rows, day, departments, leaveStarts, attempts);
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
