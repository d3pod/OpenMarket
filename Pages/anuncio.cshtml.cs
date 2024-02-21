using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System.Text;
using System.Net.Mail;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class anuncioModel : PageModel
    {
        public AppDbContext db { get; set; }
        public anuncioModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public int id { get; set; }
        [BindProperty] public string filename { get; set; }
        [BindProperty] public string Title { get; set; }
        [BindProperty] public string Description { get; set; }
        [BindProperty] public string url { get; set; }
        public IList<_adverts> advert;
        public IList<_adverts> user_adverts;
        public IList<_adverts> adverts_category;
        public IList<images> images;
        public favorites favorite { get; set; }
        [BindProperty] public int views { get; set; }
        [BindProperty] public string last_login { get; set; }
        [BindProperty] public string From { get; set; }
        [BindProperty] public string Name { get; set; }
        [BindProperty] public string Subject { get; set; }
        [BindProperty] public bool favorite_check { get; set; } = false;
        [BindProperty(SupportsGet = true)] public string GoogleMaps { get; set; }
        public string KeyMaps = "Maps_Key";
        StreamReader reader;
        string body = string.Empty;
        [BindProperty(SupportsGet = true)] public string Body { get; set; }
        [BindProperty(SupportsGet = true)] public string Msg { get; set; }
        public IList<alerts> alerts_list;
        public IActionResult OnGet()
        {
            if (Request.Cookies["accepted"] != null)
            {
                Cookies = 1;
            }
            else
            {
                Cookies = 0;
            }
            //Sessão do utilizador
            valueFormat formatter = new valueFormat();
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
                string userCookie = Request.Cookies["fzid"];
                if (userCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(userCookie));
                    if (db.accounts.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("userID", Convert.ToString(SessionUser));
                    }
                }
            }
            if (!string.IsNullOrEmpty(Request.Query["Msg"]))
            {
                Msg = Request.Query["Msg"];
            }

            int advert_id = Convert.ToInt32(Request.Query["id"]);
            id = advert_id;
            if (advert_id > 0)
            {
                if (db.adverts.Where(x => x.id == advert_id).Select(x => x.account).SingleOrDefault() == SessionUser && SessionUser > 0)
                {
                    return Redirect("~/minha-conta/meu-anuncio?id=" + advert_id);
                }
                string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
                DateTime today = DateTime.Now.Date;
                int registerToday = 0;
                if (db.views.Where(x => x.advert == advert_id && x.date.Date == today && x.ip == ip).Select(x => x.id).FirstOrDefault() > 0)
                {
                    registerToday = db.views.Where(x => x.advert == advert_id && x.date.Date == today && x.type == null).Select(x => x.id).First();
                }
                if (registerToday == 0)
                {
                    if (SessionUser != 0)
                    {
                        var register = new views(db)
                        {
                            advert = advert_id,
                            account = SessionUser,
                            date = DateTime.Now,
                            ip = ip
                        };
                        db.views.Add(register);
                        db.SaveChanges();
                    }
                    else
                    {
                        var register = new views(db)
                        {
                            advert = advert_id,
                            date = DateTime.Now,
                            ip = ip
                        };
                        db.views.Add(register);
                        db.SaveChanges();
                    }
                }
                if (SessionUser != 0)
                {
                    var favorite_id = db.favorites.Where(x => x.account == SessionUser && x.advert == advert_id).Select(x => x.id).FirstOrDefault();
                    if (favorite_id > 0)
                    {
                        favorite_check = true;
                    }
                }
                IQueryable<_adverts> filter = from x in db.adverts.Where(x => x.id == advert_id)
                                              join y in db.images on x.id equals y.product into image
                                              from y in image.DefaultIfEmpty()
                                              join q in db.cities on x.city equals q.name
                                              join w in db.countries on q.country equals w.id
                                              join u in db.accounts on x.account equals u.id
                                              where x.status == 1
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
                                                  email_visible = x.email_visible
                                              };
                advert = filter.Take(1).ToList();
                string municipality = advert.Select(x => x.municipality).FirstOrDefault().ToString();
                municipality = municipality.Replace(" ", "+");
                string locality = advert.Select(x => x.locality).FirstOrDefault().ToString();
                locality = locality.Replace(" ", "+");
                string city = advert.Select(x => x.city).FirstOrDefault().ToString();
                city = city.Replace(" ", "+");
                GoogleMaps = "https://www.google.com/maps/embed/v1/place?key=" + KeyMaps + "&q=" + locality + "," + municipality + "+" + city + "";

                Title = advert.Select(x => x.title).First();
                Description = advert.Select(x => x.description).First();
                filename = db.images.Where(x => x.product == advert_id).Select(x => x.filename).FirstOrDefault();
                /* Formata o titulo para a URL */
                url = "anuncio?" + formatter.urlFormat(Title) + "&id=" + id;

                var user = db.accounts.Where(x => x.id == advert.Select(x => x.account).First()).First();
                var date = db.login_logs.Where(x => x.account == user.id).OrderByDescending(x => x.id).First();
                if (date.date.Date == DateTime.Now.Date)
                {
                    last_login = "Esteve online hoje às " + date.date.ToString("HH:mm");
                }
                else if (date.date.Date == DateTime.Now.Date.AddDays(-1))
                {
                    last_login = "Esteve online ontem às " + date.date.ToString("HH:mm");
                }
                else
                {
                    var month = char.ToUpper(date.date.ToString("MMMM")[0]) + date.date.ToString("MMMM").Substring(1);
                    last_login = "Esteve online a " + date.date.Day + " de " + month + " de " + date.date.Year;
                }
                images = db.images.Where(x => x.product == advert_id).ToList();
                views = db.views.Where(x => x.advert == advert_id && x.type != "phone").Count();
                Adverts_Category(advert.Select(x => x.category).FirstOrDefault(), advert_id);
                User_Adverts(advert.Select(x => x.account).FirstOrDefault(), advert_id);
                if (Request.Cookies["fz_adv"] == null)
                {
                    alerts_list = db.alerts.Where(x => x.page == "anuncio" && x.status == 1).ToList();
                }
                return null;
            }
            else
            {
                return Redirect("~/anuncios");
            }
        }
        public IActionResult OnGetContact(int advert, string type)
        {
            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            DateTime today = DateTime.Now;
            int registerToday = 0;
            if (db.views.Where(x => x.advert == advert && x.date.Date == today.Date && x.date.TimeOfDay.Hours == today.TimeOfDay.Hours && ((today.TimeOfDay.Minutes - x.date.TimeOfDay.Minutes) < 15) && x.ip == ip && x.type == string.Empty).Select(x => x.id).FirstOrDefault() > 0)
            {
                registerToday = 1;
            }
            if (registerToday == 0)
            {
                if (SessionUser != 0)
                {
                    var register = new views(db)
                    {
                        advert = advert,
                        account = SessionUser,
                        date = DateTime.Now,
                        ip = ip,
                        type = type
                    };
                    db.views.Add(register);
                    db.SaveChanges();
                }
                else
                {
                    var register = new views(db)
                    {
                        advert = advert,
                        date = DateTime.Now,
                        ip = ip,
                        type = type
                    };
                    db.views.Add(register);
                    db.SaveChanges();
                }
            }
            var contact = db.adverts.Where(x => x.id == advert).Select(x => x.contact).First();
            return new JsonResult(contact);
        }
        public IActionResult OnGetFavorite(int advert, int user)
        {
            var newFavorite = new favorites
            {
                account = user,
                advert = advert,
                date = DateTime.Now
            };
            db.favorites.Add(newFavorite);
            db.SaveChanges();
            return new JsonResult("");
        }
        public IActionResult OnGetRemove(int advert, int user)
        {
            db.Remove(db.favorites.First(x => x.account == user && x.advert == advert));
            db.SaveChanges();
            return new JsonResult("");
        }
        public IActionResult OnPostChat(int advert_id, string chat_body)
        {
            int account = db.adverts.Where(x => x.id == advert_id).Select(x => x.account).First();
            string title = db.adverts.Where(x => x.id == advert_id).Select(x => x.title).First();
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            title = title.Replace(" ", "_");
            title = title.Replace("/", "_");
            var new_post = new conversations
            {
                message = chat_body,
                sender = SessionUser,
                receiver = account,
                date = DateTime.Now,
                status = "Por ler",
                advert = advert_id
            };
            db.conversations.Add(new_post);
            var post_advert = new conversations
            {
                message = "Anúncio: " + db.adverts.Where(x => x.id == advert_id).Select(x => x.title).First() + " / ID:" + advert_id,
                sender = SessionUser,
                receiver = account,
                date = DateTime.Now,
                status = "Por ler",
                advert = advert_id
            };
            db.conversations.Add(post_advert);
            db.SaveChanges();
            return Redirect("/anuncio?" + WebUtility.UrlEncode(title) + "&id=" + advert_id + "&Msg=mensagem-enviada");
        }
        public IActionResult OnPostEmail(int advert_id, string email_body)
        {
            int account = db.adverts.Where(x => x.id == advert_id).Select(x => x.account).First();
            string title = db.adverts.Where(x => x.id == advert_id).Select(x => x.title).First();
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            string user_email = db.accounts.Where(x => x.id == SessionUser).Select(x => x.email).First();
            string seller_email = db.accounts.Where(x => x.id == account).Select(x => x.email).First();
            string username = db.accounts.Where(x => x.id == SessionUser).Select(x => x.username).First();
            
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
            smtp.EnableSsl = true;
            MailAddress from = new MailAddress(user_email, "OpenMarket - " + username);
            MailAddress to = new MailAddress(seller_email);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = "Contacto de utilizador";
            mailMessage.IsBodyHtml = true;
            reader = new StreamReader("wwwroot/templates/emails/contact-client.htm");
            body = reader.ReadToEnd();

            body = body.Replace("{name}", username);
            body = body.Replace("{title}", title);
            body = body.Replace("{message}", email_body.Trim());
            mailMessage.Body = body;
            smtp.Send(mailMessage);
            valueFormat formatter = new valueFormat();
            var url = "anuncio?" + formatter.urlFormat(title) + "&id=" + advert_id;
            return Redirect(url + "&Msg=email-enviado");
        }
        public void Adverts_Category(string category, int advert)
        {
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  where x.status == 1 && x.category == category && x.id != advert
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      date = x.date,
                                                      municipality = x.municipality,
                                                      city = x.city,
                                                      country = w.name,
                                                      date_month = x.date.ToString("MMM"),
                                                      image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                                      category = x.category,
                                                      groupName = x.groupName,
                                                      type = x.type
                                                  };
            adverts_category = filter_adverts.ToList();
            List<int> listID = new List<int>();
            foreach (var item in adverts_category)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (adverts_category.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var id = adverts_category.FirstOrDefault(x => x.id == listID[i]);
                    adverts_category.Remove(id);
                }
            }
            Random rdm = new Random();
            int maxRandom = adverts_category.Count();
            if (maxRandom <= 10)
            {
                adverts_category = adverts_category.Take(10).ToList();
            }
            else
            {
                int toSkip = rdm.Next(0, maxRandom);
                if (toSkip > maxRandom - 10)
                {
                    toSkip = maxRandom - 10;
                }
                adverts_category = adverts_category.Skip(toSkip).Take(10).ToList();
            }
        }
        public void User_Adverts(int account, int advert)
        {
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  where x.status == 1 && x.account == account && x.id != advert
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      date = x.date,
                                                      municipality = x.municipality,
                                                      city = x.city,
                                                      country = w.name,
                                                      date_month = x.date.ToString("MMM"),
                                                      image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                                      category = x.category,
                                                      groupName = x.groupName,
                                                      type = x.type
                                                  };
            user_adverts = filter_adverts.ToList();
            List<int> listID = new List<int>();
            foreach (var item in user_adverts)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (user_adverts.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var id = user_adverts.FirstOrDefault(x => x.id == listID[i]);
                    user_adverts.Remove(id);
                }
            }
            Random rdm = new Random();
            int maxRandom = user_adverts.Count();
            if (maxRandom <= 10)
            {
                user_adverts = user_adverts.Take(10).ToList();
            }
            else
            {
                int toSkip = rdm.Next(0, maxRandom);
                if (toSkip > maxRandom - 10)
                {
                    toSkip = maxRandom - 10;
                }
                user_adverts = user_adverts.Skip(toSkip).Take(10).ToList();
            }
        }
    }
}
