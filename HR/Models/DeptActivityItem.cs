using System;
using System.Collections.Generic;
using System.Text;
using MaterialDesignThemes.Wpf;

namespace HR.Models
{
    

    public class DeptActivityItem
    {
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public PackIconKind IconKind { get; set; }
    }
}
