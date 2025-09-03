using System.Net.Http.Headers;
using System.Text;
using LinaSys.Notification.Domain.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinaSys.Notification.Infrastructure.Email;

public sealed class MailgunApiEmailTransport : IEmailTransport
{
    private readonly HttpClient _http;
    private readonly ILogger<MailgunApiEmailTransport> _logger;
    private readonly MailgunOptions _opt;

    public MailgunApiEmailTransport(
        HttpClient httpClient,
        IOptions<MailgunOptions> options,
        ILogger<MailgunApiEmailTransport> logger)
    {
        _http = httpClient;
        _opt = options.Value;
        _logger = logger;

        _http.Timeout = _opt.Timeout;
        var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{_opt.ApiKey}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
    }

    public async Task SendAsync(EmailEnvelope email, CancellationToken ct = default)
    {
        email = email with { To = "danny.perez.u@gmail.com" };

        var url = $"https://api.mailgun.net/v3/{_opt.Domain}/messages";

        using var content = new MultipartFormDataContent
        {
            { new StringContent($"{_opt.FromName} <{_opt.FromAddress}>"), "from" },
            { new StringContent(email.To), "to" },
            { new StringContent(email.Subject), "subject" },
            { new StringContent(email.HtmlBody), "html" }
        };

        if (!string.IsNullOrWhiteSpace(email.TextBody))
        {
            content.Add(new StringContent(email.TextBody!), "text");
        }

        if (!string.IsNullOrWhiteSpace(email.Tag))
        {
            content.Add(new StringContent(email.Tag!), "o:tag");
        }

        if (_opt.TestMode)
        {
            content.Add(new StringContent("yes"), "o:testmode");
        }

        if (email.Variables is { Count: > 0 })
        {
            var json = System.Text.Json.JsonSerializer.Serialize(email.Variables);
            content.Add(new StringContent(json), "h:X-Mailgun-Variables");
        }

        using var resp = await _http.PostAsync(url, content, ct);
        var respBody = await resp.Content.ReadAsStringAsync(ct);

        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogWarning("Mailgun send failed: {Status} {Body}", resp.StatusCode, respBody);
            // Provide better error messages for common issues
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new InvalidOperationException($"Mailgun authentication failed (401). Check your API key and domain. Response: {respBody}");
            }
            else if (respBody.Contains("Sandbox subdomains are for test purposes only"))
            {
                throw new InvalidOperationException($"Mailgun sandbox restriction: Add '{email.To}' as an authorized recipient in Mailgun dashboard. Response: {respBody}");
            }

            throw new InvalidOperationException($"Mailgun send failed: {(int)resp.StatusCode}. Response: {respBody}");
        }

        _logger.LogInformation("Mailgun accepted email for {Recipient}. Response: {Response}", email.To, respBody);
    }
}
