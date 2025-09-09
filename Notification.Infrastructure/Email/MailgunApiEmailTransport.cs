using System.Diagnostics;
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
        // TEMP override (?) – consider removing when done debugging.
        email = email with { To = "danny.perez.u@gmail.com" };

        var url = $"https://api.mailgun.net/v3/{_opt.Domain}/messages";

        var sw = Stopwatch.StartNew();
        _logger.LogDebug("[Mailgun] Start send. To={To} Subject={Subject} Tag={Tag} TestMode={TestMode} Domain={Domain}", email.To, email.Subject, email.Tag ?? "<none>", _opt.TestMode, _opt.Domain);

        try
        {
            using var content = new MultipartFormDataContent();

            void AddContent(HttpContent httpContent, string name, string? traceValue = null)
            {
                content.Add(httpContent, name);
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("[Mailgun] Added form part '{Part}' ValuePreview='{Preview}'", name, traceValue is null ? "<binary or hidden>" : (traceValue.Length > 120 ? traceValue[..120] + "…" : traceValue));
                }
            }

            AddContent(new StringContent($"{_opt.FromName} <{_opt.FromAddress}>"), "from", $"{_opt.FromName} <{_opt.FromAddress}>");
            AddContent(new StringContent(email.To), "to", email.To);
            AddContent(new StringContent(email.Subject), "subject", email.Subject);
            AddContent(new StringContent(email.HtmlBody), "html", "<html body>");

            if (!string.IsNullOrWhiteSpace(email.TextBody))
            {
                AddContent(new StringContent(email.TextBody!), "text", email.TextBody);
            }

            if (!string.IsNullOrWhiteSpace(email.Tag))
            {
                AddContent(new StringContent(email.Tag!), "o:tag", email.Tag);
            }

            if (_opt.TestMode)
            {
                AddContent(new StringContent("yes"), "o:testmode", "yes");
            }

            if (email.Variables is { Count: > 0 })
            {
                var json = System.Text.Json.JsonSerializer.Serialize(email.Variables);
                AddContent(new StringContent(json), "h:X-Mailgun-Variables", json);
            }

            _logger.LogDebug("[Mailgun] Prepared multipart form with {Count} parts in {Elapsed} ms", content.Count(), sw.ElapsedMilliseconds);

            ct.ThrowIfCancellationRequested();
            _logger.LogDebug("[Mailgun] Sending HTTP POST to {Url}", url);

            using var resp = await _http.PostAsync(url, content, ct);
            var respBody = await resp.Content.ReadAsStringAsync(ct);
            var elapsed = sw.Elapsed;

            if (!resp.IsSuccessStatusCode)
            {
                _logger.LogWarning("[Mailgun] Send failed after {Elapsed} ms. Status={Status} Body={Body}", elapsed.TotalMilliseconds, resp.StatusCode, Truncate(respBody));

                // Provide better error messages for common issues
                if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new InvalidOperationException($"Mailgun authentication failed (401). Check your API key and domain. Response: {respBody}");
                }
                else if (respBody.Contains("Sandbox subdomains are for test purposes only", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Mailgun sandbox restriction: Add '{email.To}' as an authorized recipient in Mailgun dashboard. Response: {respBody}");
                }

                throw new InvalidOperationException($"Mailgun send failed: {(int)resp.StatusCode}. Response: {respBody}");
            }

            string? id = null;
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(respBody);
                if (doc.RootElement.TryGetProperty("id", out var idProp))
                {
                    id = idProp.GetString();
                }
            }
            catch (Exception parseEx)
            {
                _logger.LogDebug(parseEx, "[Mailgun] Unable to parse response id");
            }

            _logger.LogInformation("[Mailgun] Accepted email for {Recipient} in {Elapsed} ms. Status={Status} Id={Id} ResponsePreview={Preview}", email.To, elapsed.TotalMilliseconds, resp.StatusCode, id ?? "<none>", Truncate(respBody));
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            _logger.LogWarning("[Mailgun] Send cancelled after {Elapsed} ms. To={To} Subject={Subject}", sw.ElapsedMilliseconds, email.To, email.Subject);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Mailgun] Unexpected error sending email To={To} Subject={Subject} Elapsed={Elapsed} ms", email.To, email.Subject, sw.ElapsedMilliseconds);
            throw;
        }
        finally
        {
            sw.Stop();
        }
    }

    private static string Truncate(string value, int max = 400)
        => string.IsNullOrEmpty(value) ? value : (value.Length <= max ? value : value[..max] + "…");
}
