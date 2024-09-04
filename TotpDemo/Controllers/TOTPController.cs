using Dapper;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using TotpDemo.Models;

namespace TotpDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TOTPController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection _connection;

        public TOTPController(IConfiguration config)
        {
            _config = config;
            _connection = new SqlConnection(_config.GetConnectionString("Default"));
        }

        /// <summary>
        /// Setup initail TOTP secret in the DB if the generated code is valid for the secret.
        /// </summary>
        [HttpPost("setup")]
        public IActionResult SetupTOTP([FromBody] SetupTOTPModel model)
        {
            try
            {
                if (new TOTPService().ValidateTOTP(model.Secret, model.Code))
                {
                    var sql = $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                              "UPDATE Users SET TOTPSecret = @Secret, IsTOTPEnabled = 1 WHERE Username = @Username";
                    _connection.Execute(sql, new { Username = model.Username, model.Secret });
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return BadRequest("Invalid Code for authentication. Please, try again.");
        }

        /// <summary>
        /// Confirms if the passed code as parameter is valid for the User's TOTPSecret in the DB.
        /// </summary>
        [HttpPost("confirm")]
        public IActionResult ConfirmTOTP([FromBody] ConfirmTOTPModel model)
        {
            var secret = _connection.QuerySingle<string>(
                $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                "SELECT TOTPSecret FROM Users WHERE Username = @Username",
                new { Username = model.Username });

            if (new TOTPService().ValidateTOTP(secret, model.Code))
            {
                return Ok();
            }

            return BadRequest("Invalid TOTP code.");
        }

        [HttpGet("is-enabled")]
        public IActionResult IsTOTPEnabled([FromQuery] string username)
        {
            var isEnabled = _connection.QuerySingle<bool>(
                $"USE {_config.GetRequiredSection("DB:Name").Value}; " +
                "SELECT IsTOTPEnabled FROM Users WHERE Username = @Username",
                new { username });

            if (isEnabled)
            {
                return Ok(new { Message = "TOTP is enabled for the user." });
            }

            return BadRequest(new { Error = "User has not enabled TOTP." });
        }
    }
}
