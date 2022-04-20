using System.Data;
using BackendTest.Data;
using BackendTest.Dtos;
using BackendTest.Models;
using BackendTest.Repository.IRepository;
using Dapper;
using Microsoft.VisualBasic;

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

    
    
    public async Task<User> FindUserByUsername(string username)
    {
        var query = @"SELECT * FROM Users WHERE Username = @Username";

        using var connection = _dapperContext.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("Username", username, DbType.String);

        var user = await connection.QuerySingleOrDefaultAsync<User>(query, parameters);
        
        return user;
    }
    

    public async Task<User> FindUserById(int id)
    {
        var query = @"SELECT * FROM Users WHERE Id = @UserId";

        using var connection = _dapperContext.CreateConnection();

        var parameters = new DynamicParameters();
        parameters.Add("UserId", 
            id, DbType.Int32);

        var user = await connection.QuerySingleOrDefaultAsync<User>(query, parameters);
        
        return user;
    }

    public async Task ChangePassword(int userId, string newPassword)
    {
        var query = @"UPDATE Users SET [Password] = @Password WHERE Id = @UserId";

        var parameters = new DynamicParameters();
        parameters.Add("UserId", userId, DbType.Int32);
        parameters.Add("Password", newPassword, DbType.String);

        using var connection = _dapperContext.CreateConnection();

        await connection.ExecuteAsync(query, parameters);
    }



    public async Task DeleteUser(User user)
    {
        var query = @"DELETE FROM UserRoles WHERE UserId = @UserId;
                      DELETE FROM Users WHERE Id = @Id";

        var parameters = new DynamicParameters();
        parameters.Add("UserId", user.Id, DbType.Int32);
        parameters.Add("Id", user.Id, DbType.Int32);

        using var connection = _dapperContext.CreateConnection();

        await connection.ExecuteAsync(query, parameters);
    }

    
    
    public async Task AdminEditUser(int id, UserDto userDto)
    {
        var query = @"UPDATE Users SET Username = @Username, [Password] = @Password WHERE Id = @Id";

        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

        var parameters = new DynamicParameters();
        parameters.Add("Id", id, DbType.Int32);
        parameters.Add("Username", hashedPassword, DbType.String);
        parameters.Add("Password", userDto.Password, DbType.String);

        using var connection = _dapperContext.CreateConnection();

        await connection.ExecuteAsync(query, parameters);
    }
}