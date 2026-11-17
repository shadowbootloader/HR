using System.Windows.Controls;

namespace HR.Pages
{
    public partial class EmployeOverview : Page
    {
        /// <summary>کارمندی که این صفحه نمایش می‌دهد (ممکن است null باشد).</summary>
        public EmployeeModel? Employee { get; }

        public EmployeOverview() : this(null) { }

        public EmployeOverview(EmployeeModel? employee)
        {
            InitializeComponent();
            Employee = employee;
            Loaded += (_, _) => BindEmployee();
        }

        private void BindEmployee()
        {
            if (Employee == null)
            {
                Header.Bind("کارمندی انتخاب نشده", null, null, null);
                Insights.ShowFacts(("پرونده", "از فهرست کارکنان یک نفر را باز کنید."));
                return;
            }

            Header.Bind(Employee.FullName, Employee.Position, Employee.Department, Employee.Email);
            Insights.ShowFacts(
                ("کد ملی", Employee.NationalCode),
                ("کد پرسنلی", Employee.PersonnelCode),
                ("وضعیت", Employee.Status),
                ("ایمیل", Employee.Email),
                ("دپارتمان", Employee.Department));
        }
    }
}