using System;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace HR.Controls
{
    public partial class Chart : UserControl
    {
        public Chart()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                await OrgChartWebView.EnsureCoreWebView2Async();
                OrgChartWebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
                    "hr.assets",
                    AppDomain.CurrentDomain.BaseDirectory,
                    CoreWebView2HostResourceAccessKind.Allow);
                OrgChartWebView.CoreWebView2.Navigate("https://hr.assets/Assets/orgchart/index.html");
            };
        }
    }
}
