﻿namespace News.Core.Dtos
{
    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string ProfilePicUrl { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public bool IsLockedOut { get; set; }
    }
}
