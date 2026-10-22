using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using HR.Models;

namespace HR.Controls.AddEmployees
{
    public partial class BankAccounts : UserControl, IEmployeeSection
    {
        private readonly ObservableCollection<BankAccountRecord> _items = new();

        public string SectionTitle => "حساب‌های بانکی";

        public BankAccounts()
        {
            InitializeComponent();

            PersonalBankList.ItemsSource = _items;
            _items.CollectionChanged += (_, __) => RowNumbering.Renumber(_items);

            BankNameCB.ItemsSource = new[]
            {
                "ملی", "ملت", "صادرات", "تجارت", "سپه", "کشاورزی", "مسکن",
                "پاسارگاد", "پارسیان", "سامان", "رفاه", "اقتصاد نوین", "سایر"
            };
            BankNameCB.SelectedIndex = 0;

            BankAccountTypeCB.ItemsSource = new[]
            {
                new ComboItem(BankAccountType.Current,   "جاری"),
                new ComboItem(BankAccountType.Savings,   "پس‌انداز"),
                new ComboItem(BankAccountType.ShortTerm, "کوتاه‌مدت"),
                new ComboItem(BankAccountType.LongTerm,  "بلندمدت"),
                new ComboItem(BankAccountType.Other,     "سایر"),
            };
            BankAccountTypeCB.DisplayMemberPath = nameof(ComboItem.Text);
            BankAccountTypeCB.SelectedValuePath = nameof(ComboItem.Value);
            BankAccountTypeCB.SelectedIndex = 0;
        }

        // ==================== رویدادها ====================

        private void AddBankDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var account = BankAccountTB.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(account))
            {
                Warn("شماره حساب را وارد کنید.");
                BankAccountTB.Focus();
                return;
            }

            if (_items.Any(x => x.AccountNumber == account))
            {
                Warn("این شماره حساب قبلاً ثبت شده است.");
                return;
            }

            var sheba = NormalizeSheba(ShebaNumberTB.Text);
            if (sheba.Length > 0 && !IsValidSheba(sheba))
            {
                Warn("شماره شبا باید با IR شروع شود و ۲۴ رقم داشته باشد.");
                ShebaNumberTB.Focus();
                return;
            }

            var card = DigitsOnly(CardNumberTB.Text);
            if (card.Length > 0 && card.Length != 16)
            {
                Warn("شماره کارت باید ۱۶ رقم باشد.");
                CardNumberTB.Focus();
                return;
            }

            _items.Add(new BankAccountRecord
            {
                BankName = BankNameCB.SelectedItem as string ?? string.Empty,
                AccountNumber = account,
                ShebaNumber = sheba,
                CardNumber = card,
                AccountType = BankAccountTypeCB.SelectedValue is BankAccountType t
                    ? t
                    : BankAccountType.Current
            });

            ClearForm();
        }

        private void DeleteBankDataBtn_Click(object sender, RoutedEventArgs e)
        {
            var selected = PersonalBankList.SelectedItems.Cast<BankAccountRecord>().ToList();
            foreach (var item in selected)
                _items.Remove(item);
        }

        // ==================== IEmployeeSection ====================

        public void LoadFrom(EmployeeProfile profile)
        {
            _items.Clear();
            foreach (var account in profile.BankAccounts)
                _items.Add(account);

            RowNumbering.Renumber(_items);
        }

        public void SaveTo(EmployeeProfile profile)
        {
            profile.BankAccounts.Clear();
            foreach (var account in _items)
                profile.BankAccounts.Add(account);
        }

        public bool Validate(out string message)
        {
            foreach (var account in _items)
            {
                if (!string.IsNullOrEmpty(account.ShebaNumber) && !IsValidSheba(account.ShebaNumber))
                {
                    message = $"شماره شبا نامعتبر در ردیف {account.Row}";
                    return false;
                }

                if (!string.IsNullOrEmpty(account.CardNumber) && account.CardNumber.Length != 16)
                {
                    message = $"شماره کارت نامعتبر در ردیف {account.Row}";
                    return false;
                }
            }

            message = string.Empty;
            return true;
        }

        // ==================== کمکی‌ها ====================

        private static string DigitsOnly(string? value)
            => string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : new string(value.Where(char.IsDigit).ToArray());

        private static string NormalizeSheba(string? value)
        {
            var raw = DigitsOnly(value);
            return raw.Length == 0 ? string.Empty : "IR" + raw;
        }

        private static bool IsValidSheba(string sheba)
            => Regex.IsMatch(sheba, @"^IR\d{24}$");

        private void ClearForm()
        {
            BankAccountTB.Clear();
            ShebaNumberTB.Clear();
            CardNumberTB.Clear();
            BankNameCB.SelectedIndex = 0;
            BankAccountTypeCB.SelectedIndex = 0;
            BankAccountTB.Focus();
        }

        private static void Warn(string text)
            => MessageBox.Show(text, "حساب‌های بانکی", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}