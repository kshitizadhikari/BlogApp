using BlogApp.Web.Data;
using BlogApp.Web.Infrastructure.Interfaces;
using BlogApp.Web.Models.Entities;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Options;

namespace BlogApp.Web.Infrastructure.Repositories
{
    public class EmailService : IEmailService
    {

        private readonly SmtpSettings _smtpSettings;
        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpSettings.FromAddress, _smtpSettings.FromName),
                Subject = subject,
                Body = message,
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            using var smtpClient = new SmtpClient(_smtpSettings.SmtpServer, _smtpSettings.SmtpPort)
            {
                Credentials = new NetworkCredential(_smtpSettings.SmtpUser, _smtpSettings.SmtpPassword),
                EnableSsl = _smtpSettings.UseSsl
            };

            await smtpClient.SendMailAsync(mailMessage);

        }
    }
}
