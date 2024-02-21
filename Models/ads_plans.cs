namespace openmarket.Models
{
    public class ads_plans
    {
        public int id { get; set; }
        public string name { get; set; }
        public int days_top { get; set; }
        public int days_featured { get; set; }
        public int days_principal { get; set; }
        public string category { get; set; }
        public int price { get; set; }
        public string filename { get; set; }
    }
}