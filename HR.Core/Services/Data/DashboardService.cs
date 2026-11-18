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
                    IsDeleted = employee.IsDeleted,
                    Gender = employee.Gender.Title,
                    Department = employee.Supplement != null && employee.Supplement.Department != null
                        ? employee.Supplement.Department.Title
                        : null,
                    StartDate = employee.Supplement != null ? employee.Supplement.StartDate : null,
                    EndDate = employee.Supplement != null ? employee.Supplement.EndDate : null,
                    OnLeave = employee.Leaves.Any(leave => !leave.IsCancelled && leave.StartDate <= day && leave.EndDate >= day)
                })
                .ToListAsync();

            return DashboardAggregator.Build(rows, day);
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
