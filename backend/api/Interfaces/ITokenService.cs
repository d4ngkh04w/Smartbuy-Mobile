using Microsoft.AspNetCore.Identity;

namespace api.Interfaces
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user, string role);

        // bool ValidateToken(string token);
    }
}