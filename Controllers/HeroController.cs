﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Api.Models;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroController : ControllerBase
    {
        SqliteConnection connection;
        List<Hero> heroes;
        public HeroController()
        {
            connection = new SqliteConnection("Data Source=mydb.db");
            heroes = new List<Hero>();
            fill_list();
        }
        ~HeroController()
        {
            string cmd_reset = "delete from heroes;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd_reset, connection);
            connection.Open();
            sql_cmd.ExecuteNonQuery();
            foreach(var her in heroes)
            {
                string cmd_ins="insert into heroes(id,nick,avatar,class) values("+her.Id.ToString()+",'"+her.Nick+"','"+her.Img+"','"+her.Prof+"');";
                SqliteCommand sql_cmd_in = new SqliteCommand(cmd_ins, connection);
                sql_cmd_in.ExecuteNonQuery();
            }
            connection.Close();
        }
        // GET api/values
        [HttpGet]
        //public ActionResult<string> Get()
        public ActionResult<IEnumerable<Hero>> Get()
        {
            return heroes;
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<Hero> Get(int id)
        {
            //return heroes.Single(h => h.Id == id);
            int idx = id_valid(id);
            if (idx > -1)
                return heroes[idx];
            else return NotFound();
        }
        // POST api/values
        [HttpPost]
        public void Post([FromBody] Hero value)
        {
            //int next_id = heroes.Max(h => h.Id) + 1;
            //value.Id = next_id;
            //heroes.Add(value);
            heroes.Add(value);
            order_id();
        }
        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Hero value)
        {
            //int index = heroes.FindIndex(h => h.Id == id);
            //heroes[index] = value;
            //heroes[index].Id = id;
            int idx = id_valid(id);
            if (idx >= 0)
            {
                heroes[idx] = value;
                heroes[idx].Id = id;
            }
            else NotFound();
        }
        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            //heroes.Remove(heroes.Single(h => h.Id == id));
            //order_id();
            int idx = id_valid(id);
            if (idx >= 0)
            {
                heroes.RemoveAt(idx);
                order_id();
            }
            else
                NotFound();
        }
        void fill_list()
        {
            heroes.Clear();
            string cmd = "select * from heroes;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd, connection);
            connection.Open();
            SqliteDataReader data = sql_cmd.ExecuteReader();
            while (data.Read())
            {
                long _id = (long)data[0];
                string _nick = (string)data[1];
                string _avatar = (string)data[2];
                string _class = (string)data[3];
                heroes.Add(new Hero { Id = (int)_id, Nick = _nick, Img = _avatar, Prof = _class });
            }
            order_id();
            connection.Close();
        }
        void order_id()
        {
            for (int i = 0; i < heroes.Count; i++)
                heroes[i].Id = i;
        }
        int id_valid(int id)
        {
            for(int i = 0; i < heroes.Count; i++)
            {
                if (heroes[i].Id == id)
                    return i;
            }
            return -1;
        }
    }
}