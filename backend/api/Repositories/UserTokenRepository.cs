using api.Database;
using api.Interfaces.Repositories;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Repositories
{
    public class UserTokenRepository : IUserTokenRepository
    {
        private readonly AppDBContext _dbContext;

        public UserTokenRepository(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserToken> CreateTokenAsync(UserToken userToken)
        {
            await _dbContext.UserTokens.AddAsync(userToken);
            await _dbContext.SaveChangesAsync();
            return userToken;
        }

        public async Task<UserToken?> FindTokenByHashAsync(string tokenHash, string tokenType)
        {
            return await _dbContext.UserTokens
                .Include(ut => ut.User)
                .FirstOrDefaultAsync(ut => ut.TokenHash == tokenHash && ut.TokenType == tokenType);
        }

        public async Task<UserToken?> FindTokenByUserIdAndTypeAsync(Guid userId, string tokenType, string tokenHash)
        {
            return await _dbContext.UserTokens
                .FirstOrDefaultAsync(ut => ut.UserId == userId &&
                                    ut.TokenHash == tokenHash &&
                                    ut.TokenType == tokenType);
        }

        public async Task<List<UserToken>> GetActiveTokensByUserIdAndTypeAsync(Guid userId, string tokenType)
        {
            return await _dbContext.UserTokens
                .Where(ut => ut.UserId == userId &&
                        ut.TokenType == tokenType &&
                        !ut.IsUsed &&
                        !ut.IsRevoked &&
                        ut.ExpiryDate > DateTime.Now)
                .ToListAsync();
        }

        public async Task<bool> RevokeTokenAsync(string tokenHash, string tokenType)
        {
            var token = await _dbContext.UserTokens
                .FirstOrDefaultAsync(ut => ut.TokenHash == tokenHash && ut.TokenType == tokenType);

            if (token == null)
                return false;

            token.IsRevoked = true;
            token.RevokedAt = DateTime.Now;

            _dbContext.UserTokens.Update(token);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkTokenAsUsedAsync(UserToken token)
        {
            token.IsUsed = true;
            _dbContext.UserTokens.Update(token);
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<bool> RevokeAllUserTokensOfTypeAsync(Guid userId, string tokenType)
        {
            var tokens = await _dbContext.UserTokens
                .Where(ut => ut.UserId == userId &&
                        ut.TokenType == tokenType &&
                        !ut.IsRevoked)
                .ToListAsync();

            if (!tokens.Any())
                return true;

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.Now;
            }

            _dbContext.UserTokens.UpdateRange(tokens);
            return await _dbContext.SaveChangesAsync() > 0;
        }
    }
}