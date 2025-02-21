using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

[ApiController]
[Route("api/translate")]
public class TranslationController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TranslationController> _logger;

    public TranslationController(IConfiguration configuration, HttpClient httpClient, ILogger<TranslationController> logger)
    {
        _configuration = configuration;
        _httpClient = httpClient;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
    {
        if (request == null)
        {
            _logger.LogError("❌ Received null request body");
            return BadRequest(new { error = "Request body is missing" });
        }

        if (string.IsNullOrEmpty(request.Text) || string.IsNullOrEmpty(request.TargetLang))
        {
            _logger.LogError("❌ Invalid request data: {Request}", request);
            return BadRequest(new { error = "Text and targetLang are required." });
        }

        _logger.LogInformation("🔍 Received translation request: {Text} → {TargetLang}", request.Text, request.TargetLang);

        string apiKey = _configuration["DeepL:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogError("❌ DeepL API Key is missing in configuration.");
            return StatusCode(500, new { error = "DeepL API Key is missing" });
        }

        var formData = new Dictionary<string, string>
        {
            { "auth_key", apiKey },
            { "text", request.Text },
            { "target_lang", request.TargetLang }
        };

        try
        {
            var response = await _httpClient.PostAsync(
                "https://api-free.deepl.com/v2/translate",
                new FormUrlEncodedContent(formData)
            );

            response.EnsureSuccessStatusCode();
            var responseData = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("✅ Translation successful for text: {Text}", request.Text);
            
            return Ok(responseData);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError("❌ DeepL API Error: {Message}", ex.Message);
            return StatusCode(500, new { error = "Translation failed", details = ex.Message });
        }
    }
}

// ✅ Ensure request model matches frontend request body
public class TranslationRequest
{
    public string Text { get; set; }
    public string TargetLang { get; set; }
}
