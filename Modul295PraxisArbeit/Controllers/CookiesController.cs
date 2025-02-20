using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Net;

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

        // 🍪 User Accepts Cookies → Log Full Details
        [HttpPost("accept")]
        public IActionResult AcceptCookies()
        {
            try
            {
                // 📍 Get IP Address
                string userIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";

                // 🌍 Get User-Agent (OS, Browser, Device)
                string userAgent = Request.Headers["User-Agent"].ToString() ?? "Unknown User-Agent";

                // 🔗 Get Referrer (Where the user came from)
                string referrer = Request.Headers["Referer"].ToString() ?? "No Referrer";

                // 🆔 Generate a Session ID (Simple Random String)
                string sessionId = Guid.NewGuid().ToString();

                // 🕒 Timestamp
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");

                // 📜 Log Everything
                _logger.LogInformation($"✅ Cookies accepted | IP: {userIp} | User-Agent: {userAgent} | Referrer: {referrer} | SessionID: {sessionId} | Timestamp: {timestamp}");

                // 🍪 Set Secure Cookie (1 year expiry)
                Response.Cookies.Append("UserAcceptedCookies", "true", new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddYears(1),
                    HttpOnly = true, // Cannot be accessed via JavaScript
                    Secure = true,   // Only send over HTTPS
                    SameSite = SameSiteMode.Strict // Strict SameSite policy
                });

                // ✅ Return Data to Frontend
                return Ok(new
                {
                    message = "✅ Cookie consent recorded",
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
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        // 🍪 Check if cookies are accepted
        [HttpGet("status")]
        public IActionResult CheckCookieStatus()
        {
            if (Request.Cookies.ContainsKey("UserAcceptedCookies"))
            {
                return Ok(new { message = "✅ User has accepted cookies" });
            }
            return Ok(new { message = "❌ User has NOT accepted cookies" });
        }
    }
}
