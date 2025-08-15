using Microsoft.AspNetCore.Mvc;
using IntegracionNiubizDemo.Application.Abstractions;

namespace IntegracionNiubizDemo.Web.Controllers;

public class ProductsController : Controller
{
    private readonly IProductService _service;
    public ProductsController(IProductService service) => _service = service;

    public async Task<IActionResult> Index()
    {
        var products = await _service.GetProductsAsync();
        return View(products);
    }
}
