using Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

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
            _logger.LogInformation("Attempting to send email to {ToEmail} with subject {Subject}", toEmail, subject);

            var apiKey = _configuration["SendGrid:ApiSenderKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                _logger.LogError("SendGrid API key is not configured.");
                throw new InvalidOperationException("SendGrid API key is not configured.");
            }

            _logger.LogInformation("SendGrid API key retrieved successfully.");

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("minhnhut.services.test@gmail.com", "ChatJoy");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);


            try
            {
                var response = await client.SendEmailAsync(msg);

                if (response.StatusCode != System.Net.HttpStatusCode.Accepted)
                {
                    var responseBody = await response.Body.ReadAsStringAsync();
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

        public async Task<string> GetEmailTemplate(string fullName, string confirmationLink)
        {
            var logoUrl = _configuration["EmailSettings:LogoUrl"];
            var supportLink = _configuration["EmailSettings:SupportLink"];
            var unsubscribeLink = _configuration["EmailSettings:UnsubscribeLink"];
            var unsubscribePreferencesLink = _configuration["EmailSettings:UnsubscribePreferencesLink"];

            var htmlTemplatePath = Path.Combine(Directory.GetCurrentDirectory(),"Helpers", "Template", "emailTemplate.html");
            var htmlTemplate = await File.ReadAllTextAsync(htmlTemplatePath);

            var template = htmlTemplate.Replace("{{fullName}}", fullName)
                                       .Replace("{{confirmationLink}}", confirmationLink)
                                       .Replace("{{logoUrl}}", logoUrl)
                                       .Replace("{{supportLink}}", supportLink)
                                       .Replace("{{unsubscribeLink}}", unsubscribeLink)
                                       .Replace("{{unsubscribePreferencesLink}}", unsubscribePreferencesLink);

            return template;
        }
    }
}
