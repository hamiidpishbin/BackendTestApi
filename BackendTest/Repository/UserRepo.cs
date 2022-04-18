using System.Data;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Dapper;

namespace BackendTest.Repository;

public class UserRepo : IUserRepo
{
    private readonly DapperContext _dapperContext;

    public UserRepo(DapperContext dapperContext)
    {
        _dapperContext = dapperContext;
    }
    
    public async Task<IEnumerable<User>> GetUsers()
    {
        var query = @"SELECT * FROM Users";
        
        using var connection = _dapperContext.CreateConnection();

        var users = await connection.QueryAsync<User>(query);

        return users;
    }

    public async Task<User> CreateUser(UserDto userDto)
    {
        var query = @"INSERT INTO Users (Username, Password) VALUES (@Username, @Password)" + @"SELECT CAST(SCOPE_IDENTITY() as int)";

        
        var parameters = new DynamicParameters();
        parameters.Add("Username", userDto.UserName, DbType.String);
        parameters.Add("Password", userDto.Password, DbType.String);
        
        using var connection = _dapperContext.CreateConnection();

        var createdUserId = await connection.QuerySingleAsync<int>(query, parameters);

        var createdUser = new User()
        {
            Id = createdUserId,
            UserName = userDto.UserName
        };

        return createdUser;
    }
}