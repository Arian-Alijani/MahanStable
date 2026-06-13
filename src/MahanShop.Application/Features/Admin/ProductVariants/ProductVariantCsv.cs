using System.Globalization;
using System.Text;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.ProductVariants;

/// <summary>خروجی CSV واریانت‌های یک محصول. ستون‌ها: brand,model,color,stock,sku.</summary>
public record ExportProductVariantsCsvQuery(int ProductId) : IRequest<byte[]>;

public class ExportProductVariantsCsvQueryHandler : IRequestHandler<ExportProductVariantsCsvQuery, byte[]>
{
    private readonly IApplicationDbContext _db;
    public ExportProductVariantsCsvQueryHandler(IApplicationDbContext db) => _db = db;

    public async Task<byte[]> Handle(ExportProductVariantsCsvQuery request, CancellationToken ct)
    {
        var rows = await _db.ProductVariants.AsNoTracking()
            .Where(v => v.ProductId == request.ProductId)
            .OrderBy(v => v.DisplayOrder).ThenBy(v => v.Id)
            .Select(v => new
            {
                v.Sku,
                v.Stock,
                Values = v.Values
                    .Select(x => new { x.AttributeValue.Attribute.Kind, x.AttributeValue.Value })
                    .ToList()
            })
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("brand,model,color,stock,sku");
        foreach (var v in rows)
        {
            var brand = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Brand)?.Value ?? "";
            var model = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Model)?.Value ?? "";
            var color = v.Values.FirstOrDefault(x => x.Kind == VariantAttributeKind.Color)?.Value ?? "";
            sb.Append(Csv(brand)).Append(',')
              .Append(Csv(model)).Append(',')
              .Append(Csv(color)).Append(',')
              .Append(v.Stock.ToString(CultureInfo.InvariantCulture)).Append(',')
              .Append(Csv(v.Sku)).Append('\n');
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return new byte[] { 0xEF, 0xBB, 0xBF }.Concat(bytes).ToArray();
    }

    private static string Csv(string? value)
    {
        value ??= "";
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }
}

