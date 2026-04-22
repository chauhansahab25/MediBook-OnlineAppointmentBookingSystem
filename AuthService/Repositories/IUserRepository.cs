 
using AuthService.Entities;

namespace AuthService.Repositories;

public interface IUserRepository
{
    Task<User?> FindByEmail(string email);
    Task<User?> FindByUserId(int userId);
    Task<bool> ExistsByEmail(string email);
    Task<List<User>> FindAllByRole(string role);
    Task<User?> FindByPhone(string phone);
    Task<List<User>> FindByFullNameContaining(string namePart);
    Task<bool> DeleteByUserId(int userId);
    Task<User> AddUser(User user);
    Task<User> UpdateUser(User user);
}