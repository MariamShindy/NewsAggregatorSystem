﻿using Microsoft.AspNetCore.Http;

namespace News.Core.Dtos
{
    public class EditUserDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; } 
        public IFormFile? ProfilePicUrl { get; set; }
        public string? Username { get; set; } 
        public string? Password { get; set; } 
        public string? Email { get; set; } 
    }
}
