using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Repository;

public interface IUserRepo
{
    Task<IEnumerable<User>> FindAllUsers();
    Task<User> CreateUser(UserDto user);
    Task<User> FindUserByUsername(string username);
    Task<User> FindUserById(int id);
    Task ChangePassword(int userId, string newPassword);
    Task DeleteUser(User user);
    Task AdminEditUser(int id, UserDto user);
    Task<List<string>> GetRoles(int userId);
}