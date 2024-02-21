using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
namespace openmarket.Models
{
    public class ads_movs
    {
        public int id { get; set; }
        public int plan { get; set; }
        public DateTime date { get; set; }
        public DateTime expiration { get; set; }
        public int advert { get; set; }
    }
}