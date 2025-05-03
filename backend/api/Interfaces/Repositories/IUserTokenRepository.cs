using api.Models;

namespace api.Interfaces.Repositories
{
    public interface IUserTokenRepository
    {
        Task<UserToken> CreateTokenAsync(UserToken userToken);
        Task<UserToken?> FindTokenByHashAsync(string tokenHash, string tokenType);
        Task<UserToken?> FindTokenByUserIdAndTypeAsync(Guid userId, string tokenType, string tokenHash);
        Task<List<UserToken>> GetActiveTokensByUserIdAndTypeAsync(Guid userId, string tokenType);
        Task<bool> RevokeTokenAsync(string tokenHash, string tokenType);
        Task<bool> MarkTokenAsUsedAsync(UserToken token);
        Task<bool> RevokeAllUserTokensOfTypeAsync(Guid userId, string tokenType);
    }
}