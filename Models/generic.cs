using System;
namespace openmarket.Models
{
    public class generic
    {
        public int id { get; set; }
        public string name { get; set; }
        public string father_element { get; set; }
        public byte[] picture { get; set; }
        public int value { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public int origin { get; set; }
        public DateTime date { get; set; }
        public int userID { get; set; }
        public string username { get; set; }
    }
}