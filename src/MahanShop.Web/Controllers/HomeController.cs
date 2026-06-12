using System.Diagnostics;
using MahanShop.Application.Features.Home.Queries.GetHomePage;
using MahanShop.Web.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MahanShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly IMediator _mediator;

    public HomeController(IMediator mediator) => _mediator = mediator;

    public async Task<IActionResult> Index()
    {
        var vm = await _mediator.Send(new GetHomePageQuery());
        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
