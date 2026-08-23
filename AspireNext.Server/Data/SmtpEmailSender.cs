using AspireNext.Server.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MimeKit;

namespace AspireNext.Server.Data;

public class SmtpEmailSender(IOptions<SmtpOptions> options, IConfiguration configuration) : IEmailSender<ApplicationUser>
{
    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        SendEmailAsync(email, "Confirm your email", ConfirmationBody(BuildFrontendLink(confirmationLink, "confirm-email")));

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        SendEmailAsync(email, "Reset your password", ResetBody(BuildFrontendLink(resetLink, "reset-password")));

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        var link = $"{GetFrontendBaseUrl()}/reset-password?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(resetCode)}";
        return SendEmailAsync(email, "Reset your password", ResetBody(link));
    }

    // The framework builds confirmationLink/resetLink pointing at our own API host with the
    // right query params (userId/code) already attached - swap in the frontend's host/path so
    // users land on a styled page instead of a bare API response, keeping the query string as-is.
    private string BuildFrontendLink(string originalLink, string frontendPath) =>
        $"{GetFrontendBaseUrl()}/{frontendPath}{new Uri(originalLink).Query}";

    private string GetFrontendBaseUrl() =>
        configuration["services:frontend:http:0"] ??
        configuration["services:frontend:https:0"] ??
        throw new InvalidOperationException("Frontend base URL is not configured.");

    private static string ConfirmationBody(string link) =>
        $"<p>Welcome to Wall Art Canvases! Please confirm your account by <a href=\"{link}\">clicking here</a>.</p>";

    private static string ResetBody(string link) =>
        $"<p>We received a request to reset your password. <a href=\"{link}\">Click here to choose a new one</a>. If you didn't request this, you can ignore this email.</p>";

    private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(options.Value.FromName, options.Value.FromAddress));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(options.Value.Host, options.Value.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(options.Value.Username, options.Value.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
