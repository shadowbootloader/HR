using System;
using System.Threading.Tasks;
using HR.Data.Entities;
using HR.Models;

namespace HR.Services.Data.Mapping
{
    public sealed class BankAccountChildMapper : IEmployeeChildMapper
    {
        public async Task MapAsync(EmployeeProfile profile, Employee entity, LookupResolver lookups)
        {
            if (profile.BankAccounts == null) return;

            foreach (var bank in profile.BankAccounts)
            {
                entity.BankAccounts.Add(new BankAccount
                {
                    BankAccountId = Guid.NewGuid(),
                    EmployeeId = entity.EmployeeId,
                    BankId = await lookups.IdFromTitleAsync<Bank>(bank.BankName),
                    AccountNumber = bank.AccountNumber?.Trim() ?? string.Empty,
                    Sheba = MapperHelpers.Sheba(bank.ShebaNumber),
                    CardNumber = MapperHelpers.Card(bank.CardNumber),
                    AccountTypeId = await lookups.ByteFromTitleAsync<AccountTypeBank>(bank.AccountTypeTitle),
                    Description = null
                });
            }
        }
    }
}