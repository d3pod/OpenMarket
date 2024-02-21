using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Hosting;
using System.Xml;
using System.Net.Mail;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class dashboardModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment appEnvironment;
        public dashboardModel(AppDbContext _db, IWebHostEnvironment _appEnvironment)
        {
            db = _db;
            appEnvironment = _appEnvironment;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        StreamReader reader;
        public IList<Charts> views;
        public IList<Charts> adverts;
        public IList<Charts> users;
        public IList<month> month_list;
        [BindProperty] public int Permission { get; set; }
        [BindProperty] public int Month { get; set; }
        [BindProperty] public int adverts_pending { get; set; }
        [BindProperty] public int adverts_update { get; set; }
        public IList<_adverts> Views_today;
        public IList<_adverts> Views_week;
        public IActionResult OnGet()
        {
            //Sessão do utilizador
            valueFormat formatter = new valueFormat();
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("adminID")))
            {
                SessionUser = Convert.ToInt32(HttpContext.Session.GetString("adminID"));
            }
            else
            {
                string adminCookie = Request.Cookies["fzadminid"];
                if (adminCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(adminCookie));
                    if (db.users.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("adminID", Convert.ToString(SessionUser));
                    }
                    else
                    {
                        return Redirect("~/admin/login");
                    }
                }
                else
                {
                    return Redirect("~/admin/login");
                }
            }
            Permission = db.users.Where(x => x.id == SessionUser).Select(x => x.rule).First();
            Month = DateTime.Now.Month;
            int month = 0;
            if (!string.IsNullOrEmpty(Request.Query["month"]))
            {
                month = Convert.ToInt32(Request.Query["month"]);
                Month = month;
            }
            else
            {
                month = Month;
            }
            month_list = db.month.OrderBy(x => x.id).ToList();
            adverts_pending = db.adverts.Where(x => x.status == 0).Count();
            adverts_update = db.adverts.Where(x => x.status == 10).Count();
            //Views
            IQueryable<Charts> filter_views = (from x in db.views.Where(x => x.date.Year == DateTime.Now.Year && x.date.Month == month)
                                               where x.type == null
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
            //Adverts
            IQueryable<Charts> filter_adverts = (from x in db.adverts.Where(x => x.date.Year == DateTime.Now.Year && x.date.Month == month)
                                                 group x by new
                                                 {
                                                     x.date.DayOfYear
                                                 } into g
                                                 select new Charts
                                                 {
                                                     valueX = g.Key.DayOfYear,
                                                     valueY = g.Count()
                                                 });
            adverts = filter_adverts.OrderByDescending(x => x.valueX).ToList();
            //Users
            IQueryable<Charts> filter_users = (from x in db.accounts.Where(x => x.register.Year == DateTime.Now.Year && x.register.Month == month)
                                               group x by new
                                               {
                                                   x.register.DayOfYear
                                               } into g
                                               select new Charts
                                               {
                                                   valueX = g.Key.DayOfYear,
                                                   valueY = g.Count()
                                               });
            users = filter_users.OrderByDescending(x => x.valueX).ToList();
            //Lista de anúncios mais vistos hoje
            IQueryable<_adverts> viewsToday = (from x in db.views
                                               join q in db.adverts on x.advert equals q.id
                                               where x.date.Date == DateTime.Now.Date
                                               group x by new
                                               {
                                                   q.title
                                               } into g
                                               select new _adverts
                                               {
                                                   title = g.Key.title,
                                                   views = g.Count()
                                               });
            Views_today = viewsToday.OrderByDescending(x => x.views).Take(20).ToList();

            //Lista de anúncios mais vistos esta semana
            IQueryable<_adverts> viewsWeek = (from x in db.views
                                              join q in db.adverts on x.advert equals q.id
                                              where x.date.Date > DateTime.Now.Date.AddDays(-7)
                                              group x by new
                                              {
                                                  q.title
                                              } into g
                                              select new _adverts
                                              {
                                                  title = g.Key.title,
                                                  views = g.Count()
                                              });
            Views_week = viewsWeek.OrderByDescending(x => x.views).Take(20).ToList();
            return null;
        }
        public IActionResult OnPostSitemap()
        {
            valueFormat formatter = new valueFormat();
            List<string> pages = new List<string>()
            {
                "termos-condicoes",
                "privacidade-cookies",
                "entrar",
                "dicas-seguranca",
                "contacte-nos",
                "inserir-anuncios",
                "anunciar-vender"
            };
            var groups = db.groups.Select(x => x.name);
            var categories = db.categories.Select(x => x.name);
            var countries = db.countries.Select(x => x.name);
            var cities = db.cities.Select(x => x.name);
            var municipalities = db.municipalities.Select(x => x.name);
            var accounts = db.accounts.ToList();

            var adverts = from x in db.adverts
                          where x.status == 1
                          select new adverts
                          {
                              id = x.id,
                              title = x.title
                          };

            StringBuilder sb = new StringBuilder();
            string mDate = DateTime.Now.Date.ToString("yyyy-MM-dd");
            sb.Append("<?xml version='1.0' encoding='UTF-8'?>");
            sb.Append("<urlset xmlns='http://www.sitemaps.org/schemas/sitemap/0.9'>");
            sb.Append("<url><loc>" + "https://www.openmarket.com/" + "</loc><lastmod>" + mDate + "</lastmod><priority> 1.0</priority><changefreq>always</changefreq></url>");

            foreach (var page in pages)
            {
                string url = formatter.urlFormat(page);
                sb.Append("<url><loc>" + "https://www.openmarket.com/" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.3</priority><changefreq>monthly</changefreq></url>");
            }
            foreach (var group in groups)
            {
                string url = formatter.urlFormat(group);
                sb.Append("<url><loc>" + "https://www.openmarket.com/anuncios?categoria=" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.7</priority><changefreq>always</changefreq></url>");
            }

            foreach (var category in categories)
            {
                string url = formatter.urlFormat(category);
                sb.Append("<url><loc>" + "https://www.openmarket.com/anuncios?sub-categoria=" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.7</priority><changefreq>always</changefreq></url>");
            }

            foreach (var country in countries)
            {
                string url = formatter.urlFormat(country);
                sb.Append("<url><loc>" + "https://www.openmarket.com/anuncios?pais=" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.7</priority><changefreq>always</changefreq></url>");
            }

            foreach (var city in cities)
            {
                string url = formatter.urlFormat(city);
                sb.Append("<url><loc>" + "https://www.openmarket.com/anuncios?provincia=" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.7</priority><changefreq>always</changefreq></url>");
            }

            foreach (var municipality in municipalities)
            {
                string url = formatter.urlFormat(municipality);
                sb.Append("<url><loc>" + "https://www.openmarket.com/anuncios?municipio=" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>0.7</priority><changefreq>always</changefreq></url>");
            }
            foreach (var users in accounts)
            {
                string url = formatter.urlFormat(users.username) + "&amp;id=" + users.id;
                sb.Append("<url><loc>" + "https://www.openmarket.com/perfil?" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>1.0</priority><changefreq>always</changefreq></url>");
            }

            foreach (var item in adverts)
            {
                string url = formatter.urlFormat(item.title) + "&amp;id=" + @item.id;
                sb.Append("<url><loc>https://www.openmarket.com/anuncio?" + url + "</loc><lastmod>" + mDate + "</lastmod><priority>1.0</priority><changefreq>always</changefreq></url>");
            }
            sb.Append("</urlset>");

            XmlDocument xml = new XmlDocument();
            xml.LoadXml(sb.ToString());
            xml.Save(appEnvironment.WebRootPath + "/sitemap.xml");
            return RedirectToPage("dashboard");
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("adminID");
            if (Request.Cookies["fzadminid"] != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Append("fzadminid", "0", cookieOptions);
            }
            return Redirect("~/admin/login");
        }
        public IActionResult OnPostAnuncios()
        {
            var list = db.adverts.Where(x => x.expiration.Date <= DateTime.Now.Date && x.status == 1).ToList();
            foreach (var item in list)
            {
                item.status = 2;
                db.Update(item);
                db.SaveChanges();
                sendEmail(item.title, item.account);
            }
            return RedirectToPage("dashboard");
        }
        public void sendEmail(string title, int userID)
        {
            var user = db.accounts.First(x => x.id == userID);
            if (!string.IsNullOrEmpty(user.email))
            {
                string body;
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                smtp.EnableSsl = true;
                MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                MailAddress to = new MailAddress(user.email);
                MailMessage mailMessage = new MailMessage(from, to);
                mailMessage.Subject = "O teu anúncio no OpenMarket expirou!";
                mailMessage.IsBodyHtml = true;
                reader = new StreamReader("wwwroot/templates/emails/advert-status.htm");
                body = reader.ReadToEnd();
                body = body.Replace("{username}", user.username);
                body = body.Replace("{title}", title);
                mailMessage.Body = body;
                smtp.Send(mailMessage);
            }
            string message = "O teu anúncio, " + title.Trim() + ", expirou. Podes activar novamente este anúncio gratuitamente, acedendo à página da tua conta, clicas no anúncio e de seguida em Activar. Os anúncios expiram ao fim de 180 dias por questões de segurança e filtragem. Obrigado por usares o OpenMarket.";
            var newPost = new conversations
            {
                sender = 1,
                receiver = Convert.ToInt32(userID),
                message = message,
                date = DateTime.Now,
                status = "Por ler"
            };
            db.conversations.Add(newPost);
            db.SaveChanges();
        }
        public IActionResult OnPostFacebook()
        {
            IQueryable<_adverts> filter = from x in db.adverts
                                              /* join y in db.images on x.id equals y.product into image
                                              from y in image.DefaultIfEmpty() */
                                          where x.status == 1
                                          select new _adverts
                                          {
                                              id = x.id,
                                              title = x.title/* ,
                                              image_filename = (from r in db.images where r.product == x.id select r).First().filename */
                                          };
            IList<_adverts> List = filter.ToList();
            List<int> listID = new List<int>();
            foreach (var item in List)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (List.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var advertSelected = List.FirstOrDefault(x => x.id == listID[i]);
                    List.Remove(advertSelected);
                }
            }
            Random rdm = new Random();
            int value = rdm.Next(List.Count());
            var advert = List.Skip(value).Take(1).First();
            var url = "https://openmarket.com/anuncio?";
            var advert_id = "&id=" + advert.id;
            valueFormat formatter = new valueFormat();
            url = url + formatter.urlFormat(advert.title) + advert_id;

            //Facebook
            string token = "Your_Token";
            string pageID = "Page_Id";
            long scheduled_publish_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600;
            Facebook facebook = new Facebook(token, pageID);
            string result = facebook.PublishToFacebook(">>> " + advert.title + " <<<\nSabe mais sobre este e outros anúncios em www.openmarket.com \nNo OpenMarket publicar anúncios é grátis!", url, "false", scheduled_publish_time.ToString());
            //Instagram

            //Twitter

            return RedirectToPage("dashboard");
        }
    }
}
