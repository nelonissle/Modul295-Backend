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

        // 🍪 User Accepts Cookies → Log Full Details and set secure cookie
        [HttpPost("accept")]
        public IActionResult AcceptCookies()
        {
            try
            {
                string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";
                string referrer = Request.Headers["Referer"].ToString() ?? "No Referrer";
                string sessionId = Guid.NewGuid().ToString();
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                _logger.LogInformation($"✅ Cookies accepted | IP: {userIp} | User-Agent: {userAgent} | Referrer: {referrer} | SessionID: {sessionId} | Timestamp: {timestamp}");

                // Set secure cookie (1 year expiry)
                Response.Cookies.Append("UserAcceptedCookies", "true", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true, // Cannot be accessed via JavaScript
                    Secure = true,   // Only send over HTTPS
                    SameSite = SameSiteMode.Strict // Strict SameSite policy
                });

                return Ok(new
                {
                    accepted = true,
                    message = "User has accepted cookies",
                    ip = userIp,
                    userAgent = userAgent,
                    referrer = referrer,
                    sessionId = sessionId,
                    timestamp = timestamp
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error while processing cookie consent: {ex.Message}");
                return StatusCode(500, new { accepted = false, message = "Internal server error" });
            }
        }

        // 🍪 Check if cookies are accepted
        [HttpGet("status")]
        public IActionResult CheckCookieStatus()
        {
            bool accepted = Request.Cookies.ContainsKey("UserAcceptedCookies");
            return Ok(new
            {
                accepted,
                message = accepted ? "User has accepted cookies" : "User has NOT accepted cookies"
            });
        }

        // 🍪 Clear cookie consent (to be called on logout)
        [HttpPost("clear")]
        public IActionResult ClearCookieConsent()
        {
            Response.Cookies.Delete("UserAcceptedCookies");
            return Ok(new { accepted = false, message = "Cookie consent cleared" });
        }
    }
}
