using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.IdentityModel.Tokens;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        SqliteConnection connection;
        List<User> users;
        public AuthorizationController()
        {
            connection = new SqliteConnection("Data Source=mydb.db");
            users = new List<User>();
            fill_list();
        }
        [HttpPost]
        public IActionResult Login([FromBody]UserLogin user)
        {
            if (user == null)
            {
                return BadRequest("Invalid client request");
            }
            else
            {
                var uss = users.Where(x => x.Login == user.Login && x.Password == user.Password).ToList();
                if (uss.Count==1)
                {
                    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(user.Login+"ptakilatajakluczemsha256"+user.Password));
                    //var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
                    var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                    var tokeOptions = new JwtSecurityToken(
                        issuer: "http://localhost:44365",
                        audience: "http://localhost:44365",
                        claims: new List<Claim>(),
                        expires: DateTime.Now.AddMinutes(5),
                        signingCredentials: signinCredentials
                    );
                    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokeOptions);
                    return Ok(new { Token = tokenString });
                }
                else
                {
                    return Unauthorized();
                }
            }
        }
        // GET api/values
        [HttpGet/*, Authorize*/]
        [EnableCors("developerska")]
        //public ActionResult<string> Get()
        public ActionResult<IEnumerable<User>> Get()
        {
            return users;
        }
        // GET api/values/5
        [HttpGet("{id}"), Authorize]
        [EnableCors("developerska")]
        public ActionResult<User> Get(int id)
        {
            //return heroes.Single(h => h.Id == id);
            //int idx = id_valid(id);
            //if (idx > -1)
            //    return heroes[idx];
            //else return NotFound();
            return users[id];
        }
        void fill_list()
        {
            users.Clear();
            string cmd = "select * from users;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd, connection);
            connection.Open();
            SqliteDataReader data = sql_cmd.ExecuteReader();
            while (data.Read())
            {
                long _id = (long)data[0];
                string _login = (string)data[1];
                string _password = (string)data[2];
                string _type = (string)data[3];
                users.Add(new User { Id = _id, Login = _login, Password = _password, Type = _type });
            }
            order_id();
            connection.Close();
        }
        void order_id()
        {
            for (int i = 0; i < users.Count; i++)
                users[i].Id = i;
        }
        void save_all_changes()
        {
            string cmd_reset = "delete from users;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd_reset, connection);
            connection.Open();
            sql_cmd.ExecuteNonQuery();
            foreach (var user in users)
            {
                string cmd_ins = "insert into users(id,login,password,type) values(" + user.Id.ToString() + ",'" + user.Login + "','" + user.Password + "','" + user.Type + "');";
                SqliteCommand sql_cmd_in = new SqliteCommand(cmd_ins, connection);
                sql_cmd_in.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}