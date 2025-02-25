using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;

namespace Praxisarbeit_M295.Controllers
{
    [Route("api/cookies")]
    [ApiController]
    public class CookiesController : ControllerBase
    {
        private readonly ILogger<CookiesController> _logger;

        public CookiesController(ILogger<CookiesController> logger)
        {
            _logger = logger;
        }

        // Accept Cookies with granular consent (and store detailed data)
        [HttpPost("accept")]
        public IActionResult AcceptCookies([FromBody] CookieConsentModel consent)
        {
            try
            {
                // Log details and consent choices along with additional tracking info
                string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";
                string referrer = Request.Headers["Referer"].ToString() ?? "No Referrer";
                string sessionId = !string.IsNullOrEmpty(consent.Details.SessionId) 
                                   ? consent.Details.SessionId 
                                   : Guid.NewGuid().ToString();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                _logger.LogInformation($"✅ Cookies accepted | IP: {userIp} | User-Agent: {userAgent} | Consent: {consent.ToString()} | SessionID: {sessionId} | Timestamp: {timestamp}");

                // Store consent as a JSON string in a cookie (1 year expiry)
                Response.Cookies.Append("UserCookieConsent", 
                    Newtonsoft.Json.JsonConvert.SerializeObject(consent), 
                    new CookieOptions
                    {
                        Expires = DateTime.UtcNow.AddYears(1),
                        HttpOnly = false, // Accessible by client-side scripts to read preferences
                        Secure = true,    // Only send over HTTPS
                        SameSite = SameSiteMode.Strict
                    });

                return Ok(new
                {
                    accepted = consent,
                    message = "User has accepted cookies",
                    ip = userIp,
                    userAgent = userAgent,
                    sessionId = sessionId,
                    timestamp = timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error processing cookie consent: {ex.Message}");
                return StatusCode(500, new { accepted = false, message = "Internal server error" });
            }
        }

        // Check if cookies are accepted
        [HttpGet("status")]
        public IActionResult CheckCookieStatus()
        {
            if (Request.Cookies.ContainsKey("UserCookieConsent"))
            {
                var consentJson = Request.Cookies["UserCookieConsent"];
                var consent = Newtonsoft.Json.JsonConvert.DeserializeObject<CookieConsentModel>(consentJson);
                return Ok(new { accepted = consent, message = "User has accepted cookies" });
            }
            return Ok(new { accepted = (object)null, message = "User has NOT accepted cookies" });
        }

        // Clear cookie consent (to be called on logout)
        [HttpPost("clear")]
        public IActionResult ClearCookieConsent()
        {
            Response.Cookies.Delete("UserCookieConsent");
            return Ok(new { accepted = (object)null, message = "Cookie consent cleared" });
        }
    }

    // Model for detailed consent information
    public class ConsentDetails
    {
        // Descriptive details for each cookie category:
        public string AnalyticsData { get; set; } = "Pages visited, session duration, device info";
        public string PersonalizationData { get; set; } = "User preferences such as language, theme, layout";
        public string MarketingData { get; set; } = "Browsing behavior and interests for targeted ads";

        // Additional data you might log:
        public string SessionId { get; set; } = "Not provided"; // Session ID from client, if available
        public string AuthToken { get; set; } = "Not provided"; // Authentication token if relevant
        public string BasicSiteSettings { get; set; } = "Default"; // E.g. language, region, etc.
        public string VisitedPages { get; set; } = "None";         // Pages visited (if tracked)
        public string Duration { get; set; } = "0";                // Duration of visit
        public string DeviceInfo { get; set; } = "Unknown";        // Browser, OS, etc.
        public string Language { get; set; } = "en";               // User language
        public string Theme { get; set; } = "Default";             // Theme preference
        public string Layout { get; set; } = "Default";            // Layout preference
        public string Clicks { get; set; } = "0";                  // Number of clicks (if tracked)
    }

    // Model for cookie consent with granular options and additional details
    public class CookieConsentModel
    {
        // Necessary cookies are always enabled.
        public bool Necessary { get; set; } = true;
        public bool Analytics { get; set; }
        public bool Personalization { get; set; }
        public bool Marketing { get; set; }

        // Additional detailed data about what is tracked/collected.
        public ConsentDetails Details { get; set; } = new ConsentDetails();

        public override string ToString()
        {
            return $"Analytics: {Analytics}, Personalization: {Personalization}, Marketing: {Marketing}. " +
                   $"Details -> [Analytics: {Details.AnalyticsData}, Personalization: {Details.PersonalizationData}, Marketing: {Details.MarketingData}, " +
                   $"SessionId: {Details.SessionId}, AuthToken: {Details.AuthToken}, BasicSiteSettings: {Details.BasicSiteSettings}, " +
                   $"VisitedPages: {Details.VisitedPages}, Duration: {Details.Duration}, DeviceInfo: {Details.DeviceInfo}, " +
                   $"Language: {Details.Language}, Theme: {Details.Theme}, Layout: {Details.Layout}, Clicks: {Details.Clicks}]";
        }
    }
}
