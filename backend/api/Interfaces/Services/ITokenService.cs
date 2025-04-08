using Microsoft.AspNetCore.Identity;

namespace api.Interfaces.Services
{
    public interface ITokenService
    {
        string CreateToken(IdentityUser user, string role);
    }
}