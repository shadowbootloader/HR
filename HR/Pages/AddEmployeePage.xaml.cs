using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using HR.Controls.AddEmployees;
using HR.Models;
using HR.Services;

namespace HR.Pages
{
    /// <summary>
    /// Interaction logic for AddEmployeePage.xaml
    /// </summary>
    public partial class AddEmployeePage : Page
    {

        private readonly Dictionary<string, UIElement> _sectionCache = new();
        private bool _lazyLoaded = false;



        public AddEmployeePage()
        {
            InitializeComponent();
            Loaded += AddEmployeePage_Loaded;

        }

        private async void AddEmployeePage_Loaded(object sender, RoutedEventArgs e)
        {
            // اگر ۵ تای اول را در XAML نداری، اینجا اضافه کن
            // SectionsPanel.Children.Add(GetOrCreate("Personal"));
            // ...

            await LoadRemainingSectionsAsync();
        }
        private async Task LoadRemainingSectionsAsync()
        {
            if (_lazyLoaded) return;
            _lazyLoaded = true;

            string[] keys =
            {
        "Bank", "Family", "Lang", "Skills", "Abilities",
        "WorkInterests", "SelfAssessment", "TripAbroad",
        "ForeignCooperation", "Citizenship", "Criminal",
        "ForeignRelatives", "Recruitment", "Organ",
        "Other", "Sacrifice", "Supplementary"
    };

            foreach (var key in keys)
            {
                // ساخت روی UI Thread
                var section = GetOrCreate(key);
                if ((UserControl)section != null && ((UserControl)(section)).Parent == null)
                    DataContainerST.Children.Add(section);

                // نَفَس به UI بده تا اسکرول/کلیک قفل نشود
                await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Background);
                await Task.Delay(15); // اختیاری؛ نرم‌تر می‌شود
            }
        }





        private void NewEmployeBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CVImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        //private void PersonalImage_Click(object sender, RoutedEventArgs e)
        //{
        //    var dlg = new Microsoft.Win32.OpenFileDialog
        //    {
        //        Filter = "Image files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png",
        //        Title = "انتخاب عکس پرسنلی"
        //    };

        //    if (dlg.ShowDialog() == true)
        //    {
        //        var bitmap = new BitmapImage();
        //        bitmap.BeginInit();
        //        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        //        bitmap.UriSource = new Uri(dlg.FileName);
        //        bitmap.EndInit();

        //        PersonalImageBrush.ImageSource = bitmap;
        //        PersonPlaceholder.Visibility = Visibility.Collapsed;
        //    }
        //}

        

        private void PersonalPhoneList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PersonalPhoneList_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void PersonalEducationsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PersonalEducationsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void DeleteEducationDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddEducationDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void EducationalCertificateImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteCourseDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PersonalCourseList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PersonalCourseList_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void CourseCertificateImageBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddCourseDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PersonaljobsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void PersonaljobsList_SizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        private void DeletejobDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddJobDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteBankDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddBankDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteLangDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddLangDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddForenTravelDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteForenTravelDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteForeignCooperationDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddForeignCooperationDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PersonalForeignCooperationList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DeleteCitizenshipDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddCitizenshipDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteCriminalDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddCriminalDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddForeignFamilyDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddEmploymentDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteEmploymentDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteCooperationOrganDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddCooperationOrganDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteOtherActivityDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddOtherActivityDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DeleteSacrificeDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AddSacrificeDataBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private UIElement GetOrCreate(string key)
        {
            if (_sectionCache.TryGetValue(key, out var existing))
                return existing;

            UIElement section = key switch
            {
                "Abilities" => new Abilities(),
                "Bank" => new BankAccounts(),
                "Citizenship" => new Citizenship(),
                "Criminal" => new Criminal(),
                "Family" => new Family(),
                "ForeignCooperation" => new ForeignCooperation(),
                "ForeignRelatives" => new ForeignRelatives(),
                "Lang" => new Languages(),
                "Skills" => new Skills(),
                "Organ" => new OrganCollaboration(),
                "WorkInterests" => new WorkInterests(),
                "SelfAssessment" => new SelfAssessment(),
                "Other" => new OtherActivity(),
                "Recruitment" => new Recruitment(),
                "Sacrifice" => new Sacrifice(),
                "Supplementary" => new Supplementary(),
                "TripAbroad" => new TripAbroad(),
                _ => throw new NotImplementedException()
            };

            if (section != null)
                _sectionCache[key] = section;

            return section;
        }
        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            //if (_lazyLoaded) return;

            //var sv = (ScrollViewer)sender;
            //if (sv.ExtentHeight <= 0) return;

            //// حدود ۲۰۰ پیکسل مانده به ته
            //if (sv.VerticalOffset + sv.ViewportHeight >= sv.ExtentHeight - 200)
            //    LoadRemainingSections();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            NotificationService.Success(
                "ثبت موفق",
                "اطلاعات کارمند با موفقیت ثبت شد.");

        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            NotificationService.Error(
                "خطا",
                "در ذخیره اطلاعات مشکلی به وجود آمد.");

        }
    }
}
