using System.Text;
using BackendTest.Data;
using BackendTest.Helpers;
using BackendTest.Middlewares;
using BackendTest.Repository;
using BackendTest.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ITokenManager, TokenManager>();
builder.Services.AddTransient<IMovieRepository, MovieRepository>();
builder.Services.AddTransient<ISearchParamsValidator, SearchParamsValidator>();
builder.Services.AddTransient<IMovieHelper, MovieHelper>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseJwtMiddleware();
app.MapControllers();

app.Run();