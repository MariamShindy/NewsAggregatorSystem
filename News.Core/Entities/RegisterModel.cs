﻿namespace News.Core.Entities
{
	public class RegisterModel
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string? ProfilePicUrl { get; set; }
		public string Password { get; set; }
	}
}
