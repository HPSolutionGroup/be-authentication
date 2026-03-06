namespace be_authenticationDomain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public string Token { get; set; } 

        public DateTime CreatedAt { get; set; }

        public DateTime ExpiresAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public string? ReplacedByToken { get; set; }

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        public string? IpAddress { get; set; } // IP
        public string? UserAgent { get; set; } // Trình duyệt
        public string? DeviceName { get; set; } // Tên thiết bị

        // ==== Navigate ====
        public Guid UserId { get; set; }
        public User User { get; set; }


        // ====  ====
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
