using System;
namespace openmarket.Models
{
    public class _adverts
    {
        public int id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public double price { get; set; }
        public double price_min { get; set; }
        public double price_max { get; set; }
        public string category { get; set; }
        public string city { get; set; }
        public string municipality { get; set; }
        public string locality { get; set; }
        public string country { get; set; }
        public string contact { get; set; }
        public int status { get; set; }
        public int condition { get; set; }
        public int negotiable { get; set; }
        public string groupName { get; set; }
        public DateTime date { get; set; }
        public string date_text { get; set; }
        public string date_month { get; set; }
        public DateTime expiration { get; set; }
        public int email_visible { get; set; }
        public int account { get; set; }
        public string username { get; set; }
        public string image_filename { get; set; }
        public byte[] user_picture { get; set; }
        public DateTime user_date { get; set; }
        public string user_date_month { get; set; }
        public int favorite { get; set; }
        public string type { get; set; }
        public string vendor { get; set; }
        public string orc { get; set; }
        public string period { get; set; }
        public string contract { get; set; }
        public int indexed { get; set; }
        public int views { get; set; }
        public DateTime plan_activation { get; set; }
        public int days_principal { get; set; }
        public string sponsored { get; set; }
    }
}