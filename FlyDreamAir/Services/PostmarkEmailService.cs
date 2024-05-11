using PostmarkDotNet;

namespace FlyDreamAir.Services;

public class PostmarkEmailService : IEmailService
{
    private readonly PostmarkClient _client;
    private readonly string _adminEmail;
    private readonly string _template;

    public PostmarkEmailService(
        IConfiguration configuration,
        IWebHostEnvironment webHostEnvironment,
        PostmarkClient client
    )
    {
        _client = client;
        _adminEmail = configuration["Admin:Email:Address"]
            ?? "FlyDreamAir Administrator <admin@fly.trungnt2910.com>";
        var templateConfig = configuration["Admin:Email:Template"];
        if (!string.IsNullOrEmpty(templateConfig))
        {
            var templatePath = Path.Combine(webHostEnvironment.WebRootPath, templateConfig);
            if (File.Exists(templatePath))
            {
                _template = File.ReadAllText(templatePath);
            }
            else
            {
                _template = templateConfig;
            }
        }
        else
        {
            _template = "{{Body}}";
        }
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
                HtmlBody = _template
                    .Replace("{{Subject}}", subject)
                    .Replace("{{Body}}", htmlBody)
            }
        );
    }
}
