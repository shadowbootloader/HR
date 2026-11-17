using Microsoft.EntityFrameworkCore;

namespace HR.Data.Entities
{
    public class Geo
    {
        public int GeoId { get; set; }
        public HierarchyId Node { get; set; } = null!;
        public byte GeoTypeId { get; set; }
        public int SourceId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? NativeName { get; set; }
        public string? CountryCode { get; set; }

        // Computed column
        public int Level { get; set; }

        public GeoType GeoType { get; set; } = null!;
    }
}
