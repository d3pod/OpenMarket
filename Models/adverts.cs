using System;
namespace openmarket.Models
{
    public class adverts
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public double price_min { get; set; }
        public double price_max { get; set; }
        public string groupName { get; set; }
        public string category { get; set; }
        public string city { get; set; }
        public string municipality { get; set; }
        public string locality { get; set; }
        public string contact { get; set; }
        public int status { get; set; }
        public int condition { get; set; }
        public int negotiable { get; set; }
        public DateTime date { get; set; }
        public DateTime expiration { get; set; }
        public int email_visible { get; set; }
        public int account { get; set; }
        public string vendor { get; set; }
        public string type { get; set; }
        public string orc { get; set; }
        public string period { get; set; }
        public string contract { get; set; }
    }
}