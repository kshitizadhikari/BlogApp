
using BlogApp.Web.Models.Entities;

namespace BlogApp.Web.Infrastructure.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
