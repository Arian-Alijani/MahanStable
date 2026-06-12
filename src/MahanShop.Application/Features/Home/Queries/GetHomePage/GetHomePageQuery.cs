using MediatR;

namespace MahanShop.Application.Features.Home.Queries.GetHomePage;

/// <summary>کوئری ساخت کل ViewModel صفحه اصلی از DB (بنر + دسته منتخب + نوارهای مرتب).</summary>
public record GetHomePageQuery : IRequest<HomePageViewModel>;
