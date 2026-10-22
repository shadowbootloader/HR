using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace HR.Models
{
    public class InterestingItem
    {
        public string InterestingItemName { get; set; }
        public string InterestingItemLevel { get; set; }  // ضعیف / متوسط / عالی

        public string DisplayText => $"{InterestingItemName}";

        public Brush InterestingLevelBrush =>
            InterestingItemLevel switch
            {
                "زیاد" => (Brush)new BrushConverter().ConvertFrom("#14532D"),
                "متوسط" => (Brush)new BrushConverter().ConvertFrom("#1E3A5F"),
                "کم" => (Brush)new BrushConverter().ConvertFrom("#7F1D1D"),
                _ => (Brush)new BrushConverter().ConvertFrom("#1E3A5F")
            };
    }
}
