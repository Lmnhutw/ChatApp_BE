using System.Threading.Tasks;

namespace ChatApp_BE.Helpers
{
    public interface IEmailSenders
    {
        Task SendEmailAsync(string subject, string toEmail, string message);
    }
}