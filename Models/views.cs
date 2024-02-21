using System;

namespace openmarket.Models
{
    public class views
    {
        public int id { get; set; }
        public int account { get; set; }
        public string type { get; set; }
        public string ip { get; set; }
        public DateTime date { get; set; }
        public int advert { get; set; }
        public AppDbContext db;
        public views(AppDbContext _db)
        {
            db = _db;
        }
        public void Register(int advert, string page)
        {
            var register = new views(db)
            {
                advert = advert,
                date = DateTime.Now,
                type = page
            };
            db.views.Add(register);
            db.SaveChanges();
        }
    }
}