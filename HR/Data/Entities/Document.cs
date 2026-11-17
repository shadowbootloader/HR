using System;

namespace HR.Data.Entities
{
    public class DocumentType
    {
        public byte DocumentTypeId { get; set; }
        public string Title { get; set; } = string.Empty;
    }

    /// <summary>
    /// پیوند فایل روی دیسک به کارمند یا دپارتمان. خود بایت‌ها در پایگاه نیست.
    /// </summary>
    public class Document
    {
        public Guid DocumentId { get; set; }
        public Guid? EmployeeId { get; set; }
        public short? DepartmentId { get; set; }
        public byte DocumentTypeId { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredRelativePath { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string Sha256 { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public bool IsDeleted { get; set; }

        public Employee? Employee { get; set; }
        public Department? Department { get; set; }
        public DocumentType Type { get; set; } = null!;
    }
}
