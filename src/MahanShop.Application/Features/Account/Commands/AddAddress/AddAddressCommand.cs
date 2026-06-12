using MahanShop.Application.Common;
using MahanShop.Application.Common.Interfaces;
using MahanShop.Domain.Entities;
using FluentValidation;
using MediatR;

namespace MahanShop.Application.Features.Account.Commands.AddAddress;

/// <summary>افزودن آدرس برای کاربر جاری. UserId از claim (نه فرم).</summary>
public record AddAddressCommand(
    int UserId, string Province, string City, string PostalCode,
    string FullAddress, string ReceiverName, string ReceiverPhone) : IRequest<int>;

public class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.UserId).GreaterThan(0);
        RuleFor(x => x.Province).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.PostalCode).NotEmpty().Matches(@"^\d{10}$").WithMessage("کد پستی باید ۱۰ رقم باشد.");
        RuleFor(x => x.FullAddress).NotEmpty().MaximumLength(800);
        RuleFor(x => x.ReceiverName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ReceiverPhone)
            .Must(p => PhoneNumberHelper.Normalize(p) is not null)
            .WithMessage("شماره تماس گیرنده معتبر نیست.");
    }
}

public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, int>
{
    private readonly IApplicationDbContext _db;
    public AddAddressCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task<int> Handle(AddAddressCommand request, CancellationToken ct)
    {
        var address = new Address
        {
            UserId = request.UserId,
            Province = request.Province.Trim(),
            City = request.City.Trim(),
            PostalCode = request.PostalCode.Trim(),
            FullAddress = request.FullAddress.Trim(),
            ReceiverName = request.ReceiverName.Trim(),
            ReceiverPhone = PhoneNumberHelper.Normalize(request.ReceiverPhone)!
        };
        _db.Addresses.Add(address);
        await _db.SaveChangesAsync(ct);
        return address.Id;
    }
}
