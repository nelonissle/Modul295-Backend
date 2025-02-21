using System.Threading.Tasks;

namespace Modul295PraxisArbeit.Models
{

public interface IUserService
{
    Task<bool> UserExists(string username);
    Task CreateUser(string username, string password, string role);
    Task<OrderUser> GetUserByUsername(string username); // ✅ Changed from User → OrderUser
    Task UpdateUserRole(string username, string role);
}

}