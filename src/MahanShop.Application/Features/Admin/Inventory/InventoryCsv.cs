using System.Globalization;
using System.Text;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Inventory;

/// <summary>
/// خروجی CSV از واریانت‌های فیلترشده. ستون‌ها: sku,product,brand,model,other,stock.
/// (هم‌راستا با فیلترهای صفحهٔ مدیریت موجودی.)
/// </summary>
public record ExportInventoryCsvQuery(
    string? Search = null,
    int? BrandValueId = null,
    string? Model = null,
    StockStatusFilter Status = StockStatusFilter.All) : IRequest<byte[]>;

public class ExportInventoryCsvQueryHandler : IRequestHandler<ExportInventoryCsvQuery, byte[]>
{
    private readonly IApplicationDbContext _db;
    public ExportInventoryCsvQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<byte[]> Handle(ExportInventoryCsvQuery request, CancellationToken ct)
    {
        var q = _db.ProductVariants.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.Trim();
            q = q.Where(v => v.Product.Title.Contains(term) || (v.Sku != null && v.Sku.Contains(term)));
        }
        if (request.BrandValueId is int brandId && brandId > 0)
            q = q.Where(v => v.Values.Any(x => x.AttributeValueId == brandId));
        if (!string.IsNullOrWhiteSpace(request.Model))
        {
            var m = request.Model.Trim();
            q = q.Where(v => v.Values.Any(x =>
                x.AttributeValue.Attribute.Kind == VariantAttributeKind.Model &&
                x.AttributeValue.Value.Contains(m)));
        }
        q = request.Status switch
        {
            StockStatusFilter.InStock => q.Where(v => v.Stock > 5),
            StockStatusFilter.Low => q.Where(v => v.Stock >= 1 && v.Stock <= 5),
            StockStatusFilter.Out => q.Where(v => v.Stock == 0),
            _ => q
        };

        var raw = await q
            .OrderBy(v => v.Product.Title).ThenBy(v => v.Id)
            .Select(v => new
            {
                v.Sku,
                Product = v.Product.Title,
                Values = v.Values
                    .OrderBy(x => x.AttributeValue.Attribute.DisplayOrder)
                    .Select(x => new { x.AttributeValue.Attribute.Kind, x.AttributeValue.Value })
                    .ToList(),
                v.Stock
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("sku,product,brand,model,other,stock");
        foreach (var v in raw)
        {
            var brand = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Brand)?.Value ?? "";
            var model = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Model)?.Value ?? "";
            var other = string.Join(" / ", v.Values
                .Where(x => x.Kind is VariantAttributeKind.Color or VariantAttributeKind.Other)
                .Select(x => x.Value));
            sb.Append(Csv(v.Sku)).Append(',')
              .Append(Csv(v.Product)).Append(',')
              .Append(Csv(brand)).Append(',')
              .Append(Csv(model)).Append(',')
              .Append(Csv(other)).Append(',')
              .Append(v.Stock.ToString(CultureInfo.InvariantCulture))
              .Append('\n');
        }

        // BOM تا اکسل فارسی را درست نشان دهد
        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        return bom.Concat(bytes).ToArray();
    }

    private static string Csv(string? value)
    {
        value ??= "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}

/// <summary>نتیجهٔ بارگذاری CSV موجودی.</summary>
public class ImportInventoryResult
{
    public int Updated { get; set; }
    public int NotFound { get; set; }
    public int Invalid { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// به‌روزرسانی انبوه موجودی از CSV. فقط ستون‌های sku و stock استفاده می‌شوند؛
/// تطبیق بر اساس SKU. ردیف‌های فاقد SKU یا با عدد نامعتبر نادیده/ثبت‌خطا می‌شوند.
/// </summary>
public record ImportInventoryCsvCommand(string CsvContent) : IRequest<ImportInventoryResult>;

public class ImportInventoryCsvCommandHandler : IRequestHandler<ImportInventoryCsvCommand, ImportInventoryResult>
{
    private readonly IApplicationDbContext _db;
    public ImportInventoryCsvCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ImportInventoryResult> Handle(ImportInventoryCsvCommand request, CancellationToken ct)
    {
        var result = new ImportInventoryResult();
        var content = request.CsvContent ?? "";
        // حذف BOM احتمالی
        content = content.TrimStart('\uFEFF');

        var lines = content.Replace("\r\n", "\n").Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return result;

        // تشخیص هدر: اگر سطر اول شامل sku/stock بود رد می‌شود
        var startIndex = 0;
        var header = ParseLine(lines[0]);
        int skuCol = 0, stockCol = 1;
        if (header.Any(h => h.Trim().Equals("sku", StringComparison.OrdinalIgnoreCase)))
        {
            skuCol = Array.FindIndex(header, h => h.Trim().Equals("sku", StringComparison.OrdinalIgnoreCase));
            var sc = Array.FindIndex(header, h => h.Trim().Equals("stock", StringComparison.OrdinalIgnoreCase));
            if (sc >= 0) stockCol = sc;
            startIndex = 1;
        }

        // map از SKU به stock جدید (آخرین مقدار برنده است)
        var updates = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var i = startIndex; i < lines.Length; i++)
        {
            var cells = ParseLine(lines[i]);
            if (cells.Length <= Math.Max(skuCol, stockCol)) { result.Invalid++; continue; }

            var sku = cells[skuCol].Trim();
            var stockStr = cells[stockCol].Trim();
            if (string.IsNullOrWhiteSpace(sku)) { result.Invalid++; continue; }
            if (!int.TryParse(stockStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock) || stock < 0)
            {
                result.Invalid++;
                if (result.Errors.Count < 20) result.Errors.Add($"موجودی نامعتبر برای SKU «{sku}»: «{stockStr}»");
                continue;
            }
            updates[sku] = stock;
        }

        if (updates.Count == 0) return result;

        var skus = updates.Keys.ToList();
        var variants = await _db.ProductVariants
            .Where(v => v.Sku != null && skus.Contains(v.Sku))
            .ToListAsync(ct);

        var found = new HashSet<string>(variants.Select(v => v.Sku!), StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;
        foreach (var v in variants)
        {
            if (updates.TryGetValue(v.Sku!, out var newStock))
            {
                v.Stock = newStock;
                v.UpdatedAt = now;
                result.Updated++;
            }
        }
        result.NotFound = updates.Keys.Count(k => !found.Contains(k));

        await _db.SaveChangesAsync(ct);
        return result;
    }

    /// <summary>پارس سادهٔ یک خط CSV با پشتیبانی از فیلدهای محصور در گیومه.</summary>
    private static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                    else inQuotes = false;
                }
                else sb.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
        }
        fields.Add(sb.ToString());
        return fields.ToArray();
    }
}
