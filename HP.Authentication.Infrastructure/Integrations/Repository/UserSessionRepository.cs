using HP.Authentication.Application.Abstractions.Repository;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HP.Authentication.Infrastructure.Integrations.Repository
{
    public class UserSessionRepository : IUserSessionRepository
    {
        private readonly MyDbContext _context;

        public UserSessionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserSession session)
        {
            await _context.Set<UserSession>().AddAsync(session);
        }

        public void Update(UserSession session)
        {
            _context.Set<UserSession>().Update(session);
        }

        public void UpdateRange(IEnumerable<UserSession> sessions)
        {
            _context.Set<UserSession>().UpdateRange(sessions);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<UserSession?> GetByIdAsync(Guid sessionId)
        {
            return await _context.Set<UserSession>()
                .FirstOrDefaultAsync(x => x.Id == sessionId);
        }

        public async Task<UserSession?> GetActiveByIdAsync(Guid sessionId)
        {
            return await _context.Set<UserSession>()
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.RevokedAt == null);
        }

        public async Task<List<UserSession>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.Set<UserSession>()
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .OrderByDescending(x => x.LastSeenAt ?? x.CreatedAt)
                .ToListAsync();
        }

        #region 
        /// <summary>
        /// Retrieves an active session for a specific user on a specific device.
        /// Lấy phiên đăng nhập đang hoạt động của một người dùng cụ thể trên một thiết bị cụ thể.
        /// </summary>

        /// <param name="userId">
        /// 
        /// ID của người dùng cần kiểm tra.
        /// </param>
        /// <param name="deviceId">
        /// 
        /// Mã định danh duy nhất của thiết bị/trình duyệt.
        /// </param>

        /// <returns>
        /// 
        /// Trả về UserSession nếu tìm thấy phiên đang hoạt động, ngược lại trả về null
        /// </returns>

        /// <remarks>
        /// Cần đánh Composite Index cho bộ ba (UserId, DeviceId, RevokedAt) để tối ưu hiệu năng 
        /// khi bảng dữ liệu lớn, giúp DB không phải quét toàn bộ bảng (Full Table Scan).
        /// </remarks>
        public async Task<UserSession?> GetActiveByDeviceAsync(Guid userId, string deviceId)
        {
            return await _context.Set<UserSession>()
                .FirstOrDefaultAsync(x => x.UserId == userId
                                       && x.DeviceId == deviceId
                                       && x.RevokedAt == null);
        }
        #endregion
    }
}
