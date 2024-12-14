using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Modul295PraxisArbeit.Data;
using Modul295PraxisArbeit.Models;
using Modul295PraxisArbeit.Services;
using System.Security.Cryptography;
using System.Text;

namespace Praxisarbeit_M295.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;

        public UsersController(ApplicationDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult<string>> Login([FromBody] UserLoginDto loginDto)
        {
            Console.WriteLine($"Name: {loginDto.Username}");
            var user = _context.Users
                .SingleOrDefault(u => u.Username == loginDto.Username);
            Console.WriteLine($"Name: {user.Username}");

            if (user == null || !VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized("Falscher Benutzername oder Passwort.");
            }

            var token = _jwtService.GenerateToken(user.Username, user.Role);
            return Ok(new { Token = token });
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<ActionResult<User>> Register([FromBody] UserRegisterDto registerDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
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

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Register), new { id = newUser.UserId }, newUser);
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users.ToListAsync();
            return Ok(users);
        }

        // Hilfsfunktionen für die Passwortverwaltung
        private string CreatePasswordHash(string password)
        {
            // Generiert den Hash mit bcrypt. Salt wird automatisch erstellt.
            return BCrypt.Net.BCrypt.HashPassword(password);
            /*
                        using (var hmac = new HMACSHA256())
                        {
                            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                        }*/
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            // bcrypt übernimmt das Vergleichen des Passworts mit dem gespeicherten Hash, einschließlich des Salts.
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
            /*
                        using (var hmac = new HMACSHA256())
                        {
                            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                            return Convert.ToBase64String(computedHash) == storedHash;
                        }*/
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
