using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MahanShop.Infra.Data.Services;

/// <summary>درگاه پرداخت Zarinpal (REST v4). MerchantId از env. سرویس داخلی ایران — domestic-only. مبلغ به ریال (تومان×10) ارسال می‌شود.</summary>
public class ZarinpalGateway : IPaymentGateway
{
    private readonly HttpClient _http;
    private readonly ZarinpalSettings _settings;
    private readonly ILogger<ZarinpalGateway> _logger;

    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public ZarinpalGateway(HttpClient http, IOptions<ZarinpalSettings> settings, ILogger<ZarinpalGateway> logger)
    {
        _http = http;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentRequestResult> RequestAsync(long amountToman, string callbackUrl, string description, CancellationToken ct = default)
    {
        var payload = new
        {
            merchant_id = _settings.MerchantId,
            amount = ToRial(amountToman),
            callback_url = callbackUrl,
            description
        };

        try
        {
            using var res = await _http.PostAsJsonAsync(_settings.RequestUrl, payload, ct);
            var body = await res.Content.ReadFromJsonAsync<ZarinpalResponse>(JsonOpts, ct);

            if (body?.Data is { Code: 100 } d && !string.IsNullOrEmpty(d.Authority))
                return new PaymentRequestResult(true, d.Authority, _settings.StartPayUrl(d.Authority), null);

            _logger.LogError("Zarinpal request failed: http={Status} code={Code}", res.StatusCode, body?.Data?.Code);
            return new PaymentRequestResult(false, null, null, "درخواست پرداخت ناموفق بود.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zarinpal request exception");
            return new PaymentRequestResult(false, null, null, "ارتباط با درگاه پرداخت برقرار نشد.");
        }
    }

    public async Task<PaymentVerifyResult> VerifyAsync(long amountToman, string authority, CancellationToken ct = default)
    {
        var payload = new
        {
            merchant_id = _settings.MerchantId,
            amount = ToRial(amountToman),
            authority
        };

        try
        {
            using var res = await _http.PostAsJsonAsync(_settings.VerifyUrl, payload, ct);
            var body = await res.Content.ReadFromJsonAsync<ZarinpalResponse>(JsonOpts, ct);

            var code = body?.Data?.Code;
            if (code == 100)
                return new PaymentVerifyResult(true, body!.Data!.RefId?.ToString(), false, null);
            if (code == 101)
                return new PaymentVerifyResult(true, body!.Data!.RefId?.ToString(), true, null);

            _logger.LogError("Zarinpal verify failed: http={Status} code={Code}", res.StatusCode, code);
            return new PaymentVerifyResult(false, null, false, "تایید پرداخت ناموفق بود.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Zarinpal verify exception");
            return new PaymentVerifyResult(false, null, false, "ارتباط با درگاه پرداخت برقرار نشد.");
        }
    }

    /// <summary>تومان → ریال (Zarinpal مبلغ را به ریال می‌گیرد).</summary>
    private static long ToRial(long toman) => toman * 10;

    private sealed class ZarinpalResponse
    {
        [JsonPropertyName("data")] public ZarinpalData? Data { get; set; }
    }

    private sealed class ZarinpalData
    {
        [JsonPropertyName("code")] public int Code { get; set; }
        [JsonPropertyName("authority")] public string? Authority { get; set; }
        [JsonPropertyName("ref_id")] public long? RefId { get; set; }
    }
}
