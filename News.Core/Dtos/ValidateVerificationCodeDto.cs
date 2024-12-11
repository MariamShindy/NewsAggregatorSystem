namespace News.Core.Dtos
{
    public class ValidateVerificationCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