/// <summary>نتیجهٔ بارگذاری CSV واریانت‌های یک محصول.</summary>
public class ImportProductVariantsResult
{
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// بارگذاری/به‌روزرسانی انبوه واریانت‌های یک محصول از CSV با ستون‌های brand,model,color,stock,sku.
/// تطبیق: ابتدا با SKU، در غیر این صورت با ترکیب مقادیر (برند+مدل+رنگ). مقادیر تازه در pool ساخته می‌شوند.
/// </summary>
public record ImportProductVariantsCsvCommand(int ProductId, string CsvContent) : IRequest<ImportProductVariantsResult>;

public class ImportProductVariantsCsvCommandHandler : IRequestHandler<ImportProductVariantsCsvCommand, ImportProductVariantsResult>
{
    private readonly IApplicationDbContext _db;
    public ImportProductVariantsCsvCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<ImportProductVariantsResult> Handle(ImportProductVariantsCsvCommand request, CancellationToken ct)
    {
        var result = new ImportProductVariantsResult();
        var content = (request.CsvContent ?? "").TrimStart('\uFEFF');

        if (!await _db.Products.AnyAsync(p => p.Id == request.ProductId, ct))
        {
            result.Errors.Add("محصول یافت نشد.");
            return result;
        }

        var lines = content.Replace("\r\n", "\n").Replace("\r", "\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return result;

        var header = ParseLine(lines[0]);
        int ci(string name) => Array.FindIndex(header, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
        int bCol = ci("brand"), mCol = ci("model"), cCol = ci("color"), sCol = ci("stock"), kCol = ci("sku");
        if (sCol < 0) { result.Errors.Add("ستون stock یافت نشد."); return result; }
        var startIndex = 1;

        // attributeهای موجود بر اساس نوع
        var brandAttr = await _db.VariantAttributes.FirstOrDefaultAsync(a => a.Kind == VariantAttributeKind.Brand, ct);
        var modelAttr = await _db.VariantAttributes.FirstOrDefaultAsync(a => a.Kind == VariantAttributeKind.Model, ct);
        var colorAttr = await _db.VariantAttributes.FirstOrDefaultAsync(a => a.Kind == VariantAttributeKind.Color, ct);

        // واریانت‌های فعلی محصول با مقادیرشان
        var existing = await _db.ProductVariants
            .Include(v => v.Values).ThenInclude(vv => vv.AttributeValue).ThenInclude(av => av.Attribute)
            .Where(v => v.ProductId == request.ProductId)
            .ToListAsync(ct);

        for (var i = startIndex; i < lines.Length; i++)
        {
            var cells = ParseLine(lines[i]);
            string Get(int idx) => idx >= 0 && idx < cells.Length ? cells[idx].Trim() : "";

            var brand = Get(bCol);
            var model = Get(mCol);
            var color = Get(cCol);
            var sku = Get(kCol);
            var stockStr = Get(sCol);

            if (!int.TryParse(stockStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var stock) || stock < 0)
            {
                result.Skipped++;
                if (result.Errors.Count < 20) result.Errors.Add($"سطر {i + 1}: موجودی نامعتبر «{stockStr}».");
                continue;
            }

            // 1) تطبیق با SKU
            ProductVariant? match = null;
            if (!string.IsNullOrWhiteSpace(sku))
                match = existing.FirstOrDefault(v => string.Equals(v.Sku, sku, StringComparison.OrdinalIgnoreCase));

            // 2) تطبیق با ترکیب مقادیر
            if (match is null)
                match = existing.FirstOrDefault(v => MatchesValues(v, brand, model, color));

            if (match is not null)
            {
                match.Stock = stock;
                if (!string.IsNullOrWhiteSpace(sku)) match.Sku = sku;
                match.UpdatedAt = DateTime.UtcNow;
                result.Updated++;
                continue;
            }

            // 3) ساخت واریانت جدید (نیازمند حداقل یک مقدار)
            var valueIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(brand))
            {
                if (brandAttr is null) { result.Skipped++; if (result.Errors.Count < 20) result.Errors.Add($"سطر {i + 1}: ویژگی «برند» تعریف نشده."); continue; }
                valueIds.Add(await EnsureValueAsync(brandAttr, brand, ct));
            }
            if (!string.IsNullOrWhiteSpace(model))
            {
                if (modelAttr is null) { result.Skipped++; if (result.Errors.Count < 20) result.Errors.Add($"سطر {i + 1}: ویژگی «مدل» تعریف نشده."); continue; }
                valueIds.Add(await EnsureValueAsync(modelAttr, model, ct));
            }
            if (!string.IsNullOrWhiteSpace(color))
            {
                if (colorAttr is null) { result.Skipped++; if (result.Errors.Count < 20) result.Errors.Add($"سطر {i + 1}: ویژگی «رنگ» تعریف نشده."); continue; }
                valueIds.Add(await EnsureValueAsync(colorAttr, color, ct));
            }

            if (valueIds.Count == 0)
            {
                result.Skipped++;
                if (result.Errors.Count < 20) result.Errors.Add($"سطر {i + 1}: هیچ ویژگی‌ای برای ساخت واریانت ندارد.");
                continue;
            }

            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Sku = string.IsNullOrWhiteSpace(sku) ? null : sku,
                Stock = stock,
                Price = 0,
                IsActive = true,
                Values = valueIds.Select(id => new ProductVariantValue { AttributeValueId = id }).ToList()
            };
            _db.ProductVariants.Add(variant);
            existing.Add(variant); // برای جلوگیری از ساخت تکراری در همین فایل
            result.Created++;
        }

        await _db.SaveChangesAsync(ct);
        return result;
    }

    private static bool MatchesValues(ProductVariant v, string brand, string model, string color)
    {
        string? ValOf(VariantAttributeKind k) =>
            v.Values.FirstOrDefault(x => x.AttributeValue.Attribute.Kind == k)?.AttributeValue.Value;

        bool Eq(string? a, string b) =>
            string.IsNullOrWhiteSpace(b) ? string.IsNullOrWhiteSpace(a) : string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

        return Eq(ValOf(VariantAttributeKind.Brand), brand)
            && Eq(ValOf(VariantAttributeKind.Model), model)
            && Eq(ValOf(VariantAttributeKind.Color), color);
    }

    private readonly Dictionary<(int, string), int> _valueCache = new();

    private async Task<int> EnsureValueAsync(VariantAttribute attr, string value, CancellationToken ct)
    {
        value = value.Trim();
        var key = (attr.Id, value.ToLowerInvariant());
        if (_valueCache.TryGetValue(key, out var cached)) return cached;

        var existing = await _db.VariantAttributeValues
            .FirstOrDefaultAsync(x => x.AttributeId == attr.Id && x.Value == value, ct);
        if (existing is not null)
        {
            _valueCache[key] = existing.Id;
            return existing.Id;
        }

        var created = new VariantAttributeValue { AttributeId = attr.Id, Value = value };
        _db.VariantAttributeValues.Add(created);
        await _db.SaveChangesAsync(ct);
        _valueCache[key] = created.Id;
        return created.Id;
    }

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
