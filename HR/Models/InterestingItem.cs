using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HR.Models
{
    public class InterestingItem
    {
        public string InterestingItemName { get; set; } = string.Empty;
        public string InterestingItemLevel { get; set; } = string.Empty;  // ضعیف / متوسط / عالی

        public string DisplayText => $"{InterestingItemName}";

        public Brush InterestingLevelBrush =>
            InterestingItemLevel switch
            {
                "زیاد" => Paint("#14532D"),
                "متوسط" => Paint("#1E3A5F"),
                "کم" => Paint("#7F1D1D"),
                _ => Paint("#1E3A5F")
            };

        private static Brush Paint(string hex)
            => new BrushConverter().ConvertFrom(hex) as Brush ?? Brushes.Transparent;
    }
}
