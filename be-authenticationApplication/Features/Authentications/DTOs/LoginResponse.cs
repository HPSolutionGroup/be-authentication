namespace be_authenticationApplication.Features.Authentications.DTOs
{
    public class LoginResponse
    {
        public string token { get; set; }
        public string refreshToken { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string branchName { get; set; }
        public string? Avatar { get; set; }
    }
}
