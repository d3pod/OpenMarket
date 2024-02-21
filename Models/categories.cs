namespace openmarket.Models
{
    public class categories
    {
        public int id { get; set; }
        public string name { get; set; }
        public int group_id { get; set; }
        public byte[] picture { get; set; }
        public int indexed { get; set; }
    }
}