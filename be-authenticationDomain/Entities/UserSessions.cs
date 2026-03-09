namespace be_authenticationDomain.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string? DeviceName { get; set; }
        public string? DeviceId { get; set; }    
        public string? UserAgent { get; set; }
        public string? IpAddress { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }

        public bool IsActive => RevokedAt == null;

        // ==== Navigate ====
        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
