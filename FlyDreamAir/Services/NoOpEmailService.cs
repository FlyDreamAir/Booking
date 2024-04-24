using System.Diagnostics;

namespace FlyDreamAir.Services;

public class NoOpEmailService : IEmailService
{
    public Task SendEmailAsync(string to, string subject, string body, string htmlBody)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }
        Console.WriteLine(to);
        Console.WriteLine(subject);
        Console.WriteLine(body);
        return Task.CompletedTask;
    }
}
