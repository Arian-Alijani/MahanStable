using FluentValidation;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Application.Features.Admin.Common;
using MahanShop.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MahanShop.Application.Features.Admin.Tags;

/// <summary>ایجاد برچسب جدید. Slug اگر خالی باشد از Name ساخته می‌شود.</summary>
public record CreateTagCommand(string Name, string? Slug) : IRequest<int>;

public class CreateTagCommandValidator : AbstractValidator<CreateTagCommand>
{
    public CreateTagCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام برچسب را وارد کنید.").MaximumLength(100);
    }
}

public class CreateTagCommandHandler : IRequestHandler<CreateTagCommand, int>
{
    private readonly IApplicationDbContext _db;
    public CreateTagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(CreateTagCommand request, CancellationToken ct)
    {
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Make(request.Name)
            : SlugHelper.Make(request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Tags.AnyAsync(t => t.Slug == slug, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        var tag = new Tag { Name = request.Name.Trim(), Slug = slug };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(ct);
        return tag.Id;
    }
}

/// <summary>ویرایش برچسب موجود.</summary>
public record UpdateTagCommand(int Id, string Name, string? Slug) : IRequest<bool>;

public class UpdateTagCommandValidator : AbstractValidator<UpdateTagCommand>
{
    public UpdateTagCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().WithMessage("نام برچسب را وارد کنید.").MaximumLength(100);
    }
}

public class UpdateTagCommandHandler : IRequestHandler<UpdateTagCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public UpdateTagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(UpdateTagCommand request, CancellationToken ct)
    {
        var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new ValidationException("برچسب یافت نشد.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugHelper.Make(request.Name)
            : SlugHelper.Make(request.Slug);
        if (string.IsNullOrEmpty(slug)) slug = SlugHelper.Make(request.Name);

        if (await _db.Tags.AnyAsync(t => t.Slug == slug && t.Id != request.Id, ct))
            throw new ValidationException("نامک (slug) تکراری است.");

        tag.Name = request.Name.Trim();
        tag.Slug = slug;
        tag.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return true;
    }
}

/// <summary>حذف برچسب + رابطه‌های محصولی آن.</summary>
public record DeleteTagCommand(int Id) : IRequest<bool>;

public class DeleteTagCommandHandler : IRequestHandler<DeleteTagCommand, bool>
{
    private readonly IApplicationDbContext _db;
    public DeleteTagCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<bool> Handle(DeleteTagCommand request, CancellationToken ct)
    {
        var tag = await _db.Tags
            .Include(t => t.ProductTags)
            .FirstOrDefaultAsync(t => t.Id == request.Id, ct)
            ?? throw new ValidationException("برچسب یافت نشد.");

        _db.ProductTags.RemoveRange(tag.ProductTags);
        _db.Tags.Remove(tag);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}
