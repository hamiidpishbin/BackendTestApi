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

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
        
        var parameters = new DynamicParameters();
        parameters.Add("Username", userDto.Username, DbType.String);
        parameters.Add("Password", hashedPassword, DbType.String);
        
        using var connection = _dapperContext.CreateConnection();

        var createdUserId = await connection.QuerySingleAsync<int>(query, parameters);

        var createdUser = new User()
        {
            Id = createdUserId,
            Username = userDto.Username,
            Password = hashedPassword
        };

        return createdUser;
    }

    public async Task<User> CheckExistingUser(string username)
    {
        var query = @"SELECT * FROM Users WHERE Username = @Username";

        using var connection = _dapperContext.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("Username", username, DbType.String);

        var user = await connection.QuerySingleOrDefaultAsync<User>(query, parameters);
        
        return user;
    }
    
}