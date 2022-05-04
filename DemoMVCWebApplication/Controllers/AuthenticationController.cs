using DemoMVCWebApplication.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Npgsql;
using System;

namespace DemoMVCWebApplication.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;
        private IConfiguration _configuration;
        public AuthenticationController(IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public IActionResult Login()
        {
            return View(new AuthenticationViewModel());
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            CustomRole roleViewModelTemp = new CustomRole();
            string cs = this._configuration.GetSection("ConnectionStrings").GetSection("AWS").Value;
            using var con = new NpgsqlConnection(cs);
            con.Open();

            var sql = "Select id from UM where username ='" + username + "' and userpassword = '" + password + "'";
            using var cmd = new NpgsqlCommand(sql, con);
            var userid = cmd.ExecuteScalar();
            if (userid == null)
            {
                throw new UnauthorizedAccessException("Invalid username or passwrod.");
            }

            sql = "SELECT * from Roles where role_id in (select roleid from userrole where userid =" + userid.ToString() + ")";

            cmd.CommandText = sql;
            NpgsqlDataReader npgsqlDataReader = cmd.ExecuteReader();
            RoleViewModel customRole = new RoleViewModel();
            if (npgsqlDataReader.HasRows)
            {
                while (npgsqlDataReader.Read())
                {
                    customRole.RoleId = npgsqlDataReader.GetInt64(0);
                    customRole.RoleName = npgsqlDataReader.GetString(1);
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            npgsqlDataReader.Close();
            TempData["Role"] = JsonConvert.SerializeObject(customRole);

            return RedirectToAction("Privacy", "Home");
        }
    }
}
