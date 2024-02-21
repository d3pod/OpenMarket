using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
namespace openmarket.Models
{
    public class ads
    {
        public AppDbContext db;
        public ads(AppDbContext _db)
        {
            db = _db;
        }
        public IList<ads> adsList(string page, string format)
        {
            
            return null;
        }
    }
}