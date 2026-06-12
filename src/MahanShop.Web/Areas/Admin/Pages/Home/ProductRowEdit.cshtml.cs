using FluentValidation;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Home;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>ویرایش نوار محصول.</summary>
public class ProductRowEditModel : PageModel
{
    private readonly IMediator _mediator;
    public ProductRowEditModel(IMediator mediator) => _mediator = mediator;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public HomeProductSource ProductSource { get; set; } = HomeProductSource.Featured;
    [BindProperty] public int? CategoryId { get; set; }
    [BindProperty] public int MaxItems { get; set; } = 10;
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;

    public List<CategoryOptionDto> CategoryOptions { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var dto = await _mediator.Send(new GetProductRowForEditQuery(Id));
        if (dto is null) return NotFound();

        Title = dto.Title;
        ProductSource = dto.ProductSource;
        CategoryId = dto.CategoryId;
        MaxItems = dto.MaxItems;
        DisplayOrder = dto.DisplayOrder;
        IsActive = dto.IsActive;
        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new UpdateProductRowCommand(
                Id, Title, ProductSource, CategoryId, MaxItems, DisplayOrder, IsActive));
            TempData["AdminOk"] = "نوار محصول به‌روزرسانی شد.";
            return RedirectToPage("Index");
        }
        catch (ValidationException ex)
        {
            foreach (var e in ex.Errors) ModelState.AddModelError(string.Empty, e.ErrorMessage);
            await LoadAsync();
            return Page();
        }
    }

    private async Task LoadAsync() =>
        CategoryOptions = await _mediator.Send(new GetCategoryOptionsQuery());
}
