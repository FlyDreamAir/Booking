using FlyDreamAir.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PostmarkDotNet;

namespace FlyDreamAir.Components.Account;

internal sealed class IdentityPostmarkEmailSender : IEmailSender<ApplicationUser>
{
    private readonly PostmarkClient _client;
    private readonly string _adminEmail;

    public IdentityPostmarkEmailSender(IConfiguration configuration, PostmarkClient client)
    {
        _client = client;
        _adminEmail = configuration["Email"] ?? "admin@fly.trungnt2910.com";
    }

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
        _client.SendMessageAsync(
            new PostmarkMessage()
            {
                To = email,
                From = _adminEmail,
                TrackOpens = true,
                Subject = "Confirm your email",
                TextBody = $"Please confirm your account by clicking here: {confirmationLink}",
                HtmlBody = $"Please confirm your account by <a href='{confirmationLink}'>clicking here</a>."
            }
        );

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
        _client.SendMessageAsync(
            new PostmarkMessage()
            {
                To = email,
                From = _adminEmail,
                TrackOpens = true,
                Subject = "Reset your password",
                TextBody = $"Please reset your password by clicking here: {resetLink}",
                HtmlBody = $"Please reset your password by <a href='{resetLink}'>clicking here</a>."
            }
        );

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
        _client.SendMessageAsync(
            new PostmarkMessage()
            {
                To = email,
                From = _adminEmail,
                TrackOpens = true,
                Subject = "Reset your password",
                TextBody = $"Please reset your password using the following code: {resetCode}",
                HtmlBody = $"Please reset your password using the following code: {resetCode}"
            }
        );
}
