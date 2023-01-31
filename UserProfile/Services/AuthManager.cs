using UserProfile.UserDto;
using UserProfile.Model;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNet.Identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Identity;
using UserProfile.Controllers;
using Microsoft.IdentityModel.Tokens;
using Tweetinvi.Core.Models;
using User = UserProfile.Model.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace UserProfile.Services
{

    public class AuthManager : IauthManager
    {
        
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly JwtSettings _jwtSettings;

        

        public async Task<string> ValidateUser(LoginUserDto userDto, object user)
        {
            // ... validate user code ...

            return await CreateToken(user);
        }

        private Task<string> CreateToken(object user)
        {
            throw new NotImplementedException();
        }

        public async Task<string> CreateToken(IdentityUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };
            _ = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.PasswordHash),
                new Claim(ClaimTypes.Role, "Admin")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private JwtSecurityToken GenerateTokenOptions(SigningCredentials signingCredentials, List<Claim> claims)
        {
            var jwtSettings = _config.GetSection("JwtOptions");

            var expiration = DateTime.Now.AddDays(1);

            var token = new JwtSecurityToken(
                
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
                );

            return token;
        }

    }
}
