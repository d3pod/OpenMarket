using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace openmarket.Models
{
    public class _adverts_return
    {
        public IList<_adverts> adverts;
        public AppDbContext db;
        public _adverts_return(AppDbContext _db)
        {
            db = _db;
        }

        public IList<_adverts> principal_adverts(string type, string location)
        {
            IQueryable<_adverts> filter = from x in db.adverts
                                          join w in db.categories on x.category equals w.name
                                          join g in db.ads_movs on x.id equals g.advert
                                          join t in db.ads_plans on g.plan equals t.id
                                          join y in db.images on x.id equals y.product into image
                                          from y in image.DefaultIfEmpty()
                                          where x.status == 1
                                          select new _adverts
                                          {
                                              id = x.id,
                                              title = x.title,
                                              price = x.price,
                                              date = x.date,
                                              municipality = x.municipality,
                                              city = x.city,
                                              date_month = x.date.ToString("MMM"),
                                              image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                              type = x.type,
                                              orc = x.orc,
                                              days_principal = t.days_principal,
                                              plan_activation = g.date,
                                              sponsored = "Patrocinado",
                                              expiration = g.expiration
                                              
                                          };
            IQueryable<_adverts> filter2 = from x in db.adverts
                                           join w in db.categories on x.category equals w.name
                                           join y in db.images on x.id equals y.product into image
                                           from y in image.DefaultIfEmpty()
                                           where x.status == 1
                                           select new _adverts
                                           {
                                               id = x.id,
                                               title = x.title,
                                               price = x.price,
                                               date = x.date,
                                               municipality = x.municipality,
                                               city = x.city,
                                               date_month = x.date.ToString("MMM"),
                                               image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                               type = x.type,
                                               orc = x.orc
                                           };

            IList<_adverts> sponsoredList = filter.Where(x => x.days_principal > 0 && x.expiration.Date > DateTime.Now.Date).ToList();
            IList<_adverts> normalList = filter2.OrderByDescending(x => x.id).Take(360).ToList();

            List<int> listID = new List<int>();
            foreach (var item in sponsoredList)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            List<int> listID2 = new List<int>();
            foreach (var item2 in normalList)
            {
                if (!listID.Contains(item2.id) && !listID2.Contains(item2.id))
                {
                    listID2.Add(item2.id);
                }
            }
            for (int i = 0; i < listID2.Count(); i++)
            {
                while (normalList.Where(x => x.id == listID2[i]).Count() > 1)
                {
                    var advertSelected = normalList.FirstOrDefault(x => x.id == listID2[i]);
                    normalList.Remove(advertSelected);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (sponsoredList.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var advertSelected = sponsoredList.FirstOrDefault(x => x.id == listID[i]);
                    sponsoredList.Remove(advertSelected);
                }
                while (normalList.Where(x => x.id == listID[i]).Count() > 0)
                {
                    var advertSelected = normalList.FirstOrDefault(x => x.id == listID[i]);
                    normalList.Remove(advertSelected);
                }
            }
            if (!string.IsNullOrEmpty(type) && !string.IsNullOrEmpty(location))
            {
                switch (type)
                {
                    case "pais":
                        sponsoredList = sponsoredList.Where(x => x.country == location).ToList();
                        normalList = normalList.Where(x => x.country == location).ToList();
                        break;

                    case "provincia":
                        sponsoredList = sponsoredList.Where(x => x.city == location).ToList();
                        normalList = normalList.Where(x => x.city == location).ToList();
                        break;

                    case "municipio":
                        sponsoredList = sponsoredList.Where(x => x.municipality == location).ToList();
                        normalList = normalList.Where(x => x.municipality == location).ToList();
                        break;
                }
            }
            Random rdm = new Random();
            adverts = new List<_adverts>();

            if (sponsoredList != null || sponsoredList.Count() < 36)
            {
                while (sponsoredList.Count() > 0)
                {
                    int value = rdm.Next(sponsoredList.Count());
                    var advert = sponsoredList.Skip(value).Take(1).First();
                    adverts.Add(advert);
                    sponsoredList.Remove(advert);
                }
                while (adverts.Count() < 36)
                {
                    var advert = normalList.OrderByDescending(x => x.id).Take(1).First();
                    adverts.Add(advert);
                    normalList.Remove(advert);
                }
            }
            else if(sponsoredList == null)
            {
                while (adverts.Count() < 36 && normalList.Count() > 0)
                {
                    var advert = normalList.OrderByDescending(x => x.id).Take(1).First();
                    adverts.Add(advert);
                    normalList.Remove(advert);
                }
            }
            else
            {
                while (adverts.Count() < 36)
                {
                    int value = rdm.Next(sponsoredList.Count());
                    var advert = sponsoredList.Skip(value).Take(1).First();
                    adverts.Add(advert);
                    sponsoredList.Remove(advert);
                }
            }

            return adverts;
        }
    }
}