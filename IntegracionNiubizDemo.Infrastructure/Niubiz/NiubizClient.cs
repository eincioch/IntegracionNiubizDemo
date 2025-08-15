using IntegracionNiubizDemo.Application.Abstractions;
using IntegracionNiubizDemo.Application.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace IntegracionNiubizDemo.Infrastructure.Niubiz;

public class NiubizClient : INiubizGateway
{
    private readonly HttpClient _http;
    private readonly ILogger<NiubizClient> _logger;
    private readonly NiubizOptions _opt;

    public NiubizClient(HttpClient http, IOptions<NiubizOptions> opt, ILogger<NiubizClient> logger)
    {
        _http = http;
        _logger = logger;
        _opt = opt.Value;
    }

    public async Task<string> GetSecurityTokenAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_opt.Username) || string.IsNullOrWhiteSpace(_opt.Password))
            throw new InvalidOperationException("Credenciales Niubiz no configuradas (Username/Password).");

        // URL absoluta exacta como en tu ejemplo
        var baseUrl = (_opt.BaseUrl ?? "").TrimEnd('/');
        var path = string.IsNullOrWhiteSpace(_opt.SecurityEndpoint) ? "/api.security/v1/security" : _opt.SecurityEndpoint.Trim();
        if (!path.StartsWith("/")) path = "/" + path;
        var uri = new Uri(baseUrl + path);

        // Basic Auth en ISO-8859-1
        var credentials = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes($"{_opt.Username}:{_opt.Password}"));

        using var req = new HttpRequestMessage(HttpMethod.Post, uri);
        req.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        req.Headers.Accept.Clear();
        //req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Content = new StringContent(string.Empty, Encoding.UTF8, "text/plain");

        using var res = await _http.SendAsync(req, ct);
        var body = await res.Content.ReadAsStringAsync(ct);

        if (!res.IsSuccessStatusCode)
            throw new InvalidOperationException($"No se pudo obtener el security token (HTTP {(int)res.StatusCode}): {body}");

        var token = body.Trim().Trim('"');
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("Security token vacío");

        return token;
    }

    public async Task<string> CreateSessionAsync(string securityToken, decimal amount, string purchaseNumber, string currency, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, _opt.SessionEndpoint.Replace("{merchantId}", _opt.MerchantId));
        req.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        req.Headers.Add("Authorization", securityToken);

        var body = new
        {
            channel = "web",
            amount = amount.ToString("F2", CultureInfo.InvariantCulture),
            antifraud = new { 
                clientIp = "127.0.0.1", 
                merchantDefineData = new { 
                    MDD4 = "enrique.incio@gmail.com",
                    MDD30 = "40904759",
                    MDD31 = "986687645",
                    MDD32 = "40904759",
                    MDD33 = "25",
                    MDD34 = "40904759",
                    MDD63 = "25",
                    MDD65 = "40904759",
                    MDD71 = "700526895",
                    MDD75 = "Registrado",
                    MDD77 = 0
                } 
            },
            purchaseNumber,
            recurrenceMaxAmount = amount.ToString("F2", CultureInfo.InvariantCulture)
        };

        var jsonBody = JsonSerializer.Serialize(body);
        _logger.LogDebug("JSON enviado a Niubiz: {Json}", jsonBody);
        req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req, ct);
        var json = await res.Content.ReadAsStringAsync(ct);
        if (!res.IsSuccessStatusCode)
        {
            _logger.LogError("Error creando sesión: {Status} {Body}", res.StatusCode, json);
            throw new InvalidOperationException("No se pudo crear la sesión");
        }

        using var doc = JsonDocument.Parse(json);
        var sessionKey = doc.RootElement.TryGetProperty("sessionKey", out var sk) ? sk.GetString()
                      : doc.RootElement.TryGetProperty("sessionkey", out var sk2) ? sk2.GetString()
                      : null;
        if (string.IsNullOrWhiteSpace(sessionKey))
        {
            _logger.LogError("Respuesta sesión sin sessionKey: {Body}", json);
            throw new InvalidOperationException("Respuesta de sesión inválida");
        }
        return sessionKey!;
    }

    public async Task<AuthorizationResult> AuthorizeAsync(string securityToken, string transactionToken, decimal amount, string currency, string purchaseNumber, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, _opt.AuthorizationEndpoint.Replace("{merchantId}", _opt.MerchantId));
        req.Headers.Add("Authorization", securityToken);

        var body = new
        {
            captureType = "manual",
            cardHolder = new
            {
                documentNumber = "40904759",
                documentType = "0" //0= DNI, 1= Canet de extranjería, 2= Pasaporte
            },
            channel = "web",
            countable = true,
            order = new
            {
                amount = amount.ToString("F2", CultureInfo.InvariantCulture),
                currency,
                purchaseNumber,
                tokenId = transactionToken,
            },
            recurrence = new {
                amount = amount.ToString("F2", CultureInfo.InvariantCulture),
                beneficiaryId = "0",
                frequency = "FALSE",
                maxAmount = amount.ToString("F2", CultureInfo.InvariantCulture),
                type = ""
            }
        };

        var jsonBody = JsonSerializer.Serialize(body);
        _logger.LogDebug("JSON enviado a Niubiz: {Json}", jsonBody);
        req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var res = await _http.SendAsync(req, ct);
        var json = await res.Content.ReadAsStringAsync(ct);

        _logger.LogInformation("Respuesta autorización: {Status} {Body}", res.StatusCode, json);

        var approved = false;
        string? authCode = null;
        string? maskedCard = null;

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            // Estructura oficial: "order" y "dataMap"
            if (root.TryGetProperty("order", out var orderEl))
            {
                approved = orderEl.TryGetProperty("actionCode", out var ac) && ac.GetString() == "000";
                authCode = orderEl.TryGetProperty("authorizationCode", out var au) ? au.GetString() : null;
            }

            if (root.TryGetProperty("dataMap", out var dataMap))
            {
                if (!approved && dataMap.TryGetProperty("ACTION_CODE", out var ac2))
                    approved = ac2.GetString() == "000"
                               || (dataMap.TryGetProperty("STATUS", out var st) && string.Equals(st.GetString(), "Authorized", StringComparison.OrdinalIgnoreCase));

                if (string.IsNullOrEmpty(authCode) && dataMap.TryGetProperty("AUTHORIZATION_CODE", out var au2))
                    authCode = au2.GetString();

                if (dataMap.TryGetProperty("CARD", out var card))
                    maskedCard = card.GetString();
            }

            // Fallbacks (variantes antiguas)
            if (root.TryGetProperty("data", out var legacy))
            {
                if (!approved && legacy.TryGetProperty("ACTION_CODE", out var lac))
                    approved = lac.GetString() == "000";
                if (string.IsNullOrEmpty(authCode) && legacy.TryGetProperty("AUTHORIZATION_CODE", out var lau))
                    authCode = lau.GetString();
                if (string.IsNullOrEmpty(maskedCard) && legacy.TryGetProperty("CARD", out var lcard) && lcard.ValueKind == JsonValueKind.Object && lcard.TryGetProperty("CARDNUMBER", out var cn))
                    maskedCard = cn.GetString();
            }

            if (!approved)
                approved = root.TryGetProperty("actionCode", out var acFlat) && acFlat.GetString() == "000";
            if (string.IsNullOrEmpty(authCode) && root.TryGetProperty("authorizationCode", out var auFlat))
                authCode = auFlat.GetString();
        }
        catch
        {
            // logging ya realizado
        }

        return new AuthorizationResult(approved, authCode, maskedCard, json);
    }
}