using System.Data;
using System.Runtime.CompilerServices;
using BackendTest.Data;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Dapper;

namespace BackendTest.Repository;

public class UserRolesRepo : IUserRolesRepo
{
    private readonly string DbUserId = "1";
    
    private readonly DapperContext _dapperContext;

    public UserRolesRepo(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }
    
    
    public async Task AssignUserRole(int userId)
    {
        var query = $"INSERT INTO UserRoles (UserId, RoleId) VALUES (@UserId, {DbUserId})";
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Int32);
        
        using var connection = _dapperContext.CreateConnection();

        await connection.ExecuteAsync(query, parameters);
    }

    
    
    
    
    public async Task<List<string>> GetRoles(int userId)
    {
        var query = @"SELECT Role FROM UserRoles INNER JOIN Roles ON UserRoles.RoleId = Roles.Id WHERE UserId = @UserId";
        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        var roles = await connection.QueryAsync<RoleModel>(query, parameters);
        var rolesList = new List<string>();
        foreach (var roleObj in roles)
        {
            rolesList.Add(roleObj.Role);
        }

        return rolesList;
    }
}