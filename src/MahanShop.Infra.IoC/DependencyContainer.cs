using MahanShop.Application;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Common.Settings;
using MahanShop.Infra.Data.Context;
using MahanShop.Infra.Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MahanShop.Infra.IoC;

/// <summary>نقطه مرکزی سیم‌کشی DI همه لایه‌ها. از Program.cs صدا زده می‌شه.</summary>
public static class DependencyContainer
{
    public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Database — connection string از env: ConnectionStrings__EShop_PhoneDb
        services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("EShop_PhoneDb"),
                sql => sql.MigrationsAssembly(typeof(MyDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<MyDbContext>());

        // Options — مقادیر از env (Sms__ApiKey, Zarinpal__MerchantId, Otp__Pepper). هیچ secret در سورس.
        services.Configure<SmsSettings>(configuration.GetSection(SmsSettings.SectionName));
        services.Configure<ZarinpalSettings>(configuration.GetSection(ZarinpalSettings.SectionName));
        services.Configure<OtpSettings>(configuration.GetSection(OtpSettings.SectionName));

        // لایه Application — MediatR + Validatorها
        services.AddApplicationLayer();

        // Auth/OTP — هش با HMAC (Pepper از env)
        services.AddSingleton<IOtpHasher, OtpHasher>();

        // SMS — بدون ApiKey (dev) → Fake؛ با کلید → SMS.ir واقعی (سرویس داخلی)
        var smsApiKey = configuration.GetSection(SmsSettings.SectionName)["ApiKey"];
        if (string.IsNullOrWhiteSpace(smsApiKey))
        {
            services.AddScoped<ISmsSender, FakeSmsSender>();
        }
        else
        {
            services.AddHttpClient<ISmsSender, SmsIrSender>(c =>
                c.Timeout = TimeSpan.FromSeconds(15));
        }

        // Payment — درگاه Zarinpal (سرویس داخلی). MerchantId از env؛ خالی → درخواست با خطای امن برمی‌گردد.
        services.AddHttpClient<IPaymentGateway, ZarinpalGateway>(c =>
            c.Timeout = TimeSpan.FromSeconds(20));

        return services;
    }
}
