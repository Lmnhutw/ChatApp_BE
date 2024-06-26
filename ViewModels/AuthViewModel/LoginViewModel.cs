namespace ChatApp_BE.ViewModels.AuthViewModel
{
    public class LoginViewModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public bool RememberMe { get; set; }
    }
}