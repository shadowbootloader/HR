using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HR.Models
{
    public class SoftwareSkillItem
    {
        public string SoftwareName { get; set; } = string.Empty;
        public string SkillLevel { get; set; } = string.Empty;   // ضعیف / متوسط / عالی
        public string Note { get; set; } = string.Empty;

        public string DisplayText => $"{SoftwareName} · {SkillLevel}";
        public Brush SkillLevelBrush
        {
            get
            {
                return SkillLevel switch
                {
                    "عالی" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#14532D")),   // سبز تیره
                    "متوسط" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F")), // آبی
                    "ضعیف" => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F1D1D")),  // قرمز تیره
                    _ => new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E3A5F"))
                };
            }
        }
    }
}
