using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using Modul295PraxisArbeit.Models;
using Modul295PraxisArbeit.Services;
using System.Linq;
using System.Threading.Tasks;

namespace Praxisarbeit_M295.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IMongoCollection<User> _usersCollection;
        private readonly IJwtService _jwtService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IMongoDatabase database, IJwtService jwtService, ILogger<UsersController> logger)
        {
            _usersCollection = database.GetCollection<User>("Users");
            _jwtService = jwtService;
            _logger = logger;
        }

        // Prüfe ob richtige Rolle
        public bool CheckEditRole(string username)
        {
            _logger.LogInformation($"Check Edit Role of User: {username}");
            var user = _usersCollection.Find(u => u.Username == username).SingleOrDefault();
            if (user != null)
            {
                if (user.Role == "Mitarbeiter" || user.Role == "Admin")
                    return true;
            }
            return false;
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDto loginDto)
        {
            _logger.LogInformation("Controller: Post Login");
            _logger.LogDebug($"Name: {loginDto.Username}");

            var user = _usersCollection.Find(u => u.Username == loginDto.Username).SingleOrDefault();

            if (user == null || !VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                _logger.LogError("Fehler bei Login - Password nicht OK");
                return Unauthorized("Falscher Benutzername oder Passwort.");
            }

            var token = _jwtService.GenerateToken(user.Username, user.Role);
            return Ok(new { Token = token });
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDto registerDto)
        {
            var existingUser = _usersCollection.Find(u => u.Username == registerDto.Username).SingleOrDefault();
            if (existingUser != null)
            {
                return BadRequest("Benutzername bereits vergeben.");
            }

            var passwordHash = CreatePasswordHash(registerDto.Password);

            var newUser = new User
            {
                Username = registerDto.Username,
                PasswordHash = passwordHash,
                Role = "Kunde"
            };

            await _usersCollection.InsertOneAsync(newUser);

            return CreatedAtAction(nameof(Register), new { id = newUser.UserId }, newUser);
        }

        // GET: api/Users
        [Authorize] // Autorisierung erforderlich
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _usersCollection.Find(u => true).ToListAsync();
            return Ok(users);
        }

        // Hilfsfunktionen für die Passwortverwaltung
        private string CreatePasswordHash(string password)
        {
            // Generiert den Hash mit bcrypt. Salt wird automatisch erstellt.
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // bcrypt übernimmt das Vergleichen des Passworts mit dem gespeicherten Hash, einschließlich des Salts.
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }

    // DTOs (Datenobjekte für Login/Registrierung)
    public class UserLoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class UserRegisterDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
