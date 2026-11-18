using HR.Data;
using HR.Data.Entities;
using HR.Services.Auth;
using Microsoft.EntityFrameworkCore;

namespace HR.Services.Files
{
    public sealed class InterviewDraft
    {
        public Guid InterviewId { get; set; }
        public DateTime HeldOn { get; set; }
        public string Interviewer { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? TranscriptPath { get; set; }
        public string? AudioPath { get; set; }
    }

    public sealed class InterviewService : IDisposable
    {
        private readonly PersonnelDbContext _db;
        private readonly FileStorageService _storage;
        private readonly bool _owns;

        public InterviewService(PersonnelDbContext? db = null, FileStorageService? storage = null)
        {
            if (db == null)
            {
                _db = new PersonnelDbContext();
                _owns = true;
            }
            else
            {
                _db = db;
            }
            _storage = storage ?? FileStorageService.Load();
        }

        public async Task<IReadOnlyList<InterviewRecord>> ListAsync(Guid employeeId)
            => await _db.InterviewRecords.AsNoTracking()
                .Where(item => item.EmployeeId == employeeId)
                .OrderByDescending(item => item.HeldOn)
                .ToListAsync();

        public async Task ReplaceAsync(Guid employeeId, IReadOnlyList<InterviewDraft> drafts)
        {
            AccessGuard.RequirePeopleManagement();
            var existing = await _db.InterviewRecords.Where(item => item.EmployeeId == employeeId).ToListAsync();
            _db.InterviewRecords.RemoveRange(existing);
            foreach (var draft in drafts)
            {
                if (string.IsNullOrWhiteSpace(draft.Interviewer))
                    throw new InvalidOperationException("نام مصاحبه‌کننده الزامی است.");
                _db.InterviewRecords.Add(new InterviewRecord
                {
                    InterviewId = draft.InterviewId == Guid.Empty ? Guid.NewGuid() : draft.InterviewId,
                    EmployeeId = employeeId,
                    HeldOn = draft.HeldOn == default ? DateTime.Today : draft.HeldOn.Date,
                    Interviewer = draft.Interviewer.Trim(),
                    Notes = string.IsNullOrWhiteSpace(draft.Notes) ? null : draft.Notes.Trim(),
                    TranscriptPath = Promote(draft.TranscriptPath, employeeId),
                    AudioPath = Promote(draft.AudioPath, employeeId)
                });
            }

            await _db.SaveChangesAsync();
        }

        private string? Promote(string? path, Guid employeeId)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;
            var full = _storage.FullPath(path);
            if (!File.Exists(full)) return path.Trim();
            return _storage.Promote(path, $"employees/{employeeId:N}/interviews").RelativePath;
        }

        public void Dispose()
        {
            if (_owns) _db.Dispose();
        }
    }
}
