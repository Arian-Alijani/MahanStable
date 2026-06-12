using MahanShop.Application.Features.Account.Queries.GetOrderDetail;
using MahanShop.Web.Helpers;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace MahanShop.Web.Services;

/// <summary>تولید فاکتور PDF سفارش با QuestPDF — فونت Vazir self-host، RTL. domestic-only (هیچ منبع خارجی).</summary>
public class InvoicePdfService
{
    private readonly string _fontName;

    public InvoicePdfService(IWebHostEnvironment env)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        // ثبت فونت Vazir محلی (یک‌بار کافی است، اما idempotent امن است)
        _fontName = "Vazir";
        var fontPath = Path.Combine(env.WebRootPath, "fonts", "vazirmatn", "Vazir.ttf");
        if (File.Exists(fontPath))
        {
            using var fs = File.OpenRead(fontPath);
            FontManager.RegisterFont(fs);
        }
        else
        {
            _fontName = Fonts.Calibri; // fallback اگر فونت پیدا نشد
        }
    }

    public byte[] Build(OrderDetailDto o)
    {
        string Money(long t) => t.ToString("#,0") + " تومان";

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(t => t.FontFamily(_fontName).FontSize(11).DirectionFromRightToLeft());

                page.Header().Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("ماهان شاپ").FontSize(20).Bold().FontColor("#2563EB");
                        r.ConstantItem(220).AlignLeft().Text($"فاکتور سفارش\n{o.OrderCode}").FontSize(12).Bold();
                    });
                    col.Item().PaddingTop(4).LineHorizontal(1).LineColor("#2563EB");
                });

                page.Content().PaddingVertical(12).Column(col =>
                {
                    col.Spacing(8);

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"وضعیت: {OrderStatusView.Label(o.Status)}");
                        r.RelativeItem().Text($"تاریخ ثبت: {o.CreatedAt:yyyy/MM/dd}");
                        if (o.PaidAt is not null)
                            r.RelativeItem().Text($"تاریخ پرداخت: {o.PaidAt:yyyy/MM/dd}");
                    });

                    col.Item().Text($"مشتری: {o.CustomerName}  -  {o.CustomerPhone}");

                    if (o.FullAddress is not null)
                    {
                        col.Item().Text($"تحویل‌گیرنده: {o.ReceiverName} ({o.ReceiverPhone})");
                        col.Item().Text($"آدرس: {o.Province}، {o.City} — {o.FullAddress} — کد پستی {o.PostalCode}");
                    }

                    if (!string.IsNullOrEmpty(o.TrackingCode))
                        col.Item().Text($"کد رهگیری: {o.TrackingCode}");

                    col.Item().PaddingTop(6).Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(4);
                            c.RelativeColumn(2);
                            c.RelativeColumn(1);
                            c.RelativeColumn(2);
                        });

                        table.Header(h =>
                        {
                            void HCell(string t) => h.Cell().Background("#EFF6FF").Padding(6).Text(t).Bold();
                            HCell("محصول");
                            HCell("قیمت واحد");
                            HCell("تعداد");
                            HCell("جمع");
                        });

                        foreach (var it in o.Items)
                        {
                            void Cell(string t) => table.Cell().BorderBottom(1).BorderColor("#E5E7EB").Padding(6).Text(t);
                            Cell(it.ProductTitle);
                            Cell(Money(it.UnitPrice));
                            Cell(it.Quantity.ToString());
                            Cell(Money(it.LineTotal));
                        }
                    });

                    col.Item().AlignLeft().Width(220).Column(s =>
                    {
                        s.Spacing(3);
                        void Row(string l, string v, bool bold = false)
                        {
                            s.Item().Row(r =>
                            {
                                var left = r.RelativeItem().Text(l);
                                if (bold) left.Bold();
                                var right = r.ConstantItem(110).AlignLeft().Text(v);
                                if (bold) right.Bold();
                            });
                        }
                        Row("جمع کل", Money(o.TotalAmount));
                        if (o.DiscountAmount > 0) Row("تخفیف", "- " + Money(o.DiscountAmount));
                        Row("هزینه ارسال", Money(o.ShippingCost));
                        s.Item().LineHorizontal(1).LineColor("#E5E7EB");
                        Row("مبلغ پرداختی", Money(o.FinalAmount), true);
                    });
                });

                page.Footer().AlignCenter().Text("ماهان شاپ — این فاکتور به‌صورت الکترونیکی صادر شده است.")
                    .FontSize(9).FontColor("#9CA3AF");
            });
        }).GeneratePdf();
    }
}
