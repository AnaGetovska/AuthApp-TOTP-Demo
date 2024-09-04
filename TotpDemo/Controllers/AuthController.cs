using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using TotpDemo.Models;

namespace TotpDemo.Controllers
{
    [ApiController]
    [Route("api")]
    public class AuthController : ControllerBase
    {
        private readonly SqlConnection _connection;
        private readonly IConfiguration _config;
        private readonly PasswordService _passwordService;

        public AuthController(IConfiguration config, PasswordService PasswordService)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("Default"));
            _passwordService = PasswordService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            // Validate username and password logic here...
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                      "SELECT Id FROM Users WHERE Username = @Username AND PasswordHash = @PasswordHash";
            var id = _connection.QuerySingleOrDefault<string>(sql, new { Username = model.Username, PasswordHash = hashedPass });

            if (id != null)
            {
                return Ok();
            }

            return StatusCode(500);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterModel model)
        {
            var hashedPass = _passwordService.HashPassword(model.Password);
            var sql = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";

            using (var connection = new SqlConnection(_config.GetConnectionString("Default")))
            {
                try
                {
                    var sqlUserExists = $"USE {_config.GetRequiredSection("DB:Name").Value};" + "SELECT * FROM Users WHERE Username = @Username";
                    var existingUser = connection.QuerySingleOrDefault(sqlUserExists, new { model.Username });

                    if (existingUser != null)
                    {
                        return BadRequest("Username already exists.");
                    }

                    connection.Execute(sql, new { model.Username, PasswordHash = hashedPass });

                    return Ok("User registered successfully.");
                }
                catch (SqlException ex)
                {
                    return StatusCode(500, $"Internal server error: {ex.Message}");
                }
            }
        }
    }
}
