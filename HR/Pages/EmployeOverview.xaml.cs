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
        }
    }
}