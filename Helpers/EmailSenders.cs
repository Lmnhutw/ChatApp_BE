﻿using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChatApp_BE.Helpers
{
    public class EmailSenders : IEmailSenders
    {
        private readonly IConfiguration _configuration;

        public EmailSenders(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string subject, string toEmail, string message)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("minhnhut.services.test@gmail.com", "ChapApp");
            var to = new EmailAddress(toEmail);
            var plainTextContent = message;
            var htmlContent = message;
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);
        }
    }
}