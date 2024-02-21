namespace openmarket.Models
{
    public class images
    {
        public int id { get; set; }
        public byte[] image { get; set; }
        public int product { get; set; }
        public int status { get; set; }
        public string filename { get; set; }
    }
}