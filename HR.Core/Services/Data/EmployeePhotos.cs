using System;
using System.Collections.Generic;
using System.Linq;
using HR.Data.Entities;

namespace HR.Services.Data
{
    /// <summary>مسیر نسبی عکس پرسنلی ذخیره‌شده روی PersonFile.</summary>
    public static class EmployeePhotos
    {
        public static string? PortraitPath(IEnumerable<PersonFile>? files)
        {
            if (files == null) return null;

            foreach (var file in files)
            {
                var title = file.FileType?.Title;
                if (string.IsNullOrWhiteSpace(title) || title.IndexOf("عکس", StringComparison.Ordinal) < 0)
                    continue;
                if (string.IsNullOrWhiteSpace(file.FileAddress) || file.FileAddress.Trim() == "—")
                    continue;
                return file.FileAddress.Trim();
            }

            return null;
        }
    }
}
