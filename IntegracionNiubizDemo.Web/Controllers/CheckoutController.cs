using Microsoft.AspNetCore.Mvc;
using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Dtos;

namespace IntegracionNiubizDemo.Web.Controllers;

public class CheckoutController : Controller
{
    private readonly ICheckoutService _checkout;
    public CheckoutController(ICheckoutService checkout) => _checkout = checkout;

    [HttpGet("/checkout/pay/{productId:guid}")]
    public async Task<IActionResult> Pay(Guid productId, string? email = null, CancellationToken ct = default)
    {
        try
        {
            var init = await _checkout.InitAsync(productId, email, ct);

            // Garantiza que el purchaseNumber no se pierda
            TempData["LastPurchaseNumber"] = init.PurchaseNumber;
            Response.Cookies.Append("LastPurchaseNumber", init.PurchaseNumber, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(30)
            });


            return View("Pay", init);
        }
        catch (KeyNotFoundException)
        {
            TempData["PaymentMessage"] = "Producto no encontrado. Refresca la lista e inténtalo otra vez.";
            return RedirectToAction("Index", "Products");
        }
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