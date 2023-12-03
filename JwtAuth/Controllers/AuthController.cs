using JwtAuth.Core.Dtos;
using JwtAuth.Core.OtherObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.AccessControl;
using System.Security.Claims;
using System.Text;

namespace JwtAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }

        // Route For Seeding My roles to DB
        [HttpPost]
        [Route("Seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            bool isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.OWNER);
            bool isAdminRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.ADMIN);
            bool isUserRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.USER);

            if(isOwnerRoleExist && isAdminRoleExist && isUserRoleExist)
                return Ok("Roles Already Seeded");
            
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.USER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.OWNER));

            return Ok("Role Seeding Done Sucessfully");
        }

        // Route For Registering
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto resgisterDto)
        {
            var isExistsUser = await _userManager.FindByNameAsync(resgisterDto.UserName);

            if(isExistsUser != null)
                return BadRequest("User Already Exists");

            IdentityUser newUser = new IdentityUser()
            {
                Email = resgisterDto.Email,
                UserName = resgisterDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await _userManager.CreateAsync(newUser, resgisterDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation Failed Because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return BadRequest(errorString);
            }

            // Add a Default USER to all users
            await _userManager.AddToRoleAsync(newUser, StaticUserRole.USER);

            return Ok("User Created Sucessfully");
        }

        // Route For login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if(user is null) 
                return Unauthorized("Invalid Credentials");

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if(!isPasswordCorrect)
                return Unauthorized("Invalid Credentials");

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
            };
            foreach(var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));  
            }

            var token = GenerateJwtToken(authClaims);

            return Ok(token);
        }
        private string GenerateJwtToken(List<Claim> authClaims)
        {
            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256)
                );

            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);

            return token;
        }
    }
}
