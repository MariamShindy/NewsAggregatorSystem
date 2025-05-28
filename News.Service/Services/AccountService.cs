namespace News.Service.Services
{
    public class AccountService(ILogger<AccountService> _logger , IMemoryCache _memoryCache , ImageUploader _imageUploader, 
        UserManager<ApplicationUser> _userManager, IConfiguration _configuration,
        IMailSettings _mailSettings) : IAccountService
    {
        public async Task<(bool isSuccess, string message , string? token)> RegisterUserAsync(RegisterModel model)
        {
            _logger.LogInformation("AccountService --> RegisterUser called");
            if (model.Password != model.ConfirmPassword)
            {
                _logger.LogWarning("AccountService --> RegisterUser --> Passwords do not match!");
                return (false, "Password and Confirm Password do not match!", null);
            }
            var userExists = await _userManager.FindByNameAsync(model.UserName);
            if (userExists != null)
            {
                _logger.LogWarning("AccountService --> RegisterUser --> User already exists! ");
                return (false, "User already exists!" ,null);
            }

            string? profilePicUrl = null;
            if (model.ProfilePicUrl is not null)
            {
                profilePicUrl = await _imageUploader.UploadProfileImageAsync(model.ProfilePicUrl);
                _logger.LogInformation("AccountService --> ImageUploader succeeded");
            }
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.UserName,
                FirstName = model.FirstName,
                LastName = model.LastName,
                ProfilePicUrl = profilePicUrl
            };
            var role = !_userManager.Users.Any() ? "Admin" : "User";
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, result.Errors.FirstOrDefault()?.Description ?? "User creation failed" , null);
            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("AccountService --> RegisterUser succeeded");
            var token = GenerateJwtToken(user);
            return (true, "User created successfully!" , token);
        }
        public async Task<(bool isSuccess, string token, string message, bool isDeletionCancelled)> LoginUserAsync(LoginModel model)
        {
            _logger.LogInformation("AccountService --> LoginUser called");

            var user = await _userManager.FindByNameAsync(model.UserName);

            if (user == null)
            {
                _logger.LogWarning("AccountService --> LoginUser --> Account does not exist");
                return (false, null, "Account does not exist.", false);
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                _logger.LogWarning("AccountService --> LoginUser --> Account locked");
                return (false, null, "Account locked.", false);
            }

            // Handle pending deletion and get the status
            var (isSuccess, _, deletionMessage, isDeletionCancelled) = await HandlePendingDeletion(user);
            if (!isSuccess)
                return (false, null, deletionMessage, isDeletionCancelled);

            if (await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = GenerateJwtToken(user);
                _logger.LogInformation("AccountService --> LoginUser succeeded");

                // Return the token and the deletion cancellation status
                var message = isDeletionCancelled ? "Login successful. Deletion request canceled." : "Login successful.";
                return (true, token, message, isDeletionCancelled);
            }

            _logger.LogWarning("AccountService --> LoginUser --> Invalid credentials");
            return (false, null, "Invalid credentials.", false);
        }

        private async Task<(bool isSuccess, string token, string message, bool isDeletionCancelled)> HandlePendingDeletion(ApplicationUser user)
        {
            if (user.IsPendingDeletion && user.DeletionRequestedAt.HasValue)
            {
                if ((DateTime.UtcNow - user.DeletionRequestedAt.Value).TotalDays < 14)
                {
                    user.IsPendingDeletion = false;
                    user.DeletionRequestedAt = null;
                    await _userManager.UpdateAsync(user);
                    _logger.LogInformation("AccountService --> LoginUser --> Deletion request canceled");
                    return (true, null, "Deletion request canceled.", true);
                }
                else
                {
                    _logger.LogWarning("AccountService --> LoginUser --> Account pending deletion period expired");
                    return (false, null, "Account has been deleted.", false);
                }
            }
            return (true, null, "", false);
        }


        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            _logger.LogInformation("AccountService --> ForgotPassword called");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("AccountService --> ForgotPassword --> user not found");
                return (false, "User not found.");
            }

            var verificationCode = new Random().Next(100000, 999999).ToString(); 

            _memoryCache.Set(email, verificationCode, TimeSpan.FromMinutes(15)); 

            var emailBody = $"Your password reset verification code is: {verificationCode}";

            var emailToSend = new Email
            {
                To = email,
                Subject = "Password Reset Verification Code",
                Body = emailBody
            };

            await _mailSettings.SendEmail(emailToSend);
            _logger.LogInformation("AccountService --> ForgotPassword --> Verification code has been sent to user email");

            return (true, "Verification code sent to your email."); 
        }
        public async Task<(bool Success, string Message)> ValidateVerificationCodeAsync(string email, string verificationCode)
        {
            _logger.LogInformation("AccountService --> ValidateVerificationCode called");

            if (!_memoryCache.TryGetValue(email, out string storedCode) || storedCode != verificationCode)
            {
                _logger.LogWarning("AccountService --> ValidateVerificationCode --> Invalid or expired verification code");
                return (false, "Invalid or expired verification code.");
            }

            _logger.LogInformation("AccountService --> ValidateVerificationCode --> Verification code is valid");
            return (true, "Verification code is valid.");
        }

        public async Task<(bool Success, string Message , string? Token)> ResetPasswordAsync(string email, string verificationCode, string newPassword)
        {
            _logger.LogInformation("AccountService --> ResetPassword called");

            var validateResult = await ValidateVerificationCodeAsync(email, verificationCode);
            if (!validateResult.Success)
                return (validateResult.Success, validateResult.Message, null);  

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                _logger.LogInformation("AccountService --> ResetPassword --> user not found");
                return (false, "User not found.",null);
            }

            var resetPasswordtoken =  await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetPassResult = await _userManager.ResetPasswordAsync(user, resetPasswordtoken, newPassword);

            if (!resetPassResult.Succeeded)
            {
                var errors = string.Join(", ", resetPassResult.Errors.Select(e => e.Description));
                _logger.LogWarning("AccountService --> ResetPassword failed");
                return (false, "Password reset failed: " + errors,null);
            }
            var token = GenerateJwtToken(user);
            _logger.LogInformation("AccountService --> ResetPassword succeeded");
            return (true, "Password reset successful.",token);
        }
        private string GenerateJwtToken(ApplicationUser user)
        {
            _logger.LogInformation("AccountService --> GenerateJwtToken called");

            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user?.UserName??""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var userRoles = _userManager.GetRolesAsync(user!).Result;
            authClaims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            _logger.LogInformation("AccountService --> GenerateJwtToken succeeded");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

