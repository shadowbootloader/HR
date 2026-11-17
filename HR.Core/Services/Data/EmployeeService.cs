using HR.Data;
using HR.Services;
using HR.Services.Auth;
using HR.Services.Files;
using HR.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HR.Services.Data
{
    public class EmployeeService : IEmployeeService, IDisposable
    {
        private readonly PersonnelDbContext _db;

        public EmployeeService(PersonnelDbContext db)
        {
            _db = db;
        }

        public EmployeeService() : this(new PersonnelDbContext())
        {
        }

        // ==================== خواندن ====================

        public async Task<List<Employee>> GetAllAsync(bool includeDeleted = false)
        {
            var query = _db.Employees
                .Include(e => e.Gender)
                .Include(e => e.MaritalStatus)
                .Include(e => e.Supplement)!
                    .ThenInclude(s => s.Department)
                .Include(e => e.SelfAssessment)
                .Include(e => e.Leaves)
                .AsQueryable();

            if (!includeDeleted)
                query = query.Where(e => !e.IsDeleted);

            return await query
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(Guid employeeId, bool includeDetails = true)
        {
            var query = _db.Employees.AsQueryable();

            if (includeDetails)
            {
                query = query
                    .Include(e => e.Gender)
                    .Include(e => e.MaritalStatus)
                    .Include(e => e.MilitaryStatus)
                    .Include(e => e.Religion)
                    .Include(e => e.Denomination)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.Department)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.InsuranceType)
                    .Include(e => e.Supplement)!
                        .ThenInclude(s => s.CooperationType)
                    .Include(e => e.Educations)
                        .ThenInclude(ed => ed.Degree)
                    .Include(e => e.Educations)
                        .ThenInclude(ed => ed.Status)
                    .Include(e => e.Courses)
                    .Include(e => e.BankAccounts)
                        .ThenInclude(b => b.Bank)
                    .Include(e => e.BankAccounts)
                        .ThenInclude(b => b.AccountType)
                    .Include(e => e.PhoneNumbers)
                        .ThenInclude(p => p.Type)
                    .Include(e => e.PhoneNumbers)
                        .ThenInclude(p => p.Status)
                    .Include(e => e.RelatedPersons)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.Language)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.ReadingLevel)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.WritingLevel)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.ComprehensionLevel)
                    .Include(e => e.Languages)
                        .ThenInclude(l => l.SpeakingLevel)
                    .Include(e => e.Softwares)
                        .ThenInclude(s => s.Software)
                    .Include(e => e.Softwares)
                        .ThenInclude(s => s.SkillLevel)
                    .Include(e => e.EmploymentHistories)
                    .Include(e => e.Citizenships)
                    .Include(e => e.ForeignTravels)
                    .Include(e => e.ForeignRelatives)
                    .Include(e => e.RelatedPersons).ThenInclude(r => r.ForeignCooperations)
                    .Include(e => e.OrganizationCooperations)
                    .Include(e => e.OtherActivities)
                    .Include(e => e.SacrificeRecords)
                    .Include(e => e.HiringRecords)
                    .Include(e => e.PersonFiles)
                    .Include(e => e.Addresses)
                    .Include(e => e.BackgroundRecords)
                    .Include(e => e.ExemptionType)
                    .Include(e => e.ExemptionReason)
                    .Include(e => e.SelfAssessment)
                    .Include(e => e.Abilities).ThenInclude(a => a.Ability)
                    .Include(e => e.Abilities).ThenInclude(a => a.SkillLevel)
                    .Include(e => e.WorkInterests).ThenInclude(w => w.InterestLevel)
                    .Include(e => e.PersonFiles).ThenInclude(p => p.FileType)
                    .Include(e => e.Citizenships).ThenInclude(c => c.Reason)
                    .Include(e => e.BackgroundRecords).ThenInclude(b => b.RecordType)
                    .Include(e => e.ForeignTravels).ThenInclude(t => t.TravelReason)
                    .Include(e => e.ForeignTravels).ThenInclude(t => t.TransportType)
                    .Include(e => e.ForeignRelatives).ThenInclude(f => f.AbroadReason)
                    .Include(e => e.RelatedPersons).ThenInclude(r => r.ForeignCooperations).ThenInclude(f => f.CenterType)
                    .Include(e => e.SacrificeRecords).ThenInclude(s => s.Relationship)
                    .Include(e => e.SacrificeRecords).ThenInclude(s => s.Type)
                    .Include(e => e.SacrificeRecords).ThenInclude(s => s.AmountUnit)
                    .Include(e => e.HiringRecords).ThenInclude(h => h.SectorType)
                    .Include(e => e.HiringRecords).ThenInclude(h => h.Result)
                    .Include(e => e.OrganizationCooperations).ThenInclude(o => o.SectorType);
            }

            return await query
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);
        }

        public async Task<Employee?> GetByNationalCodeAsync(string nationalCode)
        {
            return await _db.Employees
                .Include(e => e.Supplement)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.NationalCode == nationalCode && !e.IsDeleted);
        }

        public async Task<Employee?> GetByPersonnelCodeAsync(string personnelCode)
        {
            return await _db.Employees
                .Include(e => e.Supplement)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Supplement != null &&
                                          e.Supplement.PersonnelCode == personnelCode &&
                                          !e.IsDeleted);
        }

        public async Task<List<Employee>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return await GetAllAsync();

            term = term.Trim();

            return await _db.Employees
                .Include(e => e.Gender)
                .Include(e => e.Supplement)!
                    .ThenInclude(s => s.Department)
                .Where(e => !e.IsDeleted &&
                       (e.FirstName.Contains(term) ||
                        e.LastName.Contains(term) ||
                        e.NationalCode.Contains(term) ||
                        (e.Supplement != null && e.Supplement.PersonnelCode.Contains(term)) ||
                        (e.FirstName + " " + e.LastName).Contains(term)))
                .OrderBy(e => e.LastName)
                .AsNoTracking()
                .ToListAsync();
        }

        // ==================== نوشتن ====================

        public async Task<Employee> CreateAsync(Employee employee)
        {
            AccessGuard.RequirePeopleManagement();
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.EmployeeId == Guid.Empty)
                employee.EmployeeId = Guid.NewGuid();

            employee.IsDeleted = false;

            NormalizeForInsert(employee);
            await ValidateAsync(employee);
            await new DocumentLinkService().SyncEmployeeAsync(_db, employee);

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                _db.Employees.Add(employee);
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
                return employee;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(FormatDbError(ex), ex);
            }
        }

        public async Task<Employee> UpdateAsync(Employee employee)
        {
            AccessGuard.RequirePeopleManagement();
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            if (employee.EmployeeId == Guid.Empty)
                throw new InvalidOperationException("شناسه کارمند برای ویرایش نامعتبر است.");

            var existing = await _db.Employees
                .Include(e => e.Supplement)
                .Include(e => e.Educations)
                .Include(e => e.Courses)
                .Include(e => e.BankAccounts)
                .Include(e => e.PhoneNumbers)
                .Include(e => e.RelatedPersons)
                .Include(e => e.Languages)
                .Include(e => e.Softwares)
                .Include(e => e.EmploymentHistories)
                .Include(e => e.Citizenships)
                .Include(e => e.ForeignTravels)
                .Include(e => e.ForeignRelatives)
                .Include(e => e.RelatedPersons).ThenInclude(r => r.ForeignCooperations)
                .Include(e => e.OrganizationCooperations)
                .Include(e => e.OtherActivities)
                .Include(e => e.SacrificeRecords)
                .Include(e => e.HiringRecords)
                .Include(e => e.BackgroundRecords)
                .Include(e => e.Addresses)
                .Include(e => e.PersonFiles)
                .Include(e => e.Abilities)
                .Include(e => e.WorkInterests)
                .Include(e => e.SelfAssessment)
                .FirstOrDefaultAsync(e => e.EmployeeId == employee.EmployeeId);

            if (existing == null)
                throw new InvalidOperationException("کارمند یافت نشد.");

            await ValidateAsync(employee);
            DetachLookups(employee);

            // فیلدهای اصلی (کلید و پرچم حذف نرم را دست نمی‌زنیم)
            var id = existing.EmployeeId;
            var wasDeleted = existing.IsDeleted;
            _db.Entry(existing).CurrentValues.SetValues(employee);
            existing.EmployeeId = id;
            existing.IsDeleted = wasDeleted;

            if (employee.Supplement != null)
            {
                employee.Supplement.EmployeeId = id;
                if (existing.Supplement == null)
                {
                    if (employee.Supplement.EmployeeSupplementId == Guid.Empty)
                        employee.Supplement.EmployeeSupplementId = Guid.NewGuid();
                    _db.EmployeeSupplements.Add(employee.Supplement);
                }
                else
                {
                    var sid = existing.Supplement.EmployeeSupplementId;
                    _db.Entry(existing.Supplement).CurrentValues.SetValues(employee.Supplement);
                    existing.Supplement.EmployeeSupplementId = sid;
                    existing.Supplement.EmployeeId = id;
                }
            }
            else if (existing.Supplement != null)
            {
                _db.EmployeeSupplements.Remove(existing.Supplement);
                existing.Supplement = null;
            }

            // وابسته‌های RelatedPerson قبل از خود RelatedPerson حذف می‌شوند.
            ReplaceChildren(existing.ForeignCooperations, employee.ForeignCooperations, id, (x, empId) => { x.EmployeeId = empId; if (x.ForeignCooperationId == Guid.Empty) x.ForeignCooperationId = Guid.NewGuid(); });
            ReplaceChildren(existing.Citizenships, employee.Citizenships, id, (x, empId) => { x.EmployeeId = empId; if (x.CitizenshipId == Guid.Empty) x.CitizenshipId = Guid.NewGuid(); });
            ReplaceChildren(existing.BackgroundRecords, employee.BackgroundRecords, id, (x, empId) => { x.EmployeeId = empId; if (x.BackgroundRecordId == Guid.Empty) x.BackgroundRecordId = Guid.NewGuid(); });
            ReplaceChildren(existing.Addresses, employee.Addresses, id, (x, empId) => { x.EmployeeId = empId; x.RelatedPersonId = null; if (x.AddressId == Guid.Empty) x.AddressId = Guid.NewGuid(); });
            ReplaceChildren(existing.PhoneNumbers, employee.PhoneNumbers, id, (x, empId) => { x.EmployeeId = empId; if (x.PhoneNumberId == Guid.Empty) x.PhoneNumberId = Guid.NewGuid(); });
            ReplaceChildren(existing.PersonFiles, employee.PersonFiles, id, (x, empId) => { x.EmployeeId = empId; if (x.PersonFileId == Guid.Empty) x.PersonFileId = Guid.NewGuid(); });
            foreach (var person in existing.RelatedPersons.ToList())
                _db.RemoveRange(person.ForeignCooperations);

            ReplaceChildren(existing.RelatedPersons, employee.RelatedPersons, id, (x, empId) => { x.EmployeeId = empId; if (x.RelatedPersonId == Guid.Empty) x.RelatedPersonId = Guid.NewGuid(); });

            ReplaceChildren(existing.Educations, employee.Educations, id, (x, empId) => { x.EmployeeId = empId; if (x.EducationId == Guid.Empty) x.EducationId = Guid.NewGuid(); });
            ReplaceChildren(existing.Courses, employee.Courses, id, (x, empId) => { x.EmployeeId = empId; if (x.CourseId == Guid.Empty) x.CourseId = Guid.NewGuid(); });
            ReplaceChildren(existing.BankAccounts, employee.BankAccounts, id, (x, empId) => { x.EmployeeId = empId; if (x.BankAccountId == Guid.Empty) x.BankAccountId = Guid.NewGuid(); });
            ReplaceChildren(existing.Languages, employee.Languages, id, (x, empId) => { x.EmployeeId = empId; if (x.EmployeeLanguageId == Guid.Empty) x.EmployeeLanguageId = Guid.NewGuid(); });
            ReplaceChildren(existing.Softwares, employee.Softwares, id, (x, empId) => { x.EmployeeId = empId; if (x.EmployeeSoftwareId == Guid.Empty) x.EmployeeSoftwareId = Guid.NewGuid(); });
            ReplaceChildren(existing.EmploymentHistories, employee.EmploymentHistories, id, (x, empId) => { x.EmployeeId = empId; if (x.EmploymentHistoryId == Guid.Empty) x.EmploymentHistoryId = Guid.NewGuid(); });
            ReplaceChildren(existing.ForeignTravels, employee.ForeignTravels, id, (x, empId) => { x.EmployeeId = empId; if (x.ForeignTravelId == Guid.Empty) x.ForeignTravelId = Guid.NewGuid(); });
            ReplaceChildren(existing.ForeignRelatives, employee.ForeignRelatives, id, (x, empId) => { x.EmployeeId = empId; if (x.ForeignRelativeId == Guid.Empty) x.ForeignRelativeId = Guid.NewGuid(); });
            ReplaceChildren(existing.OrganizationCooperations, employee.OrganizationCooperations, id, (x, empId) => { x.EmployeeId = empId; if (x.OrganizationCooperationId == Guid.Empty) x.OrganizationCooperationId = Guid.NewGuid(); });
            ReplaceChildren(existing.OtherActivities, employee.OtherActivities, id, (x, empId) => { x.EmployeeId = empId; if (x.OtherActivityId == Guid.Empty) x.OtherActivityId = Guid.NewGuid(); });
            ReplaceChildren(existing.SacrificeRecords, employee.SacrificeRecords, id, (x, empId) => { x.EmployeeId = empId; if (x.SacrificeRecordId == Guid.Empty) x.SacrificeRecordId = Guid.NewGuid(); });
            ReplaceChildren(existing.HiringRecords, employee.HiringRecords, id, (x, empId) => { x.EmployeeId = empId; if (x.HiringRecordId == Guid.Empty) x.HiringRecordId = Guid.NewGuid(); });
            ReplaceChildren(existing.Abilities, employee.Abilities, id, (x, empId) => { x.EmployeeId = empId; if (x.EmployeeAbilityId == Guid.Empty) x.EmployeeAbilityId = Guid.NewGuid(); });
            ReplaceChildren(existing.WorkInterests, employee.WorkInterests, id, (x, empId) => { x.EmployeeId = empId; if (x.EmployeeWorkInterestId == Guid.Empty) x.EmployeeWorkInterestId = Guid.NewGuid(); });

            if (employee.SelfAssessment != null)
            {
                if (existing.SelfAssessment == null)
                {
                    employee.SelfAssessment.EmployeeId = id;
                    if (employee.SelfAssessment.EmployeeSelfAssessmentId == Guid.Empty)
                        employee.SelfAssessment.EmployeeSelfAssessmentId = Guid.NewGuid();
                    _db.Set<EmployeeSelfAssessment>().Add(employee.SelfAssessment);
                }
                else
                {
                    var sid = existing.SelfAssessment.EmployeeSelfAssessmentId;
                    _db.Entry(existing.SelfAssessment).CurrentValues.SetValues(employee.SelfAssessment);
                    existing.SelfAssessment.EmployeeSelfAssessmentId = sid;
                    existing.SelfAssessment.EmployeeId = id;
                }
            }
            else if (existing.SelfAssessment != null)
            {
                _db.Set<EmployeeSelfAssessment>().Remove(existing.SelfAssessment);
                existing.SelfAssessment = null;
            }


            // لینک حساب بانکی به Supplement (حتی اگر خالی شد null شود)
            if (existing.Supplement != null)
            {
                existing.Supplement.BankAccountId = existing.BankAccounts.Count > 0
                    ? existing.BankAccounts.First().BankAccountId
                    : null;
            }

            await new DocumentLinkService().SyncEmployeeAsync(_db, employee);

            try
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                await _db.SaveChangesAsync();
                await tx.CommitAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException(FormatDbError(ex), ex);
            }

            return existing;
        }

        private static void DetachLookups(Employee employee)
        {
            employee.Gender = null!;
            employee.MaritalStatus = null!;
            employee.MilitaryStatus = null;
            employee.Religion = null;
            employee.Denomination = null;
            employee.ExemptionType = null;
            employee.ExemptionReason = null;
            employee.Supplement?.Department = null!;
            employee.Supplement?.InsuranceType = null!;
            employee.Supplement?.CooperationType = null!;
            if (employee.Supplement != null) employee.Supplement.Employee = null!;
            if (employee.SelfAssessment != null) employee.SelfAssessment.Employee = null!;

            foreach (var file in employee.PersonFiles) file.FileType = null!;
            foreach (var edu in employee.Educations)
            {
                edu.Degree = null!;
                edu.Status = null!;
                edu.Employee = null!;
            }
            foreach (var bank in employee.BankAccounts)
            {
                bank.Bank = null!;
                bank.AccountType = null!;
                bank.Employee = null!;
            }
            foreach (var phone in employee.PhoneNumbers)
            {
                phone.Type = null!;
                phone.Status = null!;
                phone.Employee = null;
            }
            foreach (var language in employee.Languages)
            {
                language.Employee = null!;
                language.Language = null!;
                language.ReadingLevel = null!;
                language.WritingLevel = null!;
                language.ComprehensionLevel = null!;
                language.SpeakingLevel = null!;
            }
            foreach (var software in employee.Softwares)
            {
                software.Employee = null!;
                software.Software = null!;
                software.SkillLevel = null!;
            }
            foreach (var ability in employee.Abilities)
            {
                ability.Employee = null!;
                ability.Ability = null!;
                ability.SkillLevel = null!;
            }
            foreach (var person in employee.RelatedPersons)
            {
                person.Employee = null!;
                foreach (var cooperation in person.ForeignCooperations)
                {
                    cooperation.CenterType = null!;
                    cooperation.Employee = null;
                    cooperation.RelatedPerson = null;
                }
            }
        }

        private void ReplaceChildren<T>(
            ICollection<T> existing,
            ICollection<T>? incoming,
            Guid employeeId,
            Action<T, Guid> prepare) where T : class
        {
            foreach (var old in existing.ToList())
            {
                existing.Remove(old);
                _db.Remove(old);
            }

            if (incoming == null) return;
            foreach (var item in incoming)
            {
                prepare(item, employeeId);
                existing.Add(item);
                _db.Entry(item).State = EntityState.Added;
            }
        }

        public async Task<bool> SoftDeleteAsync(Guid employeeId)
        {
            AccessGuard.RequirePeopleManagement();
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.IsDeleted = true;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HardDeleteAsync(Guid employeeId)
        {
            AccessGuard.RequirePeopleManagement();
            var employee = await _db.Employees
                .Include(e => e.Supplement)
                .Include(e => e.SelfAssessment)
                .Include(e => e.Educations)
                .Include(e => e.Courses)
                .Include(e => e.BankAccounts)
                .Include(e => e.PhoneNumbers)
                .Include(e => e.Languages)
                .Include(e => e.Softwares)
                .Include(e => e.EmploymentHistories)
                .Include(e => e.Citizenships)
                .Include(e => e.ForeignTravels)
                .Include(e => e.ForeignRelatives)
                .Include(e => e.RelatedPersons).ThenInclude(r => r.ForeignCooperations)
                .Include(e => e.OrganizationCooperations)
                .Include(e => e.OtherActivities)
                .Include(e => e.SacrificeRecords)
                .Include(e => e.HiringRecords)
                .Include(e => e.BackgroundRecords)
                .Include(e => e.PersonFiles)
                .Include(e => e.Addresses)
                .Include(e => e.Abilities)
                .Include(e => e.WorkInterests)
                .Include(e => e.RelatedPersons)
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee == null) return false;

            var managedDepartments = await _db.Departments
                .Where(d => d.ManagerEmployeeId == employeeId
                         || d.DeputyEmployeeId == employeeId
                         || d.CashCustodianEmployeeId == employeeId)
                .ToListAsync();
            foreach (var department in managedDepartments)
            {
                if (department.ManagerEmployeeId == employeeId) department.ManagerEmployeeId = null;
                if (department.DeputyEmployeeId == employeeId) department.DeputyEmployeeId = null;
                if (department.CashCustodianEmployeeId == employeeId) department.CashCustodianEmployeeId = null;
            }

            await new DocumentLinkService().RemoveEmployeeAsync(_db, employeeId);

            _db.RemoveRange(employee.ForeignCooperations);
            _db.RemoveRange(employee.RelatedPersons.SelectMany(p => p.ForeignCooperations));
            _db.RemoveRange(employee.Citizenships);
            _db.RemoveRange(employee.BackgroundRecords);
            _db.RemoveRange(employee.Addresses);
            _db.RemoveRange(employee.PhoneNumbers);
            _db.RemoveRange(employee.PersonFiles);
            _db.RemoveRange(employee.Educations);
            _db.RemoveRange(employee.Courses);
            _db.RemoveRange(employee.BankAccounts);
            _db.RemoveRange(employee.Languages);
            _db.RemoveRange(employee.Softwares);
            _db.RemoveRange(employee.EmploymentHistories);
            _db.RemoveRange(employee.ForeignTravels);
            _db.RemoveRange(employee.ForeignRelatives);
            _db.RemoveRange(employee.OrganizationCooperations);
            _db.RemoveRange(employee.OtherActivities);
            _db.RemoveRange(employee.SacrificeRecords);
            _db.RemoveRange(employee.HiringRecords);
            _db.RemoveRange(employee.Abilities);
            _db.RemoveRange(employee.WorkInterests);
            _db.RemoveRange(employee.RelatedPersons);
            if (employee.Supplement != null) _db.Remove(employee.Supplement);
            if (employee.SelfAssessment != null) _db.Remove(employee.SelfAssessment);
            _db.Employees.Remove(employee);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestoreAsync(Guid employeeId)
        {
            AccessGuard.RequirePeopleManagement();
            var employee = await _db.Employees.FindAsync(employeeId);
            if (employee == null) return false;

            employee.IsDeleted = false;
            await _db.SaveChangesAsync();
            return true;
        }

        // ==================== بررسی تکراری بودن ====================

        public async Task<bool> ExistsNationalCodeAsync(string nationalCode, Guid? excludeId = null)
        {
            var query = _db.Employees.Where(e => e.NationalCode == nationalCode && !e.IsDeleted);
            if (excludeId.HasValue)
                query = query.Where(e => e.EmployeeId != excludeId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> ExistsPersonnelCodeAsync(string personnelCode, Guid? excludeId = null)
        {
            var query = _db.EmployeeSupplements
                .Where(s => s.PersonnelCode == personnelCode);

            if (excludeId.HasValue)
                query = query.Where(s => s.EmployeeId != excludeId.Value);

            return await query.AnyAsync();
        }

        // ==================== کمکی Create ====================

        private static void NormalizeForInsert(Employee employee)
        {
            if (employee.Supplement != null)
            {
                if (employee.Supplement.EmployeeSupplementId == Guid.Empty)
                    employee.Supplement.EmployeeSupplementId = Guid.NewGuid();

                employee.Supplement.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.Educations)
            {
                if (item.EducationId == Guid.Empty) item.EducationId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.Courses)
            {
                if (item.CourseId == Guid.Empty) item.CourseId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.BankAccounts)
            {
                if (item.BankAccountId == Guid.Empty) item.BankAccountId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.PhoneNumbers)
            {
                if (item.PhoneNumberId == Guid.Empty) item.PhoneNumberId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.RelatedPersons)
            {
                if (item.RelatedPersonId == Guid.Empty) item.RelatedPersonId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.Languages)
            {
                if (item.EmployeeLanguageId == Guid.Empty) item.EmployeeLanguageId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.Softwares)
            {
                if (item.EmployeeSoftwareId == Guid.Empty) item.EmployeeSoftwareId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.EmploymentHistories)
            {
                if (item.EmploymentHistoryId == Guid.Empty) item.EmploymentHistoryId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.Citizenships)
            {
                if (item.CitizenshipId == Guid.Empty) item.CitizenshipId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.ForeignTravels)
            {
                if (item.ForeignTravelId == Guid.Empty) item.ForeignTravelId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.ForeignRelatives)
            {
                if (item.ForeignRelativeId == Guid.Empty) item.ForeignRelativeId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.ForeignCooperations)
            {
                if (item.ForeignCooperationId == Guid.Empty) item.ForeignCooperationId = Guid.NewGuid();
                if (item.RelatedPersonId == null) item.EmployeeId = employee.EmployeeId;
                else item.EmployeeId = null;
            }

            foreach (var person in employee.RelatedPersons)
            {
                foreach (var cooperation in person.ForeignCooperations)
                {
                    if (cooperation.ForeignCooperationId == Guid.Empty)
                        cooperation.ForeignCooperationId = Guid.NewGuid();
                    cooperation.EmployeeId = null;
                    cooperation.RelatedPersonId = person.RelatedPersonId;
                }
            }

            foreach (var item in employee.OrganizationCooperations)
            {
                if (item.OrganizationCooperationId == Guid.Empty) item.OrganizationCooperationId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.OtherActivities)
            {
                if (item.OtherActivityId == Guid.Empty) item.OtherActivityId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.SacrificeRecords)
            {
                if (item.SacrificeRecordId == Guid.Empty) item.SacrificeRecordId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.HiringRecords)
            {
                if (item.HiringRecordId == Guid.Empty) item.HiringRecordId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            foreach (var item in employee.BackgroundRecords)
            {
                if (item.BackgroundRecordId == Guid.Empty) item.BackgroundRecordId = Guid.NewGuid();
                item.EmployeeId = employee.EmployeeId;
            }

            if (employee.Supplement != null &&
                employee.Supplement.BankAccountId == null &&
                employee.BankAccounts.Count > 0)
            {
                employee.Supplement.BankAccountId = employee.BankAccounts.First().BankAccountId;
            }
        }

        private async Task ValidateAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FirstName))
                throw new InvalidOperationException("نام الزامی است.");

            if (string.IsNullOrWhiteSpace(employee.LastName))
                throw new InvalidOperationException("نام خانوادگی الزامی است.");

            if (!NationalCode.IsValid(employee.NationalCode))
                throw new InvalidOperationException("کد ملی نامعتبر است.");

            var duplicateNationalCode = await _db.Employees.AnyAsync(e =>
                e.NationalCode == employee.NationalCode && e.EmployeeId != employee.EmployeeId);
            if (duplicateNationalCode)
                throw new InvalidOperationException("این کد ملی قبلاً ثبت شده است.");

            if (employee.GenderId == 0)
                throw new InvalidOperationException("جدول Gender خالی است یا مقدار جنسیت نامعتبر است.");

            if (employee.MaritalStatusId == 0)
                throw new InvalidOperationException("جدول MaritalStatus خالی است یا وضعیت تأهل نامعتبر است.");

            if (employee.GenderId == 2)
            {
                if (employee.MilitaryStatusId != null || employee.ExemptionTypeId != null || employee.ExemptionReasonId != null)
                    throw new InvalidOperationException("برای جنسیت با شناسه ۲، نظام‌وظیفه و معافیت باید خالی باشد.");
            }
            else if (employee.GenderId == 1)
            {
                if (employee.MilitaryStatusId == null)
                    throw new InvalidOperationException("برای جنسیت با شناسه ۱، وضعیت نظام‌وظیفه الزامی است.");

                var exempt = employee.MilitaryStatusId == 1;
                if (exempt && (employee.ExemptionTypeId == null || employee.ExemptionReasonId == null))
                    throw new InvalidOperationException("برای وضعیت نظام‌وظیفه ۱، نوع و علت معافیت الزامی است.");
                if (!exempt && (employee.ExemptionTypeId != null || employee.ExemptionReasonId != null))
                    throw new InvalidOperationException("نوع و علت معافیت فقط وقتی مجاز است که وضعیت نظام‌وظیفه ۱ باشد.");
            }

            if (employee.Supplement != null)
            {
                if (employee.Supplement.DepartmentId <= 0)
                    throw new InvalidOperationException("دپارتمان را از فهرست انتخاب کنید.");

                var exists = await _db.Departments.AsNoTracking()
                    .AnyAsync(d => d.DepartmentId == employee.Supplement.DepartmentId);
                if (!exists)
                    throw new InvalidOperationException("دپارتمان انتخاب‌شده در پایگاه داده وجود ندارد.");

                if (string.IsNullOrWhiteSpace(employee.Supplement.PersonnelCode))
                    throw new InvalidOperationException("کد پرسنلی الزامی است.");
            }

            foreach (var bank in employee.BankAccounts)
            {
                if (bank.BankId <= 0)
                    throw new InvalidOperationException("برای حساب بانکی، بانک معتبر انتخاب نشده است (جدول Bank را بررسی کنید).");
            }

            foreach (var lang in employee.Languages)
            {
                if (lang.LanguageId <= 0)
                    throw new InvalidOperationException("برای زبان، عنوان معتبر انتخاب نشده است (جدول Language را بررسی کنید).");
            }
        }

        private static string FormatDbError(DbUpdateException ex)
        {
            var root = ex.GetBaseException()?.Message ?? ex.Message;

            if (root.Contains("UNIQUE", StringComparison.OrdinalIgnoreCase) ||
                root.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                root.Contains("Cannot insert duplicate", StringComparison.OrdinalIgnoreCase))
            {
                return "مقدار تکراری است (کد ملی، کد پرسنلی، شبا یا کارت بانکی). جزئیات: " + root;
            }

            if (root.Contains("FOREIGN KEY", StringComparison.OrdinalIgnoreCase) ||
                root.Contains("REFERENCE", StringComparison.OrdinalIgnoreCase))
            {
                return "یکی از مقادیر Lookup یا دپارتمان نامعتبر است. جداول پایه را پر کنید. جزئیات: " + root;
            }

            return "خطا در ذخیره در پایگاه داده: " + root;
        }

        public void Dispose() => _db.Dispose();
    }
}
