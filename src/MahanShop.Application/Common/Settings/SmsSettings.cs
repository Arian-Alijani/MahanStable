namespace MahanShop.Application.Common.Settings;

/// <summary>تنظیمات سرویس پیامک (OTP) — SMS.ir. مقدار از env: Sms__ApiKey و ...</summary>
public class SmsSettings
{
    public const string SectionName = "Sms";

    public string ApiKey { get; set; } = string.Empty;

    /// <summary>شناسه قالب تاییدشده در پنل SMS.ir.</summary>
    public int TemplateId { get; set; }

    /// <summary>نام پارامتر کد در قالب SMS.ir (باید با placeholder قالب یکی باشد).</summary>
    public string VerifyParameterName { get; set; } = "Code";
}
