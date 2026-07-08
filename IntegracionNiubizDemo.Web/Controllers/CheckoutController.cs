using Microsoft.AspNetCore.Mvc;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Dtos;
using IntegracionNiubizDemo.Web.Models;

namespace IntegracionNiubizDemo.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly ICheckoutService _checkout;
    private readonly IProductService _products;
    private readonly IGeocodingService _geocoding;

    public CheckoutController(ICheckoutService checkout, IProductService products, IGeocodingService geocoding)
    {
        _checkout = checkout;
        _products = products;
        _geocoding = geocoding;
    }

    [HttpGet("/checkout/pay/{productId:guid}")]
    public async Task<IActionResult> Pay(Guid productId, string? email = null, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct);
        if (product is null)
        {
            TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
            return RedirectToAction("Index", "Products");
        }

        var vm = new CheckoutPayViewModel
        {
            ProductId = product.Id,
            ProductName = product.Name,
            Amount = product.Price,
            CustomerEmail = email
        };
        return View("Pay", vm);
    }

    [HttpPost("/checkout/pay/{productId:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(Guid productId, CheckoutPayViewModel model, CancellationToken ct = default)
    {
        var product = await _products.GetByIdAsync(productId, ct);
        if (product is null)
        {
            TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
            return RedirectToAction("Index", "Products");
        }

        // Repuebla los datos de solo lectura del producto (no viajan desde el form).
        model.ProductId = product.Id;
        model.ProductName = product.Name;
        model.Amount = product.Price;

        if (!ModelState.IsValid)
        {
            return View("Pay", model);
        }

        try
        {
            var address = new CustomerAddressInput(
                model.Address,
                model.City,
                model.State,
                model.Country,
                model.PostalCode,
                model.Phone);

            var init = await _checkout.InitAsync(productId, model.CustomerEmail, address, ct);

            // Garantiza que el purchaseNumber no se pierda
            TempData["LastPurchaseNumber"] = init.PurchaseNumber;
            Response.Cookies.Append("LastPurchaseNumber", init.PurchaseNumber, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });

            model.SessionKey = init.SessionKey;
            model.MerchantId = init.MerchantId;
            model.PurchaseNumber = init.PurchaseNumber;
            model.StaticJsUrl = init.StaticJsUrl;
            model.Currency = init.Currency;
            model.Amount = init.Amount;

            return View("Pay", model);
        }
        catch (KeyNotFoundException)
        {
            TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
            return RedirectToAction("Index", "Products");
        }
    }

    [HttpPost("/checkout/reverse-geocode")]
    public async Task<IActionResult> ReverseGeocode([FromBody] ReverseGeocodeRequest request, CancellationToken ct = default)
    {
        if (request is null || !ModelState.IsValid)
        {
            return BadRequest(new { success = false, error = "Coordenadas inválidas" });
        }

        var result = await _geocoding.ReverseGeocodeAsync(request.Latitude, request.Longitude, ct);
        return Ok(result);
    }

    [HttpPost("/checkout/confirm")]
    //[ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(
        [FromForm] string? purchaseNumber,
        [FromForm] string? transactionToken,
        [FromForm(Name = "tokenId")] string? tokenId,
        [FromForm(Name = "token")] string? token,
        CancellationToken ct = default)
    {
        // Recuperar purchaseNumber desde el form o la cookie/TempData
        var pn = purchaseNumber;
        if (string.IsNullOrWhiteSpace(pn))
            Request.Cookies.TryGetValue("LastPurchaseNumber", out pn);
        if (string.IsNullOrWhiteSpace(pn))
            pn = TempData.Peek("LastPurchaseNumber") as string;

        // Opcional: limpiar la cookie para evitar reutilización
        if (Request.Cookies.ContainsKey("LastPurchaseNumber"))
            Response.Cookies.Delete("LastPurchaseNumber");

        // Consolidar token
        
        var tok = transactionToken ?? tokenId ?? token;
        if (string.IsNullOrWhiteSpace(tok))
        {
            var err = new ConfirmResult(false, pn ?? "-", null, "No se recibió token de transacción", null, "{}");
            return View("Confirm", err);
        }

        if (string.IsNullOrWhiteSpace(pn))
        {
            var errPn = new ConfirmResult(false, "-", null, "No se recibió purchaseNumber", null, "{}");
            return View("Confirm", errPn);
        }

        var result = await _checkout.ConfirmAsync(pn, tok, ct);
        return View("Confirm", result);
    }
}