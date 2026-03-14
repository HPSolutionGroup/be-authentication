namespace HP.Authentication.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Token { get; set; } // Lưu Hash

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset ExpiresAt { get; set; }
        public DateTimeOffset? LastUsedAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }

        public Guid? ReplacedByTokenId { get; set; }
        public Guid? ParentTokenId { get; set; }

        public string? CreatedByIp { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReasonRevoked { get; set; }

        public Guid FamilyId { get; set; }
        // ==== Navigate ====
        public Guid UserId { get; set; }
        public User User { get; set; }
        public Guid? SessionId { get; set; }
        public UserSession? Session { get; set; }

        // ====  ====
        public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokedAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
