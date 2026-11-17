using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HR.Models
{
    public class AbilityItem
    {
        public string AbilityName { get; set; } = string.Empty;
        public string AbilityLevel { get; set; } = string.Empty;  // ضعیف / متوسط / عالی

        public string DisplayText => $"{AbilityName} · {AbilityLevel}";

        public Brush AbilityLevelBrush =>
            AbilityLevel switch
            {
                "عالی" => Paint("#14532D"),
                "متوسط" => Paint("#1E3A5F"),
                "ضعیف" => Paint("#7F1D1D"),
                _ => Paint("#1E3A5F")
            };

        private static Brush Paint(string hex)
            => new BrushConverter().ConvertFrom(hex) as Brush ?? Brushes.Transparent;
    }
}
