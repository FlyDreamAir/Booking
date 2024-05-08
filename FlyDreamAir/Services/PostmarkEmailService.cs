﻿using PostmarkDotNet;

namespace FlyDreamAir.Services;

public class PostmarkEmailService : IEmailService
{
    private readonly PostmarkClient _client;
    private readonly string _adminEmail;

    public PostmarkEmailService(IConfiguration configuration, PostmarkClient client)
    {
        _client = client;
        _adminEmail = configuration["Admin:Email"] ?? "admin@fly.trungnt2910.com";
    }

    public async Task SendEmailAsync(string to, string subject, string body, string htmlBody)
    {
        await _client.SendMessageAsync(
            new PostmarkMessage()
            {
                To = to,
                From = _adminEmail,
                TrackOpens = true,
                Subject = subject,
                TextBody = body,
                HtmlBody = htmlBody
            }
        );
    }
}
