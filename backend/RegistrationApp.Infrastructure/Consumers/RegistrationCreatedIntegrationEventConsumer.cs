using System;
using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RegistrationApp.Infrastructure.IntegrationEvents;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace RegistrationApp.Infrastructure.Consumers;

public class RegistrationCreatedIntegrationEventConsumer : IConsumer<RegistrationCreatedIntegrationEvent>
{
    private readonly ILogger<RegistrationCreatedIntegrationEventConsumer> _logger;
    private readonly IConfiguration _configuration;

    public RegistrationCreatedIntegrationEventConsumer(
        ILogger<RegistrationCreatedIntegrationEventConsumer> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task Consume(ConsumeContext<RegistrationCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing integration event for Registration ID: {RegistrationId}", message.Id);

        // 1. Fetch SMTP configuration values from appsettings
        var fromAddress = _configuration["Email:FromAddress"] ?? "mtmhggalfa@gmail.com";
        var fromName = _configuration["Email:FromName"] ?? "Manar";
        var host = _configuration["Email:Smtp:Host"] ?? "smtp.gmail.com";
        var portStr = _configuration["Email:Smtp:Port"] ?? "587";
        var username = _configuration["Email:Smtp:Username"] ?? "mtmhggalfa@gmail.com";
        var password = _configuration["Email:Smtp:Password"] ?? "";
        var securityStr = _configuration["Email:Smtp:Security"] ?? "StartTls";

        int port = int.TryParse(portStr, out var p) ? p : 587;

        _logger.LogInformation("Preparing welcome email for {Email} via SMTP Host {Host}:{Port}", message.Email, host, port);

        try
        {
            // 2. Construct the HTML Mail Message
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(fromName, fromAddress));
            emailMessage.To.Add(new MailboxAddress($"{message.FirstName} {message.LastName}", message.Email));
            emailMessage.Subject = "Welcome to 3S Group - Registration Complete!";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
                    <div style='font-family: sans-serif; max-width: 600px; margin: 0 auto; border: 1px solid #e0e0e0; border-radius: 8px; overflow: hidden; box-shadow: 0 4px 10px rgba(0,0,0,0.05);'>
                        <div style='background: linear-gradient(135deg, #6366f1 0%, #4f46e5 100%); padding: 30px; text-align: center; color: white;'>
                            <h1 style='margin: 0; font-size: 24px; font-weight: 700; letter-spacing: 0.5px;'>Registration Complete!</h1>
                            <p style='margin: 5px 0 0 0; opacity: 0.9;'>Welcome to 3S Group Secure Profile System</p>
                        </div>
                        <div style='padding: 30px; color: #333; line-height: 1.6;'>
                            <h2 style='color: #4f46e5; margin-top: 0;'>Hello {message.FirstName} {message.LastName},</h2>
                            <p>We are excited to let you know that your user registration profile has been successfully saved in our SQL Server database using EF Core persistence.</p>
                            
                            <div style='background-color: #f8fafc; border: 1px solid #f1f5f9; border-radius: 6px; padding: 20px; margin: 25px 0;'>
                                <h3 style='margin: 0 0 10px 0; font-size: 14px; text-transform: uppercase; color: #64748b; letter-spacing: 1px;'>Profile Record Details</h3>
                                <table style='width: 100%; border-collapse: collapse;'>
                                    <tr>
                                        <td style='padding: 6px 0; font-weight: 600; color: #475569; width: 140px;'>Registration ID:</td>
                                        <td style='padding: 6px 0; font-family: monospace; font-size: 14px; color: #6366f1; font-weight: 700;'>{message.Id}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 6px 0; font-weight: 600; color: #475569;'>Email Address:</td>
                                        <td style='padding: 6px 0; color: #334155;'>{message.Email}</td>
                                    </tr>
                                    <tr>
                                        <td style='padding: 6px 0; font-weight: 600; color: #475569;'>Mobile Number:</td>
                                        <td style='padding: 6px 0; color: #334155;'>{message.MobileNumber}</td>
                                    </tr>
                                </table>
                            </div>
                            
                            <p>An integration event was securely dispatched via <strong>MassTransit Outbox Pattern</strong> to coordinate this welcome email.</p>
                            <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 30px 0;' />
                            <p style='margin: 0; color: #64748b; font-size: 13px;'>If you did not initiate this action, please ignore this email or contact support.</p>
                            <p style='margin: 5px 0 0 0; color: #64748b; font-size: 13px; font-weight: 600;'>3S Group Security Infrastructure</p>
                        </div>
                    </div>"
            };
            emailMessage.Body = bodyBuilder.ToMessageBody();

            // 3. Connect and send email via SMTP using MailKit
            using var client = new SmtpClient();

            // Match socket security settings
            var security = SecureSocketOptions.Auto;
            if (string.Equals(securityStr, "StartTls", StringComparison.OrdinalIgnoreCase))
                security = SecureSocketOptions.StartTls;
            else if (string.Equals(securityStr, "SslOnConnect", StringComparison.OrdinalIgnoreCase))
                security = SecureSocketOptions.SslOnConnect;
            else if (string.Equals(securityStr, "None", StringComparison.OrdinalIgnoreCase))
                security = SecureSocketOptions.None;

            await client.ConnectAsync(host, port, security);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password);
            }

            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Real welcome email successfully dispatched via SMTP to {Email}.", message.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {Email}", message.Email);
            throw; // Re-throw to let MassTransit outbox retry loop handle failure recovery
        }
    }
}
