﻿namespace News.Core.Dtos
{
    public class RegisterDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? profilePicUrl {  get; set; } = string.Empty;
        //public string ConfirmPassword { get; set; }  = String.Empty;
    }
}
