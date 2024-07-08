using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace ChatApp_BE.Helpers
{
    public class IEmailSenders
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<IEmailSenders> _logger;

        public IEmailSenders(IConfiguration configuration, HttpClient httpClient, ILogger<IEmailSenders> logger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<string> GetEmailTemplate(string fullName, string confirmationLink)
        {
            var logoUrl = _configuration["EmailSettings:LogoUrl"];
            var supportLink = _configuration["EmailSettings:SupportLink"];
            var unsubscribeLink = _configuration["EmailSettings:UnsubscribeLink"];
            var unsubscribePreferencesLink = _configuration["EmailSettings:UnsubscribePreferencesLink"];

            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var htmlTemplatePath = Path.Combine(basePath, "Helpers", "Template", "emailTemplate.html");

            if (!File.Exists(htmlTemplatePath))
            {
                throw new FileNotFoundException($"The template file was not found at path: {htmlTemplatePath}");
            }

            var htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            var template = htmlTemplate.Replace("{{fullName}}", fullName)
                                       .Replace("{{confirmationLink}}", confirmationLink)
                                       .Replace("{{logoUrl}}", logoUrl)
                                       .Replace("{{supportLink}}", supportLink)
                                       .Replace("{{unsubscribeLink}}", unsubscribeLink)
                                       .Replace("{{unsubscribePreferencesLink}}", unsubscribePreferencesLink);

            return template;
        }

        public async Task SendEmailAsync(string subject, string toEmail, string message)
        {
            var apiKey = _configuration["SendGrid:ApiSenderKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API key is not configured.");
                throw new System.Exception("SendGrid API key is not configured.");
            }

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("minhnhut.services.test@gmail.com", "ChatJoy");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            try
            {
                var response = await client.SendEmailAsync(msg);
                var responseBody = await response.Body.ReadAsStringAsync();

                _logger.LogInformation("SendGrid response status code: {StatusCode}", response.StatusCode);
                _logger.LogInformation("SendGrid response body: {ResponseBody}", responseBody);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    throw new System.Exception($"Failed to send email. StatusCode: {response.StatusCode}, ResponseBody: {responseBody}");
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
