using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using News.Core.Contracts;
using News.Core.Dtos;
using News.Core.Entities;

namespace News.API.Controllers
{
    [ApiController]
	[Route("api/[controller]")]
	public class AccountController( SignInManager<ApplicationUser> _signInManager, IAccountService _accountService) : ControllerBase
	{
        //      // POST : api/account/register
        //      [HttpPost("register")]
        //      public async Task<IActionResult> Register([FromBody] RegisterModel model)
        //      {
        //          var (isSuccess, message) = await _accountService.RegisterUser(model);
        //          if (!isSuccess)
        //              return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = message });

        //          return Ok(new { Status = "Success", Message = message });
        //      }

        //      // POST : api/account/login
        //      [HttpPost("login")]
        //      public async Task<IActionResult> Login([FromBody] LoginModel model)
        //      {
        //          var (isSuccess, token, message) = await _accountService.LoginUser(model);

        //          if (!isSuccess)
        //              return Unauthorized(new { Message = message });

        //          return Ok(new { Token = token });
        //      }

        //      // POST : api/account/logout
        //      [HttpPost("logout")]
        //public async Task<IActionResult> Logout()
        //{
        //	await _signInManager.SignOutAsync();
        //	return Ok(new { result = "Logged out" });
        //}

        //      // POST : api/account/forgot-password
        //      [HttpPost("forgot-password")]
        //      public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
        //      {
        //          var result = await _accountService.ForgotPassword(model.Email);
        //          if (!result.Success)
        //              return BadRequest(new { Status = "Error", Message = result.Message });

        //          return Ok(new { Status = "Success", Messge = result.Message });
        //      }

        //      // POST : api/account/reset-password
        //      [HttpPost("reset-password")]
        //      public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        //      {
        //          var result = await _accountService.ResetPassword(model.Email, model.Token, model.NewPassword);
        //          if (!result.Success)
        //              return BadRequest(new { Status = "Error", Message = result.Message });

        //          return Ok(new { Status = "Success", Message = result.Message });
        //      }

        // POST : api/account/register
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            //AUTOMAPPER
            var (isSuccess, message) = await _accountService.RegisterUser(new RegisterModel
            {
                Email = model.Email,
                UserName = model.Username,
                Password = model.Password,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicUrl = model.profilePicUrl
                //ConfirmPassword = model.ConfirmPassword
            });
            if (!isSuccess)
                return StatusCode(StatusCodes.Status500InternalServerError, new { Status = "Error", Message = message });

            return Ok(new { Status = "Success", Message = message });
        }

        // POST : api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            //AUTOMAPPER
            var (isSuccess, token, message) = await _accountService.LoginUser(new LoginModel
            {
                UserName = model.Username,
                Password = model.Password
            });

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
            var result = await _accountService.ForgotPassword(model.Email);
            if (!result.Success)
                return BadRequest(new { Status = "Error", Message = result.Message });
            return Ok(new { Status = "Success", Message = result.Message });
        }

        // POST : api/account/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var result = await _accountService.ResetPassword(model.Email, model.Token, model.NewPassword);
            if (!result.Success)
                return BadRequest(new { Status = "Error", Message = result.Message });
            return Ok(new { Status = "Success", Message = result.Message });
        }
    }

}
