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

        // 🔍 Check if user has edit rights
        public bool CheckEditRole(string username)
        {
            _logger.LogInformation($"Checking edit role for user: {username}");
            var user = _usersCollection.Find(u => u.Username == username).SingleOrDefault();
            return user != null && (user.Role == "Mitarbeiter" || user.Role == "Admin");
        }

        // 🛠 REGISTER NEW USER
        [HttpPost("Register")]
        public async Task<ActionResult<OrderUser>> Register([FromBody] UserRegisterDto registerDto)
        {
            try
            {
                if (string.IsNullOrEmpty(registerDto.Username) || string.IsNullOrEmpty(registerDto.Password))
                {
                    return BadRequest(new { message = "Username and password are required." });
                }

                var userExists = await _usersCollection.Find(u => u.Username == registerDto.Username).AnyAsync();
                _logger.LogInformation($"Checking if username exists: {registerDto.Username} -> Exists: {userExists}");

                if (userExists)
                {
                    _logger.LogWarning($"⚠️ Username '{registerDto.Username}' is already taken.");
                    return BadRequest(new { message = "Username already exists." });
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
                return CreatedAtAction(nameof(Register), new { id = newUser.Id }, newUser);
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error during registration: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error.", error = ex.Message });
            }
        }

        // 🔑 LOGIN USER
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDto loginDto)
        {
            _logger.LogInformation($"🔍 Login attempt for username: {loginDto.Username}");

            var user = await _usersCollection.Find(u => u.Username == loginDto.Username).SingleOrDefaultAsync();

            if (user == null)
            {
                _logger.LogWarning($"⚠️ Login failed - Username '{loginDto.Username}' not found.");
                return Unauthorized("Invalid username or password.");
            }

            if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning($"🔒 Password verification failed for user '{loginDto.Username}'");
                return Unauthorized("Invalid username or password.");
            }

            var token = _jwtService.GenerateToken(user.Username, user.Role);
            _logger.LogInformation($"✅ Login successful for user '{user.Username}'");
            return Ok(new { Token = token });
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
                return Ok(new { message = "User role updated successfully." });
            }

            _logger.LogWarning($"⚠️ Failed to update role for '{username}'. User not found.");
            return NotFound(new { message = "User not found." });
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
                return Ok(new { message = "User deleted successfully." });
            }

            _logger.LogWarning($"⚠️ Failed to delete user '{username}'. User not found.");
            return NotFound(new { message = "User not found." });
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
