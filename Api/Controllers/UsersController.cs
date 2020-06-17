using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Api.Models;
using Microsoft.AspNetCore.Cors;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        SqliteConnection connection;
        List<User>users;
        public UsersController()
        {
            connection = new SqliteConnection("Data Source=mydb.db");
            users = new List<User>();
        }
        //[HttpGet("{login},{password}")]
        //[EnableCors("developerska")]
        //public bool Get(string login,string password)
        //{
        //    var user = users.Where(x => x.Login == login && x.Password == password);
        //    if (user.ToList().Count == 1) return true;
        //    else return false;
        //}
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
                users.Add(new User{ Id = _id,Login=_login,Password=_password,Type=_type });
            }
            order_id();
            connection.Close();
        }
        void order_id()
        {
            for (int i = 0; i < users.Count; i++)
                users[i].Id = i;
        }
        //int id_valid(int id)
        //{
        //    for (int i = 0; i < users.Count; i++)
        //    {
        //        if (users[i].Id == id)
        //            return i;
        //    }
        //    return -1;
        //}
        void save_all_changes()
        {
            string cmd_reset = "delete from users;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd_reset, connection);
            connection.Open();
            sql_cmd.ExecuteNonQuery();
            foreach (var user in users)
            {
                string cmd_ins = "insert into users(id,login,password,type) values(" + user.Id.ToString() + ",'" + user.Login + "','" + user.Password + "','"+user.Type+"');";
                SqliteCommand sql_cmd_in = new SqliteCommand(cmd_ins, connection);
                sql_cmd_in.ExecuteNonQuery();
            }
            connection.Close();
        }
    }
}