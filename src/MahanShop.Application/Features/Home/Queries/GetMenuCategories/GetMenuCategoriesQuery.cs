using MediatR;

namespace MahanShop.Application.Features.Home.Queries.GetMenuCategories;

/// <summary>درخت دسته‌بندی فعالِ ShowInMenu برای مگامنوی هدر.</summary>
public record GetMenuCategoriesQuery : IRequest<List<MenuCategoryDto>>;
