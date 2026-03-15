namespace HP.Authentication.Domain.Entities
{
    public class UserSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// =============================================================

        /// <summary>
        /// Unique hardware/browser identifier to distinguish devices.
        /// Mã định danh thiết bị duy nhất để phân biệt các máy khác nhau.
        /// </summary>
        public string? DeviceId { get; set; }
        
        /// =============================================================
        
        /// <summary>
        /// Name of the device (e.g., "Huy's MacBook").
        /// Tên của thiết bị (ví dụ: "MacBook của Huy").
        /// </summary>
        public string? DeviceName { get; set; }

        /// =============================================================

        /// <summary>
        /// Information about the browser/OS (from HTTP Header).
        /// Thông tin về trình duyệt/hệ điều hành (lấy từ HTTP Header).
        /// </summary>
        public string? UserAgent { get; set; }

        /// =============================================================

        /// <summary>
        /// Client's IP address used for security and location tracking.
        /// Địa chỉ IP của người dùng, dùng để bảo mật và theo dõi vị trí.x
        /// </summary>
        public string? IpAddress { get; set; }

        /// =============================================================

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? LastSeenAt { get; set; }
        public DateTimeOffset? RevokedAt { get; set; }
        public string? RevokedReason { get; set; }

        public bool IsActive => RevokedAt == null;

        // ==== Navigate ====
        public Guid UserId { get; set; }
        public User User { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
