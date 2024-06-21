using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChatApp_BE.Helpers
{
    public class IEmailSenders
    {
        private readonly IConfiguration _configuration;

        public IEmailSenders(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string subject, string toEmail, string message)
        {
            var apiKey = _configuration["SendGrid:ApiSenderKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("minhnhut.services.test@gmail.com", "ChapApp");
            var to = new EmailAddress(toEmail);
            //var plainTextContent = message;
            //var htmlContent = message;
            var templateId = _configuration["SendGrid:TemplateId"];
            var templateData = new Dictionary<string, object>
            {
                {"TKey1", "TValue1" },
            };
            var msg = MailHelper.CreateSingleTemplateEmail(from, to, templateId, templateData);
            var response = await client.SendEmailAsync(msg);
        }
    }
}