using System;
using System.Linq;
using openmarket.Models;
namespace openmarket.Models
{
    public class notificationIcon
    {
        public AppDbContext db;
        public notificationIcon(AppDbContext _db)
        {
            db = _db;
        }
        public int counter(int session)
        {
            int toRead = 0;
            return toRead = db.conversations.Where(x => x.receiver == session && x.status == "Por ler").Select(x => x.status).Count(); 
        }
    }
}