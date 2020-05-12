using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models
{
    public class Hero
    {
        public Hero(int id, string nick, string img, string prof)
        {
            Id = id;
            Nick = nick;
            Img = img;
            Prof = prof;
        }
        public int Id { get; set; }
        public string Nick { get; set; }
        public string Img { get; set; }
        public string Prof { get; set; }
        public override string ToString()
        {
            return "" + Id + " " + Nick + " " + Img + " " + Prof;
        }
    }
}
