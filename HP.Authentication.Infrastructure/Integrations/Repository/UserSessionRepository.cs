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

        #region Add UserSession

        /// <summary>
        /// Adds a new user session to the current database context.
        /// The session is not persisted until SaveChangesAsync is called.
        ///
        /// Thêm mới một user session vào context hiện tại.
        /// Session chỉ được lưu thực sự xuống database khi gọi SaveChangesAsync.
        /// </summary>
        public async Task AddAsync(UserSession session)
        {
            await _context.Set<UserSession>().AddAsync(session);
        }

        #endregion

        #region Update UserSession

        /// <summary>
        /// Updates a single user session.
        /// Typically used when refreshing session metadata such as LastSeenAt,
        /// device information, or revoke state.
        ///
        /// Cập nhật một user session.
        /// Thường dùng khi làm mới thông tin session như LastSeenAt,
        /// thông tin thiết bị, hoặc trạng thái revoke.
        /// </summary>
        public void Update(UserSession session)
        {
            _context.Set<UserSession>().Update(session);
        }

        #endregion

        #region Update List UserSession

        /// <summary>
        /// Updates multiple user sessions simultaneously.
        /// Typically used when revoking all active sessions of a user.
        ///
        /// Cập nhật nhiều user session cùng lúc.
        /// Thường dùng khi revoke toàn bộ session active của một user.
        /// </summary>
        public void UpdateRange(IEnumerable<UserSession> sessions)
        {
            _context.Set<UserSession>().UpdateRange(sessions);
        }

        #endregion

        #region Save Changes

        /// <summary>
        /// Persists all pending changes in the current database context.
        ///
        /// Lưu toàn bộ thay đổi đang chờ trong database context hiện tại.
        /// </summary>
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Get Session By ID

        /// <summary>
        /// Finds a user session by its identifier.
        /// This method returns the session regardless of whether it is active or revoked.
        ///
        /// Tìm user session theo định danh session.
        /// Hàm này trả về session bất kể đang active hay đã bị revoke.
        /// </summary>
        public async Task<UserSession?> GetByIdAsync(Guid sessionId)
        {
            return await _context.Set<UserSession>()
                .FirstOrDefaultAsync(x => x.Id == sessionId);
        }

        #endregion

        #region Get Active Session By ID

        /// <summary>
        /// Finds an active user session by its identifier.
        /// Only sessions that have not been revoked are returned.
        ///
        /// Tìm user session đang active theo định danh session.
        /// Chỉ trả về những session chưa bị revoke.
        /// </summary>
        public async Task<UserSession?> GetActiveByIdAsync(Guid sessionId)
        {
            return await _context.Set<UserSession>()
                .FirstOrDefaultAsync(x => x.Id == sessionId && x.RevokedAt == null);
        }

        #endregion

        #region Get Active Sessions By User

        /// <summary>
        /// Retrieves all active sessions of a specific user.
        /// Sessions are ordered by latest activity, falling back to creation time if needed.
        ///
        /// Lấy tất cả session đang active của một user cụ thể.
        /// Danh sách được sắp xếp theo thời gian hoạt động gần nhất,
        /// nếu không có thì dùng thời gian tạo session.
        /// </summary>
        public async Task<List<UserSession>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.Set<UserSession>()
                .Where(x => x.UserId == userId && x.RevokedAt == null)
                .OrderByDescending(x => x.LastSeenAt ?? x.CreatedAt)
                .ToListAsync();
        }
        #endregion

        #region Get Active Session By Device
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
