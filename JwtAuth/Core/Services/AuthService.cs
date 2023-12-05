using JwtAuth.Core.Dtos;
using JwtAuth.Core.Entities;
using JwtAuth.Core.Interfaces;
using JwtAuth.Core.OtherObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuth.Core.Services
{
    public class AuthService : IAuthService
    {

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
        }


        public async Task<AuthServiceResponseDto> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if (user is null)
                return new AuthServiceResponseDto() 
                { 
                    IsSuccess = false, 
                    Message = "Invalid Credentials" 
                };

            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordCorrect)
                return new AuthServiceResponseDto() 
                { 
                    IsSuccess = false, 
                    Message = "Invalid Credentials" 
                };

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("JWTID", Guid.NewGuid().ToString()),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GenerateJwtToken(authClaims);

            return new AuthServiceResponseDto() 
            { 
                IsSuccess = true, 
                Message = token 
            };
        }

        public async Task<AuthServiceResponseDto> MakeAdminAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
                return new AuthServiceResponseDto() 
                { 
                    IsSuccess = false, 
                    Message = "Invalid UserName" 
                };

            await _userManager.AddToRoleAsync(user, StaticUserRole.ADMIN);

            return new AuthServiceResponseDto() 
            {
                IsSuccess = true,
                Message = "User is now Admin" 
            };
        }

        public async Task<AuthServiceResponseDto> MakeOwnerAsync(UpdatePermissionDto updatePermissionDto)
        {
            var user = await _userManager.FindByNameAsync(updatePermissionDto.UserName);

            if (user is null)
                return new AuthServiceResponseDto()
                { 
                    IsSuccess = false, 
                    Message = "Invalid UserName" 
                };

            await _userManager.AddToRoleAsync(user, StaticUserRole.OWNER);

            return new AuthServiceResponseDto() 
            { 
                IsSuccess = true,
                Message = "User is now Owner" 
            };
        }

        public async Task<AuthServiceResponseDto> ResgisterAsync(RegisterDto registerDto)
        {
            var isExistsUser = await _userManager.FindByNameAsync(registerDto.UserName);

            if (isExistsUser != null)
                return new AuthServiceResponseDto()
                {
                    IsSuccess = false,
                    Message = "User Already Exists"
                };

            ApplicationUser newUser = new ApplicationUser()
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.UserName,
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);

            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation Failed Because: ";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new AuthServiceResponseDto() 
                { 
                    IsSuccess = false, 
                    Message = errorString 
                };
            }

            // Add a Default USER to all users
            await _userManager.AddToRoleAsync(newUser, StaticUserRole.USER);

            return new AuthServiceResponseDto()
            {
                IsSuccess = true,
                Message = "User Created Sucessfully"
            };
        }

        public async Task<AuthServiceResponseDto> SeedRolesAsync()
        {
            bool isOwnerRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.OWNER);
            bool isAdminRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.ADMIN);
            bool isUserRoleExist = await _roleManager.RoleExistsAsync(StaticUserRole.USER);

            if (isOwnerRoleExist && isAdminRoleExist && isUserRoleExist)
                return new AuthServiceResponseDto()
                {
                    IsSuccess = true,
                    Message = "Roles Already Seeded"
                };

            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.USER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRole.OWNER));

            return new AuthServiceResponseDto()
            {
                IsSuccess = true,
                Message = "Role Seeding Done Sucessfully"
            };
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
