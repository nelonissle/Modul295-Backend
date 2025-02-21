using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Modul295PraxisArbeit.Models;
using Modul295PraxisArbeit.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Text.RegularExpressions;
using OtpNet; // ✅ Required for TOTP
using QRCoder; // ✅ Required for QR Code Generation
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SkiaSharp;
using QRCoder;
using System.IO; // Required for MemoryStream
using System.Drawing; // Required for Bitmap (if using System.Drawing)
using System.Drawing.Imaging;
using System.Net.Mail;
using System.Net;
using System.Security.Claims; // Required for ImageFormat



namespace Praxisarbeit_M295.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<OrderUser> _usersCollection;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMongoDatabase database, IJwtService jwtService, ILogger<UsersController> logger)
        {
            _usersCollection = database.GetCollection<OrderUser>("Users") ?? throw new ArgumentNullException(nameof(database), "MongoDB Database is null.");
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService), "JwtService is null.");
            _logger = logger ?? throw new ArgumentNullException(nameof(logger), "Logger is null.");
        }


        // 🛠 REGISTER NEW USER WITH PASSWORD VALIDATION & JWT TOKEN
        [HttpPost("Register")]
        public async Task<ActionResult<object>> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Password))
                {
                    _logger.LogWarning("❌ Registration failed: Username or password is empty.");
                    return BadRequest(new { message = "❌ Username and password are required." });
                }

                // 🔒 Validate Password Strength
                if (!IsValidPassword(registerDto.Password))
                {
                    _logger.LogWarning($"❌ Registration failed: Weak password for user '{registerDto.Username}'.");
                    return BadRequest(new { message = "❌ Password must be at least 15 characters long and contain a letter, a number, and a special character." });
                }

                var userExists = await _usersCollection.Find(u => u.Username == registerDto.Username).AnyAsync();
                _logger.LogInformation($"🔍 Checking if username exists: {registerDto.Username} -> Exists: {userExists}");

                if (userExists)
                {
                    _logger.LogWarning($"⚠️ Registration failed: Username '{registerDto.Username}' is already taken.");
                    return BadRequest(new { message = "❌ Username already exists." });
                }

                string passwordHash = CreatePasswordHash(registerDto.Password);

                var newUser = new OrderUser
                {
                    Username = registerDto.Username,
                    PasswordHash = passwordHash,
                    Role = string.IsNullOrEmpty(registerDto.Role) ? "Kunde" : registerDto.Role // Default to "Kunde"
                };

                _logger.LogInformation($"📝 Inserting new user: {registerDto.Username} with role {newUser.Role}");
                await _usersCollection.InsertOneAsync(newUser);

                _logger.LogInformation($"✅ User '{registerDto.Username}' registered successfully.");

                // ✅ Generate JWT Token 🔥
                var jwtService = HttpContext.RequestServices.GetRequiredService<IJwtService>();
                string token = jwtService.GenerateToken(newUser.Username, newUser.Role);

                _logger.LogInformation($"🔑 Token generated successfully for '{registerDto.Username}'.");

                return CreatedAtAction(nameof(Register), new { id = newUser.Id }, new { token });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error during registration: {ex.Message}");
                return StatusCode(500, new { message = "❌ Internal server error.", error = ex.Message });
            }
        }
        [Authorize] // ✅ Ensure only authenticated users can enable 2FA
        [HttpPost("2fa/enable-email")]
        public async Task<IActionResult> EnableEmail2FA()
        {
            try
            {
                // ✅ Extract username from JWT token
                var claimsIdentity = HttpContext.User.Identity as ClaimsIdentity;
                var username = claimsIdentity?.FindFirst(ClaimTypes.Name)?.Value;

                if (string.IsNullOrEmpty(username))
                {
                    _logger.LogWarning("❌ Unauthorized request. No username found in token.");
                    return Unauthorized(new { message = "❌ Unauthorized request. Please log in again." });
                }

                // ✅ Find user in database
                var user = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
                if (user == null)
                {
                    _logger.LogWarning($"❌ User '{username}' not found in database.");
                    return NotFound(new { message = "❌ User not found." });
                }

                // ✅ Enable 2FA for the user
                var update = Builders<OrderUser>.Update.Set(u => u.TwoFactorEnabled, true);
                await _usersCollection.UpdateOneAsync(u => u.Username == username, update);

                _logger.LogInformation($"✅ 2FA enabled for user: {username}");
                return Ok(new { message = "✅ 2FA enabled successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error enabling 2FA: {ex.Message}");
                return StatusCode(500, new { message = "❌ Internal server error." });
            }
        }

        // ✅ Send 2FA Code via Email
        [HttpPost("2fa/send-email")]
        public async Task<IActionResult> Send2FACode([FromBody] Send2FARequest request)
        {
            var user = await _usersCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync();
            if (user == null || !user.TwoFactorEnabled)
                return BadRequest(new { message = "❌ User not found or 2FA is not enabled." });

            var random = new Random();
            string code = random.Next(100000, 999999).ToString();

            var update = Builders<OrderUser>.Update.Set(u => u.TwoFactorCode, code);
            await _usersCollection.UpdateOneAsync(u => u.Username == request.Username, update);

            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("nelonissle@gmail.com", "----------"), // ✅ Add your email and password
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress("nelonissle@gmail.com"),
                    Subject = "Ihr 2FA Code",
                    Body = $"Ihr 2FA-Code lautet: {code}",
                    IsBodyHtml = false,
                };

                mailMessage.To.Add(user.Username);
                await smtpClient.SendMailAsync(mailMessage);
                return Ok(new { message = "✅ 2FA code sent to your email." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "❌ Error sending email.", error = ex.Message });
            }
        }

        // ✅ Verify Email-Based 2FA
        [HttpPost("2fa/verify-email")]
        public async Task<IActionResult> Verify2FAEmail([FromBody] Verify2FARequest request)
        {
            var user = await _usersCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync();

            if (user == null || user.TwoFactorCode != request.Code)
                return Unauthorized(new { message = "❌ Invalid 2FA Code." });

            string token = _jwtService.GenerateToken(user.Username, user.Role);
            return Ok(new { token });
        }

        [HttpPost("Login")]
        public async Task<ActionResult<object>> Login([FromBody] UserLoginDto loginDto)
        {
            _logger.LogInformation($"🔍 Login attempt for username: {loginDto.Username}");

            var user = await _usersCollection.Find(u => u.Username == loginDto.Username).SingleOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning($"⚠️ Login failed - Username '{loginDto.Username}' not found.");
                return Unauthorized(new { message = "❌ Invalid username or password." });
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning($"🔒 Password verification failed for user '{loginDto.Username}'");
                return Unauthorized(new { message = "❌ Invalid username or password." });
            }

            // ✅ Check if 2FA is enabled for the user
            if (user.TwoFactorEnabled)
            {
                _logger.LogInformation($"🔒 2FA required for user '{user.Username}'");
                return Ok(new { requires2FA = true, username = user.Username });
            }

            // ✅ If 2FA is NOT enabled, issue a JWT token directly
            var token = _jwtService.GenerateToken(user.Username, user.Role);
            _logger.LogInformation($"✅ Login successful for user '{user.Username}'");

            return Ok(new { token });
        }

        [HttpGet("2fa/status/{username}")]
        public async Task<IActionResult> Get2FAStatus(string username)
        {
            var user = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null) return NotFound(new { message = "User not found" });

            return Ok(new { twoFactorEnabled = user.TwoFactorEnabled });
        }


        [HttpPost("2fa/disable/{username}")]
        public async Task<IActionResult> Disable2FA(string username)
        {
            var user = await _usersCollection.Find(u => u.Username == username).FirstOrDefaultAsync();
            if (user == null) return NotFound(new { message = "User not found" });

            var update = Builders<OrderUser>.Update
                .Set(u => u.TwoFactorEnabled, false)
                .Set(u => u.TwoFactorSecret, null); // Remove secret

            await _usersCollection.UpdateOneAsync(u => u.Username == username, update);
            return Ok(new { success = true, message = "2FA disabled successfully" });
        }


        // 🔍 GET ALL USERS (ADMIN ONLY)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderUser>>> GetUsers()
        {
            _logger.LogInformation("Fetching all users (Admin Only)");
            var users = await _usersCollection.Find(u => true).ToListAsync();
            return Ok(users);
        }

        // 🔄 UPDATE USER ROLE (ADMIN ONLY)
        [Authorize(Roles = "Admin")]
        [HttpPut("UpdateRole/{username}")]
        public async Task<IActionResult> UpdateUserRole(string username, [FromBody] UpdateRoleDto updateDto)
        {
            _logger.LogInformation($"🔄 Updating role for user: {username} -> New Role: {updateDto.Role}");

            var filter = Builders<OrderUser>.Filter.Eq(u => u.Username, username);
            var update = Builders<OrderUser>.Update.Set(u => u.Role, updateDto.Role);
            var result = await _usersCollection.UpdateOneAsync(filter, update);

            if (result.ModifiedCount > 0)
            {
                _logger.LogInformation($"✅ Successfully updated role for user '{username}' to '{updateDto.Role}'");
                return Ok(new { message = "✅ User role updated successfully." });
            }

            _logger.LogWarning($"⚠️ Failed to update role for '{username}'. User not found.");
            return NotFound(new { message = "❌ User not found." });
        }

        // 🗑 DELETE USER (ADMIN ONLY)
        [Authorize(Roles = "Admin")]
        [HttpDelete("Delete/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            _logger.LogInformation($"🗑 Deleting user: {username}");

            var result = await _usersCollection.DeleteOneAsync(u => u.Username == username);

            if (result.DeletedCount > 0)
            {
                _logger.LogInformation($"✅ User '{username}' deleted successfully.");
                return Ok(new { message = "✅ User deleted successfully." });
            }

            _logger.LogWarning($"⚠️ Failed to delete user '{username}'. User not found.");
            return NotFound(new { message = "❌ User not found." });
        }

        [HttpPost("2fa/verify")]
        public async Task<IActionResult> Verify2FACode([FromBody] Verify2FARequest request)
        {
            _logger.LogInformation($"🔍 Incoming 2FA Verification for User: {request?.Username}");

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Code))
            {
                _logger.LogWarning("⚠️ Bad Request: Username or Code is missing.");
                return BadRequest(new { message = "❌ Invalid request. Username and code are required." });
            }

            var user = await _usersCollection.Find(u => u.Username == request.Username).FirstOrDefaultAsync();

            if (user == null || string.IsNullOrEmpty(user.TwoFactorSecret))
            {
                _logger.LogWarning($"⚠️ Unauthorized: User {request.Username} not found or no 2FA enabled.");
                return Unauthorized(new { message = "❌ User or 2FA secret not found." });
            }

            _logger.LogInformation($"🔍 Stored 2FA Secret: {user.TwoFactorSecret}");
            _logger.LogInformation($"🔍 Provided 2FA Code: {request.Code}");

            var otpKey = Base32Encoding.ToBytes(user.TwoFactorSecret);
            var totp = new Totp(otpKey);
            bool isValid = totp.VerifyTotp(request.Code, out long timeStepMatched, VerificationWindow.RfcSpecifiedNetworkDelay);

            if (!isValid)
            {
                _logger.LogWarning($"❌ Invalid 2FA Code for user {request.Username}");
                return Unauthorized(new { message = "❌ Invalid 2FA Code." });
            }

            // ✅ Generate JWT Token after 2FA verification
            var token = _jwtService.GenerateToken(user.Username, user.Role);
            _logger.LogInformation($"✅ 2FA Verification successful for user {request.Username}");

            return Ok(new { token });
        }



        // ✅ Request model for 2FA verification
        public class Verify2FARequest
        {
            public string Username { get; set; }
            public string Code { get; set; }
        }

        // DTOs
        public class Send2FARequest
        {
            public string Username { get; set; }
        }

        // 🔐 HASH PASSWORD
        private string CreatePasswordHash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        // 🔑 VERIFY PASSWORD
        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        private bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("❌ Password validation failed: Password is empty.");
                return false;
            }

            if (password.Length < 15)
            {
                _logger.LogWarning("❌ Password validation failed: Password is too short (< 15 characters).");
                return false;
            }

            if (!Regex.IsMatch(password, @"[A-Za-z]"))
            {
                _logger.LogWarning("❌ Password validation failed: Password does not contain a letter.");
                return false;
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                _logger.LogWarning("❌ Password validation failed: Password does not contain a number.");
                return false;
            }

            if (!Regex.IsMatch(password, @"[\W_]")) // Sonderzeichen
            {
                _logger.LogWarning("❌ Password validation failed: Password does not contain a special character.");
                return false;
            }

            _logger.LogInformation("✅ Password validation passed.");
            return true;
        }

    }

    // DTOs
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Kunde"; // Default to "Kunde"
    }

    public class UpdateRoleDto
    {
        public string Role { get; set; }
    }
}



/*
Test für Login 
email: Martin@gmail.com
Password: bASEL-sTADT1893

Test für admin Login
email: Marcel@gmail.com
Password: bASEL-sTADT1893



Admin page geht nicht 
Muss behoben werden!!!!
*/