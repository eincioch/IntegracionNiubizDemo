using System.ComponentModel.DataAnnotations;

namespace IntegracionNiubizDemo.Web.Models;

/// <summary>
/// ViewModel del checkout: combina los datos de solo lectura del producto con el
/// formulario de dirección/ubicación del comprador (requerido por Niubiz para el
/// dataMap antifraude) y, tras iniciar la sesión, los datos necesarios para
/// renderizar el widget de pago de Niubiz.
/// </summary>
public class CheckoutPayViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "PEN";

    [EmailAddress(ErrorMessage = "Correo inválido")]
    [Display(Name = "Correo electrónico")]
    public string? CustomerEmail { get; set; }

    [Required(ErrorMessage = "La dirección es requerida")]
    [StringLength(300)]
    [Display(Name = "Dirección")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es requerida")]
    [StringLength(100)]
    [Display(Name = "Ciudad")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "El departamento/estado es requerido")]
    [StringLength(50)]
    [Display(Name = "Departamento/Estado")]
    public string State { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es requerido")]
    [RegularExpression("^[A-Za-z]{2}$", ErrorMessage = "Usa el código ISO de 2 letras (ej. PE)")]
    [Display(Name = "País")]
    public string Country { get; set; } = "PE";

    [Required(ErrorMessage = "El código postal es requerido")]
    [StringLength(20)]
    [Display(Name = "Código postal")]
    public string PostalCode { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Teléfono inválido")]
    [StringLength(20)]
    [Display(Name = "Teléfono")]
    public string? Phone { get; set; }

    // Datos de sesión Niubiz: solo se completan tras un POST exitoso a InitAsync.
    public string? SessionKey { get; set; }
    public string? MerchantId { get; set; }
    public string? PurchaseNumber { get; set; }
    public string? StaticJsUrl { get; set; }

    /// <summary>Indica si ya existe una sesión Niubiz activa y debe mostrarse el widget de pago.</summary>
    public bool HasSession => !string.IsNullOrWhiteSpace(SessionKey);
}
