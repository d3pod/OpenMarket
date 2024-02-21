using System;
namespace openmarket.Models
{
    public class wallet_movs
    {
        public int id { get; set; }
        public int user { get; set; }
        public int value { get; set; }
        public string description { get; set; }
        public int origin { get; set; }
        public DateTime date { get; set; }
    }
}