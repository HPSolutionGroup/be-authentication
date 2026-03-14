using HP.Authentication.Application.Abstractions.Repository;
using HP.Authentication.Domain.Entities;
using HP.Authentication.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HP.Authentication.Infrastructure.Integrations.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MyDbContext _context;

        public RefreshTokenRepository(MyDbContext context)
        {
            _context = context;
        }

        #region Add RefreshToken
        public async Task AddAsync(RefreshToken token)
        {
            await _context.Set<RefreshToken>().AddAsync(token);
        }
        #endregion

        #region Update RefreshToken
        public void Update(RefreshToken token)
        {
            _context.Set<RefreshToken>().Update(token);
        }
        #endregion

        #region Update List RefreshToken
        /// <summary>
        /// Updates multiple refresh tokens simultaneously. 
        /// Typically used when revoking a token family, a specific session, or all tokens of a user.
        /// 
        /// Cập nhật nhiều refresh token cùng lúc.
        /// Thường dùng khi revoke cả family, revoke theo session, revoke tất cả token của user.
        /// </summary>
        public void UpdateRange(IEnumerable<RefreshToken> tokens)
        {
            _context.Set<RefreshToken>().UpdateRange(tokens);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
        #endregion

        #region Get By Token
        /// <summary>
        /// Finds a refresh token by its hash value. 
        /// Can eagerly load the User entity if user information is needed after token verification.
        /// 
        /// Tìm refresh token theo giá trị hash của token.
        /// Có thể include User nếu cần dùng tiếp thông tin user sau khi verify token.
        /// </summary>
        public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, bool includeUser = false)
        {
            IQueryable<RefreshToken> query = _context.Set<RefreshToken>();

            if (includeUser)
            {
                query = query.Include(x => x.User);
            }

            return await query.FirstOrDefaultAsync(x => x.Token == tokenHash);
        }
        #endregion

        #region Get Token By ID
        public async Task<RefreshToken?> GetByIdAsync(Guid id)
        {
            return await _context.Set<RefreshToken>()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        #endregion

        #region Get Active Tokens By Family
        /// <summary>
        /// Retrieves all active refresh tokens within the same family. 
        /// Used to revoke the entire token chain when token reuse (potential hack) is detected.
        /// 
        /// Lấy tất cả refresh token còn active trong cùng một family.
        /// Dùng khi phát hiện token reuse để revoke toàn bộ chuỗi token liên quan.
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveByFamilyIdAsync(Guid familyId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.FamilyId == familyId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
        #endregion

        #region Get Active Tokens By Session
        /// <summary>
        /// Retrieves all active refresh tokens belonging to the same session. 
        /// Used when logging out of a specific device or session.
        /// 
        /// Lấy tất cả refresh token còn active thuộc cùng một session.
        /// Dùng khi logout một thiết bị / một phiên đăng nhập cụ thể.
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveBySessionIdAsync(Guid sessionId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.SessionId == sessionId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
        #endregion

        #region Get Active Tokens By User
        /// <summary>
        /// Retrieves all active refresh tokens for a specific user. 
        /// Used when logging out of all devices or manually revoking all user sessions.
        /// 
        /// Lấy tất cả refresh token còn active của một user.
        /// Dùng khi logout tất cả thiết bị hoặc thu hồi toàn bộ phiên đăng nhập của user.
        /// </summary>
        public async Task<List<RefreshToken>> GetActiveByUserIdAsync(Guid userId)
        {
            return await _context.Set<RefreshToken>()
                .Where(x => x.UserId == userId
                            && x.RevokedAt == null
                            && x.ExpiresAt > DateTime.UtcNow)
                .ToListAsync();
        }
        #endregion
    }
}
