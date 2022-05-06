using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Repository;

public interface IUserRepository
{
    Task<List<User>> FindAllUsers();
    Task<CreatedUserDto> CreateUser(UserDto user);
    Task<User> FindUserByUsername(string username);
    Task<User> FindUserById(int id);
    Task ChangePassword(int userId, string newPassword);
    Task DeleteUser(int userId);
    Task AdminUpdateUser(int id, UserDto user);
    Task<List<string>> GetUserRoles(int userId);
    Task InsertIntoUserRolesTable(int userId);
}