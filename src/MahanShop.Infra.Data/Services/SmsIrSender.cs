using System.Net.Http.Json;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MahanShop.Infra.Data.Services;

/// <summary>ارسال OTP از طریق SMS.ir (verify/send با قالب تاییدشده). کلید از env. سرویس داخلی ایران — domestic-only.</summary>
public class SmsIrSender : ISmsSender
{
    private readonly HttpClient _http;
    private readonly SmsSettings _settings;
    private readonly ILogger<SmsIrSender> _logger;

    public SmsIrSender(HttpClient http, IOptions<SmsSettings> settings, ILogger<SmsIrSender> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendVerificationAsync(string phone, string code, CancellationToken ct = default)
    {
        var payload = new
        {
            mobile = phone,
            templateId = _settings.TemplateId,
            parameters = new[]
            {
                new { name = _settings.VerifyParameterName, value = code }
            }
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, "https://api.sms.ir/v1/send/verify")
        {
            Content = JsonContent.Create(payload)
        };
        req.Headers.Add("X-API-KEY", _settings.ApiKey);
        req.Headers.Add("Accept", "application/json");

        using var res = await _http.SendAsync(req, ct);
        if (!res.IsSuccessStatusCode)
        {
            // کد در لاگ نمی‌رود — فقط شماره و status
            _logger.LogError("SMS.ir send failed {Status} for {Phone}", res.StatusCode, phone);
            throw new InvalidOperationException("ارسال پیامک ناموفق بود.");
        }
    }
}
