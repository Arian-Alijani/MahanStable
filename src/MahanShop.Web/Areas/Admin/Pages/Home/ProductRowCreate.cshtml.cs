using FluentValidation;
using MahanShop.Application.Features.Admin.Categories;
using MahanShop.Application.Features.Admin.Home;
using MahanShop.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MahanShop.Web.Areas.Admin.Pages.Home;

/// <summary>ایجاد نوار محصول جدید بر اساس منبع (پیشنهادی/پرفروش/جدید/تخفیف/دسته).</summary>
public class ProductRowCreateModel : PageModel
{
    private readonly IMediator _mediator;
    public ProductRowCreateModel(IMediator mediator) => _mediator = mediator;

    [BindProperty] public string Title { get; set; } = "";
    [BindProperty] public HomeProductSource ProductSource { get; set; } = HomeProductSource.Featured;
    [BindProperty] public int? CategoryId { get; set; }
    [BindProperty] public int MaxItems { get; set; } = 10;
    [BindProperty] public int DisplayOrder { get; set; }
    [BindProperty] public bool IsActive { get; set; } = true;

    public List<CategoryOptionDto> CategoryOptions { get; private set; } = new();

    public async Task OnGetAsync() => await LoadAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await _mediator.Send(new CreateProductRowCommand(
                Title, ProductSource, CategoryId, MaxItems, DisplayOrder, IsActive));
            TempData["AdminOk"] = "نوار محصول ساخته شد.";
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
