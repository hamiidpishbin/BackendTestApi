using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Repository.IRepository;

public interface IUserRepo
{
    Task<IEnumerable<User>> GetUsers();
    Task<User> CreateUser(UserDto userDto);
    Task<User> FindUserByUsername(string username);
    Task<User> FindUserById(int id);
    Task ChangePassword(int userId, string newPassword);
    Task DeleteUser(User user);
    Task AdminEditUser(int id, UserDto userDto);
}