using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Text.Json;  // Ensure System.Text.Json is being used for deserialization
using System.Threading.Tasks.Dataflow;

namespace DeeplTranslationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranslationController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _deeplApiKey;

        public TranslationController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _deeplApiKey = configuration["DeepL:ApiKey"] ?? throw new ArgumentNullException("DeepL API key not configured.");
        }

        [HttpPost("translate")]
        public async Task<IActionResult> Translate([FromBody] TranslationRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(request.Text) || string.IsNullOrWhiteSpace(request.TargetLang))
            {
                return BadRequest("Text and TargetLang are required fields.");
            }

            Console.WriteLine("Got the text");

            var deeplUrl = "http://api-free.deepl.com/v2/translate";
            var postData = new StringContent(
                $"auth_key={_deeplApiKey}&text={request.Text}&target_lang={request.TargetLang}&source_lang={request.SourceLang}",
                Encoding.UTF8, "application/x-www-form-urlencoded");

            try
            {
                // Send the request to the Deepl API
                var response = await _httpClient.PostAsync(deeplUrl, postData);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return StatusCode((int)response.StatusCode, $"DeepL API error: {errorMessage}");
                }

                // Read the response from Deepl and inspect it
                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Log or inspect the raw JSON response for debugging purposes
                Console.WriteLine($"Deepl Response: {jsonResponse}");

                var result = JsonSerializer.Deserialize<TranslationResponse>(jsonResponse);

                // Check if response contains valid translation
                if (result?.Translations == null || result.Translations.Length == 0)
                {
                    return StatusCode(500, "Translation response was invalid or empty.");
                }

                // Return the translated text
                return Ok(new { TranslatedText = result.Translations[0].Text });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error communicating with Deepl API: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing Deepl API response: {ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }
    }

    // Request DTO
    public class TranslationRequest
    {
        public string Text { get; set; }
        public string SourceLang { get; set; }
        public string TargetLang { get; set; }
    }

    // Response DTO
    public class TranslationResponse
    {
        public TranslationResult[] Translations { get; set; }
    }

    // Result DTO (Individual translation result)
    public class TranslationResult
    {
        public string Text { get; set; }
    }
}
