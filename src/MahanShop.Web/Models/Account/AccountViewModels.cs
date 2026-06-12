using System.ComponentModel.DataAnnotations;

namespace MahanShop.Web.Models.Account;

/// <summary>فرم مرحله اول ورود — شماره موبایل.</summary>
public class LoginViewModel
{
    [Display(Name = "شماره موبایل")]
    public string? Phone { get; set; }

    public string? ReturnUrl { get; set; }
}

/// <summary>فرم مرحله دوم — کد تایید (+ نام اختیاری برای ثبت‌نام اولیه).</summary>
public class VerifyViewModel
{
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "کد تایید")]
    public string? Code { get; set; }

    [Display(Name = "نام و نام خانوادگی")]
    public string? FullName { get; set; }

    public string? ReturnUrl { get; set; }

    /// <summary>ثانیه باقی‌مانده تا امکان ارسال مجدد.</summary>
    public int CooldownSeconds { get; set; }
}
