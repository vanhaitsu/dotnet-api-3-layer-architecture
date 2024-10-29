using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHTML)
    {
        var mailServer = _configuration["EmailSettings:MailServer"]!;
        var fromEmail = _configuration["EmailSettings:FromEmail"]!;
        var password = _configuration["EmailSettings:Password"]!;
        var port = int.Parse(_configuration["EmailSettings:MailPort"]!);
        var client = new SmtpClient(mailServer, port)
        {
            Credentials = new NetworkCredential(fromEmail, password),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(fromEmail, toEmail, subject, body)
        {
            IsBodyHtml = isBodyHTML
        };

        return client.SendMailAsync(mailMessage);
    }
}