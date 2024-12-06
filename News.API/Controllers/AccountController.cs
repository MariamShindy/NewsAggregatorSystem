using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class AccountController(IMapper _mapper, SignInManager<ApplicationUser> _signInManager, IAccountService _accountService) : ControllerBase
	{
        // POST : api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register(/*[FromBody]*/[FromForm] RegisterDto model)
        {
            var registerModel = _mapper.Map<RegisterModel>(model);
            var (isSuccess, message,token) = await _accountService.RegisterUserAsync(registerModel);
            if (!isSuccess)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = message });
            return Ok(new { Status = "Success", Message = message, Token = token });
        }

        // POST : api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var loginModel = _mapper.Map<LoginModel>(model);
            var (isSuccess, token,message) = await _accountService.LoginUserAsync(loginModel);
            if (!isSuccess)
                return Unauthorized(new { Message = message });
            return Ok(new { Token = token });
        }

        // POST : api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { result = "Logged out" });
        }

        // POST : api/account/forgot-password
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var result = await _accountService.ForgotPasswordAsync(model.Email);
            if (!result.Success)
                return BadRequest(new { Status = "Error", Message = result.Message });
            return Ok(new { Status = "Success", Message = result.Message });
        }

        // POST : api/account/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _accountService.ResetPasswordAsync(model.Email, model.Token, model.NewPassword);
            if (!result.Success)
                return BadRequest(new { Status = "Error", Message = result.Message });
            return Ok(new { Status = "Success", Message = result.Message });
        }
    }

}
