namespace Services.Interfaces;

public interface IEmailHelper
{
    Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml);
}