using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Helpers;

public class EmailHelper : IEmailHelper
{
    private readonly IConfiguration _configuration;

    public EmailHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml)
    {
        var host = _configuration["EmailSettings:Host"];
        int.TryParse(_configuration["EmailSettings:Port"], out var port);
        var from = _configuration["EmailSettings:From"];
        ArgumentException.ThrowIfNullOrWhiteSpace(from);
        var password = _configuration["EmailSettings:Password"];
        var client = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(from, password),
            EnableSsl = true
        };
        var mailMessage = new MailMessage(from, to, subject, body)
        {
            IsBodyHtml = isBodyHtml
        };

        return client.SendMailAsync(mailMessage);
    }
}