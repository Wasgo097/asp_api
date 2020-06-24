using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Api.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeroesController : ControllerBase
    {
        SqliteConnection connection;
        List<Hero> heroes;
        public HeroesController()
        {
            connection = new SqliteConnection("Data Source=mydb.db");
            heroes = new List<Hero>();
            fill_list();
        }
        /// <summary>
        /// Returns all heroes
        /// </summary>
        /// <response code="200">Returns all heroes from base</response>
        /// <response code="401">User isn't login</response>
        [HttpGet, Authorize/*(Roles ="casual"),Authorize(Roles ="admin")*/]
        [EnableCors("developerska")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public ActionResult<IEnumerable<Hero>> Get()
        {
            return heroes;
        }
        ///// <remarks>
        ///// Sample request:
        /////     GET
        /////     {
        /////        id:1
        /////     }
        ///// </remarks>
        /// <summary>
        /// Returns hero from id position
        /// </summary>
        /// <param name="id"></param>
        /// <response code="200">Returns specific hero from base</response>
        /// <response code="401">User isn't login</response>
        /// <response code="404">Id is invalid</response>
        [HttpGet("{id}"), Authorize/*(Roles = "casual"), Authorize(Roles = "admin")*/]
        [EnableCors("developerska")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Hero> Get(int id)
        {
            //return heroes.Single(h => h.Id == id);
            int idx = id_valid(id);
            if (idx > -1)
                return heroes[idx];
            else return NotFound();
        }
        /// <remarks>
        /// Sample request:
        ///     POST /Hero
        ///     {
        ///         Id:0,
        ///        "Nick":"abc",
        ///        "Img":"cba",
        ///        "Prof":"xyz"
        ///     }
        /// </remarks>
        /// <summary>
        /// Adds a specific hero on last position in base
        /// </summary>
        /// <param name="value"></param>
        /// <response code="200">Returns true after added hero to base</response>
        /// <response code="401">User isn't login</response>
        /// <response code="404">Id is invalid</response>
        [HttpPost, Authorize(Roles = "admin")]
        [EnableCors("developerska")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public bool Post([FromBody] Hero value)
        {
            heroes.Add(value);
            order_id();
            insert_Hero(heroes.Last());
            return true;
        }
        /// <remarks>
        /// Sample request:
        ///     PUT /Hero
        ///     {   
        ///         "Id": 0,
        ///         "Nick": "string",
        ///         "Img": "string",
        ///         "Prof": "string"
        ///     }
        /// </remarks>
        /// <summary>
        /// Changes a specific hero.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="value"></param>
        /// <response code="200">Returns true after founded and changed hero, when id is invalid returns false</response>
        /// <response code="401">User isn't login</response>
        /// <response code="404">Id is invalid</response>
        [HttpPut("{id}"), Authorize(Roles = "admin")]
        [EnableCors("developerska")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public bool Put(int id, [FromBody] Hero value)
        {
            int idx = id_valid(id);
            if (idx >= 0)
            {
                heroes[idx] = value;
                heroes[idx].Id = id;
                save_all_changes();
                return true;
            }
            else NotFound();
            return false;
        }
        /// <summary>
        /// Deletes a specific hero.
        /// </summary>
        /// <param name="hero id"></param>
        /// <response code="200">Returns all heroes in base</response>
        /// <response code="401">User isn't login</response>
        /// <response code="404">Id is invalid</response>
        [HttpDelete("{id}"), Authorize(Roles = "admin")]
        [EnableCors("developerska")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public bool Delete(int id)
        {
            //heroes.Remove(heroes.Single(h => h.Id == id));
            //order_id();
            int idx = id_valid(id);
            if (idx >= 0)
            {
                heroes.RemoveAt(idx);
                delete_Hero(id);
                order_id();
                save_all_changes();
                return true;
            }
            else
                NotFound();
            return false;
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
        void save_all_changes()
        {
            string cmd_reset = "delete from heroes;";
            SqliteCommand sql_cmd = new SqliteCommand(cmd_reset, connection);
            connection.Open();
            sql_cmd.ExecuteNonQuery();
            foreach (var her in heroes)
            {
                string cmd_ins = "insert into heroes(id,nick,avatar,class) values(" + her.Id.ToString() + ",'" + her.Nick + "','" + her.Img + "','" + her.Prof + "');";
                SqliteCommand sql_cmd_in = new SqliteCommand(cmd_ins, connection);
                sql_cmd_in.ExecuteNonQuery();
            }
            connection.Close();
        }
        void insert_Hero(Hero her)
        {
            connection.Open();
            string cmd_ins = "insert into heroes(id,nick,avatar,class) values(" + her.Id.ToString() + ",'" + her.Nick + "','" + her.Img + "','" + her.Prof + "');";
            SqliteCommand sql_cmd_in = new SqliteCommand(cmd_ins, connection);
            sql_cmd_in.ExecuteNonQuery();
            connection.Close();
        }
        void delete_Hero(int id)
        {
            connection.Open();
            string cmd_del = "delete from heroes where id=" + id + ";";
            SqliteCommand sql_cmd_del = new SqliteCommand(cmd_del, connection);
            sql_cmd_del.ExecuteNonQuery();
            connection.Close();
        }
    }
}