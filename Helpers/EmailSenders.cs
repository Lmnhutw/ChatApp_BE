using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace ChatApp_BE.Helpers
{
    public class IEmailSenders
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IEmailSenders> _logger;

        public IEmailSenders(IConfiguration configuration, ILogger<IEmailSenders> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string subject, string toEmail, string message)
        {
            var apiKey = _configuration["SendGrid:ApiSenderKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API key is not configured.");
                throw new InvalidOperationException("SendGrid API key is not configured.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("minhnhut.services.test@gmail.com", "ChapApp");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);
                if (response.StatusCode >= System.Net.HttpStatusCode.BadRequest)
                {
                    var errorMessage = await response.Body.ReadAsStringAsync();
                    _logger.LogError("Failed to send email. StatusCode: {StatusCode}, Error: {Error}", response.StatusCode, errorMessage);
                    throw new InvalidOperationException($"Failed to send email. StatusCode: {response.StatusCode}, Error: {errorMessage}");
                }
                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while sending email to {Email}", toEmail);
                throw;
            }
        }
    }
}
