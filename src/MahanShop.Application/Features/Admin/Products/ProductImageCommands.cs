using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Products;

/// <summary>افزودن یک عکس به گالری محصول (Url قبلاً توسط سرویس آپلود ذخیره شده).</summary>
public record AddProductImageCommand(int ProductId, string Url, string? Alt) : IRequest<int>;

public class AddProductImageCommandHandler : IRequestHandler<AddProductImageCommand, int>
{
    private readonly IApplicationDbContext _db;
    public AddProductImageCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(AddProductImageCommand request, CancellationToken ct)
    {
        var product = await _db.Products.Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, ct)
            ?? throw new ValidationException("محصول یافت نشد.");

        var isFirst = product.Images.Count == 0;
        var maxOrder = product.Images.Count == 0 ? -1 : product.Images.Max(i => i.DisplayOrder);

        var img = new ProductImage
        {
            ProductId = product.Id,
            Url = request.Url,
            Alt = request.Alt?.Trim(),
            IsMain = isFirst,          // اولین عکس = عکس اصلی
            DisplayOrder = maxOrder + 1
        };
        _db.ProductImages.Add(img);
        await _db.SaveChangesAsync(ct);
        return img.Id;
    }
}

/// <summary>حذف یک عکس از گالری. اگر عکس اصلی حذف شد، عکس بعدی اصلی می‌شود.</summary>
public record DeleteProductImageCommand(int ImageId) : IRequest<bool>;

public class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteProductImageCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteProductImageCommand request, CancellationToken ct)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == request.ImageId, ct)
            ?? throw new ValidationException("عکس یافت نشد.");
        var wasMain = img.IsMain;
        var productId = img.ProductId;
        _db.ProductImages.Remove(img);
        await _db.SaveChangesAsync(ct);

        if (wasMain)
        {
            var next = await _db.ProductImages
                .Where(i => i.ProductId == productId)
                .OrderBy(i => i.DisplayOrder).FirstOrDefaultAsync(ct);
            if (next != null) { next.IsMain = true; await _db.SaveChangesAsync(ct); }
        }
        return true;
    }
}

/// <summary>تعیین عکس اصلی محصول.</summary>
public record SetMainProductImageCommand(int ImageId) : IRequest<bool>;

public class SetMainProductImageCommandHandler : IRequestHandler<SetMainProductImageCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public SetMainProductImageCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(SetMainProductImageCommand request, CancellationToken ct)
    {
        var img = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == request.ImageId, ct)
            ?? throw new ValidationException("عکس یافت نشد.");
        var siblings = await _db.ProductImages.Where(i => i.ProductId == img.ProductId).ToListAsync(ct);
        foreach (var s in siblings) s.IsMain = s.Id == img.Id;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
