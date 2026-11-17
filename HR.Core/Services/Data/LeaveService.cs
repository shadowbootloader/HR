using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Data
{
    public sealed class LeaveService : IDisposable
    {
        public static readonly string[] Types = { "استحقاقی", "استعلاجی", "بدون حقوق", "زایمان" };

        private readonly PersonnelDbContext _db;
        private readonly bool _ownsContext;

        public LeaveService(PersonnelDbContext? db = null)
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

        public static bool Covers(LeaveRecord leave, DateTime today)
            => !leave.IsCancelled && leave.StartDate.Date <= today.Date && leave.EndDate.Date >= today.Date;

        public static (DateTime EndDate, bool IsCancelled) Close(DateTime start, DateTime end, DateTime today)
        {
            if (start.Date >= today.Date)
                return (end.Date, true);

            var closed = today.Date.AddDays(-1);
            return (closed < start.Date ? start.Date : closed, false);
        }

        public async Task<LeaveRecord> RegisterAsync(Guid employeeId, string leaveType, DateTime start, DateTime end)
        {
            AccessGuard.RequirePeopleManagement();
            if (!Types.Contains(leaveType))
                throw new InvalidOperationException("نوع مرخصی معتبر نیست.");
            if (end.Date < start.Date)
                throw new InvalidOperationException("تاریخ پایان نمی‌تواند قبل از شروع باشد.");
            if (!await _db.Employees.AnyAsync(employee => employee.EmployeeId == employeeId && !employee.IsDeleted))
                throw new InvalidOperationException("کارمند یافت نشد.");

            var open = await OpenAsync(employeeId, DateTime.Today);
            if (open != null)
                throw new InvalidOperationException("این کارمند هم‌اکنون مرخصی باز دارد.");

            var leave = new LeaveRecord
            {
                LeaveId = Guid.NewGuid(),
                EmployeeId = employeeId,
                LeaveType = leaveType,
                StartDate = start.Date,
                EndDate = end.Date,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = AuthSession.ActorName
            };
            _db.LeaveRecords.Add(leave);
            await _db.SaveChangesAsync();
            return leave;
        }

        public async Task EndAsync(Guid leaveId, DateTime? today = null)
        {
            AccessGuard.RequirePeopleManagement();
            var leave = await _db.LeaveRecords.FirstOrDefaultAsync(item => item.LeaveId == leaveId)
                ?? throw new InvalidOperationException("مرخصی یافت نشد.");
            var day = (today ?? DateTime.Today).Date;
            var closed = Close(leave.StartDate, leave.EndDate, day);
            leave.EndDate = closed.EndDate;
            leave.IsCancelled = closed.IsCancelled;
            await _db.SaveChangesAsync();
        }

        public async Task<LeaveRecord?> OpenAsync(Guid employeeId, DateTime today)
        {
            var rows = await _db.LeaveRecords.AsNoTracking()
                .Where(item => item.EmployeeId == employeeId)
                .ToListAsync();
            return rows.FirstOrDefault(item => Covers(item, today));
        }

        public void Dispose()
        {
            if (_ownsContext) _db.Dispose();
        }
    }
}
