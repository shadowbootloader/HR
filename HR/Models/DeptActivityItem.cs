using System;
using System.Collections.Generic;
using System.Text;
using MaterialDesignThemes.Wpf;

namespace HR.Models
{
    

    public class DeptActivityItem
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public PackIconKind IconKind { get; set; }
    }
}
