using System;
namespace openmarket.Models
{
    public class conversations
    {
        public int id { get; set; }
        public int sender { get; set; }
        public int receiver { get; set; }
        public string message { get; set; }
        public string image { get; set; }
        public DateTime date { get; set; }
        public string status { get; set; }
        public int advert { get; set; }
    }
}