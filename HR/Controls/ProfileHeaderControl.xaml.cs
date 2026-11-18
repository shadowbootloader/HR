using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HR.Services;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for ProfileHeaderControl.xaml
    /// </summary>
    public partial class ProfileHeaderControl : UserControl
    {
        public ProfileHeaderControl()
        {
            InitializeComponent();
        }

        public void Bind(string? fullName, string? position, string? department, string? location)
        {
            EmpFullName.Text = string.IsNullOrWhiteSpace(fullName) ? "—" : fullName;
            EmpGroup.Text = string.IsNullOrWhiteSpace(position) ? "—" : position;
            EmpFeild.Text = string.IsNullOrWhiteSpace(department) ? "—" : department;
            EmpLocation.Text = string.IsNullOrWhiteSpace(location) ? string.Empty : location;
        }

        /// <summary>عکس پرسنلی از مسیر نسبی PersonFile. اگر فایل نباشد، براش خالی می‌ماند.</summary>
        public void BindPhoto(string? storedPath)
        {
            EmpImage.ImageSource = ImagePicker.Load(storedPath);
        }

        /// <summary>کادر «اطلاعات هویتی».</summary>
        public void BindIdentity(string? personnelCode, string? birthDate, string? maritalStatus, string? phone, string? joinDate, string? manager)
            => IdentityBox.FillNotchedBorder(personnelCode ?? "", birthDate ?? "", maritalStatus ?? "", phone ?? "", joinDate ?? "", manager ?? "");

        /// <summary>کارت «عملکرد کلی».</summary>
        public void BindPerformance(double? averagePercent, string? levelLabel, int attemptCount)
            => OverallPerformance.Show(averagePercent, levelLabel, attemptCount);

        /// <summary>رنگ نقطهٔ وضعیت: سبز فعال، زرد مرخصی، خاکستری بقیه.</summary>
        public void BindStatus(string? status)
        {
            EmpStatus.Fill = status switch
            {
                "فعال" => new SolidColorBrush(Color.FromRgb(0x1E, 0xD5, 0x8A)),
                "مرخصی" => new SolidColorBrush(Color.FromRgb(0xF5, 0xB7, 0x31)),
                _ => new SolidColorBrush(Color.FromRgb(0x64, 0x74, 0x8B))
            };
            EmpStatus.ToolTip = status;
        }
    }
}
