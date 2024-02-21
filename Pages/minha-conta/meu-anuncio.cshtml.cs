using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class meu_anuncioModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Cookies { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public meu_anuncioModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty] public string _Msg { get; set; }
        [BindProperty] public int advert_id { get; set; } = 0;
        [BindProperty] public string filename { get; set; }
        [BindProperty] public string Title { get; set; }
        [BindProperty] public string url { get; set; }
        [BindProperty(SupportsGet = true)] public string GoogleMaps { get; set; }
        public string KeyMaps = "Maps_Key";
        public IList<images> images;
        public IList<_adverts> advert;
        public IList<Charts> views;
        public IList<Charts> viewsIndex;
        [BindProperty] public int totalViews { get; set; }
        public IList<alerts> alerts_list;
        public IActionResult OnGet(string Msg)
        {
            _Msg = Msg;
            if (Request.Cookies["Accepted"] != null)
            {
                Cookies = 1;
            }
            else
            {
                Cookies = 0;
            }

            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("userID")))
            {
                SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
                if (db.accounts.Where(x => x.id == SessionUser).Select(x => x.status).Single() == 11)
                {
                    HttpContext.Session.Remove("userID");
                    if (Request.Cookies["fzid"] != null)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        Response.Cookies.Append("fzid", "0", cookieOptions);
                    }
                    return Redirect("~/entrar");
                }
            }
            else
            {
                valueFormat formatter = new valueFormat();
                string userCookie = Request.Cookies["fzid"];
                if (userCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(userCookie));
                    if (db.accounts.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("userID", Convert.ToString(SessionUser));
                    }
                    else if (!string.IsNullOrEmpty(Request.Query["backUrl"]))
                    {
                        return Redirect("~/entrar?backUrl=" + Request.Query["backUrl"]);
                    }
                    else
                    {
                        return Redirect("~/entrar?");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request.Query["backUrl"]))
                    {
                        return Redirect("~/entrar?backUrl=" + Request.Query["backUrl"]);
                    }
                    else
                    {
                        return Redirect("~/entrar?");
                    }
                }

            }
            if (Request.Cookies["fz_ma"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "meu-anuncio" && x.status == 1).ToList();
            }
            advert_id = Convert.ToInt32(Request.Query["id"]);
            if (advert_id > 0)
            {
                if (db.adverts.Where(x => x.id == advert_id).Select(x => x.account).Single() != SessionUser)
                {
                    return Redirect("~/minha-conta");
                }
                else
                {
                    IQueryable<_adverts> filter = from x in db.adverts.Where(x => x.id == advert_id)
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  join u in db.accounts on x.account equals u.id
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      negotiable = x.negotiable,
                                                      date = x.date,
                                                      municipality = x.municipality,
                                                      locality = x.locality,
                                                      city = x.city,
                                                      country = w.name,
                                                      description = x.description,
                                                      date_month = x.date.ToString("MMMM"),
                                                      image_filename = y.filename,
                                                      groupName = x.groupName,
                                                      category = x.category,
                                                      account = x.account,
                                                      username = u.username,
                                                      user_date_month = u.register.ToString("MMMM"),
                                                      user_date = u.register,
                                                      user_picture = u.picture,
                                                      contact = x.contact,
                                                      type = x.type,
                                                      condition = x.condition,
                                                      vendor = x.vendor,
                                                      email_visible = x.email_visible,
                                                      status = x.status
                                                  };
                    filename = db.images.Where(x => x.product == advert_id).Select(x => x.filename).FirstOrDefault();
                    advert = filter.Take(1).ToList();
                    string municipality = advert.Select(x => x.municipality).FirstOrDefault().ToString();
                    municipality = municipality.Replace(" ", "+");
                    string locality = advert.Select(x => x.locality).FirstOrDefault().ToString();
                    locality = locality.Replace(" ", "+");
                    string city = advert.Select(x => x.city).FirstOrDefault().ToString();
                    city = city.Replace(" ", "+");
                    GoogleMaps = "https://www.google.com/maps/embed/v1/place?key=" + KeyMaps + "&q=" + locality + "," + municipality + "+" + city + "";

                    Title = advert.Select(x => x.title).First();
                    var advert_title = Title.Replace(" ", "_");
                    advert_title = advert_title.Replace("/", "_");
                    url = "anuncio?" + advert_title + "&id=" + advert_id;
                    images = db.images.Where(x => x.product == advert_id).ToList();
                    totalViews = db.views.Where(x => x.advert == advert_id && x.type == null).Count();

                    IQueryable<Charts> filter_views = (from x in db.views
                                                       where x.date.Year == DateTime.Now.Year && x.date > DateTime.Now.AddDays(-60) && x.advert == advert.Select(x => x.id).FirstOrDefault() && x.type == null
                                                       group x by new
                                                       {
                                                           x.date.DayOfYear
                                                       } into g
                                                       select new Charts
                                                       {
                                                           valueX = g.Key.DayOfYear,
                                                           valueY = g.Count()
                                                       });
                    views = filter_views.OrderByDescending(x => x.valueX).ToList();
                    return null;
                }
            }
            else
            {
                return Redirect("~/minha-conta");
            }
        }
        public IActionResult OnPostEliminar(int advert)
        {
            db.Remove(db.adverts.First(x => x.id == advert));
            db.SaveChanges();
            while (db.images.Where(x => x.product == advert).Count() > 0)
            {
                db.Remove(db.images.First(x => x.product == advert));
                db.SaveChanges();
            }
            return Redirect("~/minha-conta");
        }
        public IActionResult OnPostActivar(int advert)
        {
            var updateStatus = db.adverts.First(x => x.id == advert);
            if (updateStatus.expiration < DateTime.Now)
            {
                updateStatus.status = 1;
                updateStatus.expiration = DateTime.Now.AddDays(28);
                db.Update(updateStatus);
                db.SaveChanges();
            }
            else
            {
                updateStatus.status = 1;
                db.Update(updateStatus);
                db.SaveChanges();
            }
            return Redirect("~/minha-conta");
        }
        public IActionResult OnPostDesactivar(int advert)
        {
            var updateStatus = db.adverts.First(x => x.id == advert);
            updateStatus.status = 2;
            db.Update(updateStatus);
            db.SaveChanges();
            return Redirect("~/minha-conta");
        }
        public IActionResult OnPostVendido(int advert)
        {
            var updateStatus = db.adverts.First(x => x.id == advert);
            updateStatus.status = 3;
            db.Update(updateStatus);
            db.SaveChanges();
            return Redirect("~/minha-conta?Msg=Vendido");
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("userID");
            if (Request.Cookies["fzid"] != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Append("fzid", "0", cookieOptions);
            }
            return Redirect("~/index");
        }
    }
}
