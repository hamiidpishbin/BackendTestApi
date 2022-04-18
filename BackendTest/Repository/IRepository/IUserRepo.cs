using BackendTest.Dtos;
using BackendTest.Models;

namespace BackendTest.Repository.IRepository;

public interface IUserRepo
{
    Task<IEnumerable<User>> GetUsers();
    Task<User> CreateUser(UserDto userDto);
    Task<User> CheckDuplicateUser(UserDto userDto);
}