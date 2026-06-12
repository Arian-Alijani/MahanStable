using System.Text.Json;
using MahanShop.Application.Features.Cart.Models;

namespace MahanShop.Web.Services;

/// <summary>سبد خرید session-based. فقط ProductId/VariantId/Quantity ذخیره می‌شود — قیمت هرگز در session (سرور از DB می‌خواند). کلید session ثابت.</summary>
public class CartStore
{
    private const string Key = "cart";
    private const int MaxQtyPerLine = 20;
    private const int MaxLines = 50;

    private readonly IHttpContextAccessor _accessor;

    public CartStore(IHttpContextAccessor accessor) => _accessor = accessor;

    private ISession Session => _accessor.HttpContext!.Session;

    public List<CartItemInput> GetItems()
    {
        var json = Session.GetString(Key);
        if (string.IsNullOrEmpty(json)) return new();
        try
        {
            return JsonSerializer.Deserialize<List<CartItemInput>>(json) ?? new();
        }
        catch (JsonException)
        {
            return new();
        }
    }

    private void Save(List<CartItemInput> items) =>
        Session.SetString(Key, JsonSerializer.Serialize(items));

    /// <summary>افزودن/افزایش. اقلام یکسان (محصول+variant) ادغام. مقدار clamp به MaxQtyPerLine. اعتبار واقعی قیمت/موجودی هنگام نمایش/ثبت در DB.</summary>
    public void Add(int productId, int? variantId, int quantity)
    {
        if (productId <= 0 || quantity <= 0) return;
        var items = GetItems();

        var existing = items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
        if (existing is not null)
        {
            existing.Quantity = Math.Min(MaxQtyPerLine, existing.Quantity + quantity);
        }
        else
        {
            if (items.Count >= MaxLines) return;
            items.Add(new CartItemInput { ProductId = productId, VariantId = variantId, Quantity = Math.Min(MaxQtyPerLine, quantity) });
        }
        Save(items);
    }

    /// <summary>تنظیم مقدار دقیق یک قلم. مقدار ۰ یا کمتر → حذف.</summary>
    public void SetQuantity(int productId, int? variantId, int quantity)
    {
        var items = GetItems();
        var existing = items.FirstOrDefault(i => i.ProductId == productId && i.VariantId == variantId);
        if (existing is null) return;

        if (quantity <= 0)
            items.Remove(existing);
        else
            existing.Quantity = Math.Min(MaxQtyPerLine, quantity);
        Save(items);
    }

    public void Remove(int productId, int? variantId)
    {
        var items = GetItems();
        items.RemoveAll(i => i.ProductId == productId && i.VariantId == variantId);
        Save(items);
    }

    public void Clear() => Session.Remove(Key);

    public int Count() => GetItems().Sum(i => i.Quantity);
}
