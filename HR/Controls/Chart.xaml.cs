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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace HR.Controls
{
    /// <summary>
    /// Interaction logic for Chart.xaml
    /// </summary>
    public partial class Chart : UserControl
    {

        private bool isPageReady = false;
        private TaskCompletionSource<bool> pageReadyTcs = new TaskCompletionSource<bool>();
        //private List<ChartEmployee> allEmployees;

        public Chart()
        {
            InitializeComponent();
            //allEmployees = SampleData.GetAllEmployees(); // یا از دیتابیس لود کن
            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            await OrgChartWebView.EnsureCoreWebView2Async(null);

            string folder = AppDomain.CurrentDomain.BaseDirectory;
            OrgChartWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "app", folder, CoreWebView2HostResourceAccessKind.Allow);

            OrgChartWebView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                if (e.IsSuccess)
                {
                    isPageReady = true;
                    pageReadyTcs.TrySetResult(true);
                }
            };

            OrgChartWebView.CoreWebView2.WebMessageReceived += (s, e) =>
            {
                if (e.TryGetWebMessageAsString() == "ready")
                {
                    isPageReady = true;
                    pageReadyTcs.TrySetResult(true);
                }
            };

            OrgChartWebView.CoreWebView2.Navigate("https://app/OrgChart.html");
        }

        // تبدیل عکس به Base64
        private string GetPhotoAsBase64(byte[] photoBytes)
        {
            if (photoBytes == null || photoBytes.Length == 0)
                return null;

            try
            {
                string base64 = Convert.ToBase64String(photoBytes);
                return $"data:image/jpeg;base64,{base64}";
            }
            catch
            {
                return null;
            }
        }

        // نمایش چارت یک دپارتمان
        public async Task ShowDepartment(List<ChartEmployee> Data)
        {
            try
            {
                if (!isPageReady)
                    await pageReadyTcs.Task;

                var employees = Data;

                if (employees.Count == 0)
                {
                    MessageBox.Show("کارمندی برای این دپارتمان پیدا نشد");
                    return;
                }

                var chartData = employees.Select(e => new
                {
                    id = e.Id,
                    pid = e.ManagerId,
                    name = e.FullName,
                    title = e.Position,
                    department = e.DepartmentName,
                    role = e.Role ?? "",
                    color = e.DepartmentColor ?? "#5C6BC0",
                    img = GetPhotoAsBase64(e.PhotoBytes)   // عکس از دیتابیس
                }).ToList();

                string json = System.Text.Json.JsonSerializer.Serialize(chartData);
                OrgChartWebView.CoreWebView2.PostWebMessageAsJson(json);
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطا:\n" + ex.Message);
            }
        }

    }





    public class ChartEmployee
    {
        public int Id { get; set; }
        public int? ManagerId { get; set; }
        public string FullName { get; set; }
        public string Position { get; set; }
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; }
        public string Role { get; set; }              // مثلاً CEO, Manager
        public string DepartmentColor { get; set; }   // مثلاً "#5C6BC0"
        public byte[] PhotoBytes { get; set; }        // عکس از دیتابیس
    }
}
