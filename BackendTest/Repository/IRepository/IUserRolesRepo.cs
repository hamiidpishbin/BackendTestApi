using BackendTest.Models;

namespace BackendTest.Repository.IRepository;

public interface IUserRolesRepo
{
    Task AssignUserRole(int userId);
    Task<List<string>> GetRoles(int userId);
}