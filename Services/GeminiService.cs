using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace ArgentinaAccesible.Services;

public class GeminiService
{
    private const int MaxIntentos = 3;

    private readonly HttpClient _http;
    private readonly IConfiguration _config;
    private readonly ILogger<GeminiService> _logger;

    public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
    {
        _http = http;
        _config = config;
        _logger = logger;
    }

    // Motivo del último fallo (solo se muestra en Development)
    public string? UltimoError { get; private set; }

    // Devuelve null si no se pudo obtener respuesta
    public async Task<string?> PreguntarAsync(string systemPrompt, IEnumerable<(string Rol, string Texto)> historial)
    {
        UltimoError = null;

        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            UltimoError = "Falta configurar Gemini:ApiKey en los secretos de usuario.";
            _logger.LogWarning("Falta configurar Gemini:ApiKey");
            return null;
        }

        var modelo = _config["Gemini:Model"] ?? "gemini-3.5-flash-lite";
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{modelo}:generateContent";

        var cuerpo = new
        {
            system_instruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = historial.Select(m => new { role = m.Rol, parts = new[] { new { text = m.Texto } } }),
            generationConfig = new { temperature = 0.4, maxOutputTokens = 2048 }
        };

        try
        {
            for (var intento = 1; intento <= MaxIntentos; intento++)
            {
                // Un HttpRequestMessage no se puede reutilizar: se crea uno nuevo en cada intento
                using var pedido = new HttpRequestMessage(HttpMethod.Post, url) { Content = JsonContent.Create(cuerpo) };
                pedido.Headers.Add("x-goog-api-key", apiKey);

                using var respuesta = await _http.SendAsync(pedido);
                var json = await respuesta.Content.ReadAsStringAsync();

                if (!respuesta.IsSuccessStatusCode)
                {
                    UltimoError = $"Modelo '{modelo}'. Gemini respondió {(int)respuesta.StatusCode}: {json}";
                    _logger.LogWarning("Gemini respondió {Estado} (intento {Intento}): {Cuerpo}", (int)respuesta.StatusCode, intento, json);

                    // Si Google está saturado (503) o limitó el uso (429), se espera un poco y se reintenta
                    var reintentable = respuesta.StatusCode == HttpStatusCode.ServiceUnavailable
                                       || respuesta.StatusCode == HttpStatusCode.TooManyRequests;
                    if (reintentable && intento < MaxIntentos)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(2 * intento));
                        continue;
                    }
                    return null;
                }

                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("candidates", out var candidatos) || candidatos.GetArrayLength() == 0)
                {
                    UltimoError = $"Gemini no devolvió candidatos: {json}";
                    return null;
                }

                var texto = new StringBuilder();
                if (candidatos[0].TryGetProperty("content", out var contenido) &&
                    contenido.TryGetProperty("parts", out var partes))
                {
                    foreach (var parte in partes.EnumerateArray())
                        if (parte.TryGetProperty("text", out var t)) texto.Append(t.GetString());
                }

                var resultado = texto.ToString().Trim();
                if (resultado.Length == 0)
                {
                    UltimoError = $"Gemini devolvió una respuesta vacía: {json}";
                    return null;
                }
                return resultado;
            }

            return null;
        }
        catch (Exception ex)
        {
            UltimoError = $"Excepción al llamar a Gemini: {ex.Message}";
            _logger.LogError(ex, "Error al llamar a Gemini");
            return null;
        }
    }
}