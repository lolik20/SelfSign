using Microsoft.IdentityModel.Tokens;
using SelfSign.Common.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SelfSign.Utils
{

    public static class Jwt
    {
        private static readonly IConfigurationSection _config;
        static Jwt()
        {
            _config = Startup.JwtSection;
        }

        public static string GetToken(Guid id, Role role)
        {
            var claims = GetIdentity(id, role);
            var jwt = new JwtSecurityToken(
                               issuer: _config["Issuer"],
                               audience: _config["Audience"],
                               notBefore: DateTime.UtcNow,
                               claims: claims,
                               expires: DateTime.UtcNow.AddHours(_config.GetValue<int>("Lifetime")),
                               signingCredentials: new SigningCredentials(GetSymmetricKey(), SecurityAlgorithms.HmacSha256));
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }
        private static SymmetricSecurityKey GetSymmetricKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_config["Key"]));

        }
        private static IEnumerable<Claim> GetIdentity(Guid id, Role role)
        {

            var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType,id.ToString()),
                    new Claim(ClaimsIdentity.DefaultRoleClaimType, role.ToString())
                };
            ClaimsIdentity claimsIdentity =
            new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                ClaimsIdentity.DefaultRoleClaimType);
            
            return claimsIdentity.Claims;
        }

    }
}
