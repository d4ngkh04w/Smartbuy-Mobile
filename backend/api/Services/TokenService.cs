using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace api.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _key;

        public TokenService(IConfiguration config)
        {
            _config = config;
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Key"]!));
        }

        public string CreateToken(IdentityUser user, string roles)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
    
            claims.Add(new Claim(ClaimTypes.Role, roles));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddMonths(int.Parse(_config["JWT:ExpireMonths"]!)),
                SigningCredentials = creds,
                Audience = _config["JWT:Audience"],
                Issuer = _config["JWT:Issuer"],
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // public bool ValidateToken(string token)
        // {
        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     try
        //     {
        //         tokenHandler.ValidateToken(token, new TokenValidationParameters
        //         {
        //             ValidateIssuerSigningKey = true,
        //             IssuerSigningKey = _key,
        //             ValidateIssuer = false,
        //             ValidateAudience = false
        //         }, out SecurityToken validatedToken);
        //         return true;
        //     }
        //     catch
        //     {
        //         return false;
        //     }
        // }
    }
}