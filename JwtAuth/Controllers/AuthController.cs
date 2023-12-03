using JwtAuth.Core.Dtos;
using JwtAuth.Core.Entities;
using JwtAuth.Core.Interfaces;
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
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // Route For Seeding My roles to DB
        [HttpPost]
        [Route("Seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            var seedRoles = await _authService.SeedRolesAsync();

            return Ok(seedRoles);
        }

        // Route For Registering
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto resgisterDto)
        {
            var registerResult = await _authService.ResgisterAsync(resgisterDto);

            if(registerResult.IsSuccess)
                return Ok(registerResult);

            return BadRequest(registerResult);
        }

        // Route For login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);

            if(loginResult.IsSuccess)
               return Ok(loginResult);
            return BadRequest(loginResult);
        }

        // Route to make user Admin
        [HttpPost]
        [Route("make-admin")]
        public async Task<IActionResult> MakeAdmin([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var makeAdminResult = await _authService.MakeAdminAsync(updatePermissionDto);

            if (makeAdminResult.IsSuccess)
                return Ok(makeAdminResult);
            return BadRequest(makeAdminResult);
        }

        // Route to make user Owner 

        [HttpPost]
        [Route("make-owner")]
        public async Task<IActionResult> MakeOwner([FromBody] UpdatePermissionDto updatePermissionDto)
        {
            var makeOwnerResult = await _authService.MakeOwnerAsync(updatePermissionDto);

            if (makeOwnerResult.IsSuccess)
                return Ok(makeOwnerResult);
            return BadRequest(makeOwnerResult);
        }
    }
}
