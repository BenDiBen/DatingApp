using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using DatingApp.Api.Data;
using DatingApp.Api.Dtos;
using DatingApp.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext context;
        private readonly IConfiguration config;
        public AuthController(DataContext context, IConfiguration config)
        {
            this.config = config;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto registerUser)
        {
            registerUser.UserName = registerUser.UserName.ToLower();

            if (await this.context.Users.ExistsAsync(registerUser.UserName))
            {
                return this.BadRequest("Username already exists.");
            }

            var registeredUser = await this.context.Users.RegisterAsync(new User { UserName = registerUser.UserName }, registerUser.Password);
            await this.context.SaveChangesAsync();

            if (registeredUser is null)
            {
                return this.NotFound();
            }

            return Ok(registeredUser);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto loginUser)
        {
            var user = await this.context.Users.LoginAsync(loginUser.Username.ToLower(), loginUser.Password);
            if (user is null)
            {
                return this.Unauthorized();
            }

            var claims = new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(this.config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                SigningCredentials = creds,
                Expires = DateTime.Now.AddDays(1)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return this.Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}