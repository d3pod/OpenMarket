using System;
namespace openmarket.Models
{
    public class login_logs
    {
        public int id { get; set; }
        public int account { get; set; }
        public string ip { get; set; }
        public DateTime date { get; set; }
    }
}