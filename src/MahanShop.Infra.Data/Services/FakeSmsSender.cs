using MahanShop.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace MahanShop.Infra.Data.Services;

/// <summary>پیامک جعلی برای dev (بدون کلید). کد را در لاگ سرور چاپ می‌کند — هرگز در production استفاده نشود.</summary>
public class FakeSmsSender : ISmsSender
{
    private readonly ILogger<FakeSmsSender> _logger;

    public FakeSmsSender(ILogger<FakeSmsSender> logger) => _logger = logger;

    public Task SendVerificationAsync(string phone, string code, CancellationToken ct = default)
    {
        _logger.LogWarning("[FakeSms] OTP for {Phone} = {Code}", phone, code);
        return Task.CompletedTask;
    }
}
