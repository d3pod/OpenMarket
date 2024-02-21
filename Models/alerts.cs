using System;
namespace openmarket.Models
{
    public class alerts
    {
        public int id { get; set; }
        public string title { get; set; }
        public string text { get; set; }
        public string button_text { get; set; }
        public string button_url { get; set; }
        public DateTime validation { get; set; }
        public int status { get; set; }
        public int cookie_time { get; set; }
        public string page { get; set; }
    }
}