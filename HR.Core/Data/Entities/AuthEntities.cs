using System;
using System.Collections.Generic;

namespace HR.Data.Entities
{
    public class AppUser
    {
        public Guid AppUserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();
        public int Iterations { get; set; }
        public Guid? EmployeeId { get; set; }
        public bool IsActive { get; set; } = true;
        public int FailedAttemptCount { get; set; }
        public DateTime? LockoutEndUtc { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public Employee? Employee { get; set; }
        public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
    }

    public class Role
    {
        public byte RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<UserRole> Users { get; set; } = new List<UserRole>();
    }

    public class UserRole
    {
        public Guid AppUserId { get; set; }
        public byte RoleId { get; set; }
        public AppUser User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
