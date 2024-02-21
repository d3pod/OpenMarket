using System;
namespace openmarket.Models
{
    public class _chat
    {
        public int id { get; set; }
        public int sender { get; set; }
        public string sender_username { get; set; }
        public int receiver { get; set; }
        public string receiver_username { get; set; }
        public string message { get; set; }
        public string image { get; set; }
        public DateTime date { get; set; }
        public string date_month { get; set; }
        public string status { get; set; }
        public int account { get; set; }
    }
}