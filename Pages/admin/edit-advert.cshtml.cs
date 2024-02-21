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
using System.Net.Mail;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class edit_advertModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment env { get; set; }
        public edit_advertModel(AppDbContext _db, IWebHostEnvironment _env)
        {
            db = _db;
            env = _env;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        [BindProperty] public adverts adverts { get; set; }
        [BindProperty] public accounts account { get; set; }
        public IList<groups> groups;
        public IList<categories> categories;
        public IList<cities> cities;
        public IList<municipalities> municipalities;
        public IList<images> images;
        [BindProperty] public string _Msg { get; set; }
        [BindProperty] public IFormFile[] uploadImages { get; set; }
        StreamReader reader;
        [BindProperty(SupportsGet = true)] public string subject { get; set; }
        [BindProperty(SupportsGet = true)] public string body { get; set; }
        [BindProperty(SupportsGet = true)] public string body_text { get; set; }
        public void OnGet(int id, string Msg)
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
                    int adminID = Convert.ToInt32(formatter.decrypter(adminCookie));
                    if (db.users.Where(x => x.id == adminID).Count() == 1)
                    {
                        SessionUser = adminID;
                        HttpContext.Session.SetString("adminID", Convert.ToString(SessionUser));
                    }
                    else
                    {
                        Redirect("~/admin/login");
                    }
                }
                else
                {
                    Redirect("~/admin/login");
                }
            }
            _Msg = Msg;
            Permission = db.users.Where(x => x.id == SessionUser).Select(x => x.rule).First();
            groups = db.groups.OrderBy(x => x.name).ToList();
            cities = db.cities.OrderBy(x => x.name).ToList();
            adverts = db.adverts.Where(x => x.id == id).First();
            var group = db.groups.Where(x => x.name == adverts.groupName).Select(x => x.id).First();
            categories = db.categories.Where(x => x.group_id == group).OrderBy(x => x.name).ToList();
            var city = db.cities.Where(x => x.name == adverts.city).Select(x => x.id).First();
            municipalities = db.municipalities.Where(x => x.city == city).OrderBy(x => x.name).ToList();
            images = db.images.Where(x => x.product == adverts.id).ToList();
            account = db.accounts.Where(x => x.id == adverts.account).First();
        }
        public IActionResult OnGetCategories(string group)
        {
            var group_id = db.groups.Where(x => x.name == group).Select(x => x.id).First();
            var categories = db.categories.Where(x => x.group_id == group_id).OrderBy(x => x.name).Select(x => x.name).ToList();
            return new JsonResult(categories);
        }
        public IActionResult OnGetMunicipalities(string city)
        {
            var city_id = db.cities.Where(x => x.name == city).Select(x => x.id).First();
            var municipalities = db.municipalities.Where(x => x.city == city_id).OrderBy(x => x.name).Select(x => x.name).ToList();
            return new JsonResult(municipalities);
        }
        public IActionResult OnPostUpdate(int id, int image_id, int email)
        {
            valueFormat formatter = new valueFormat();
            //Apaga imagem
            if (image_id > 0)
            {
                var path = env.WebRootPath + "/anuncios/" + db.images.Where(x => x.id == image_id).Select(x => x.filename).FirstOrDefault();
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
                db.Remove(db.images.First(x => x.id == image_id));
                db.SaveChanges();
                return RedirectToPage("edit-advert", new { id = id });
            }
            //Eliminar
            else if (adverts.status == 9)
            {
                while (db.images.Where(x => x.product == id).Select(x => x.id).Count() > 0)
                {
                    var path = env.WebRootPath + "/anuncios/" + db.images.Where(x => x.id == image_id).Select(x => x.filename).FirstOrDefault();
                    FileInfo file = new FileInfo(path);
                    if (file.Exists)
                    {
                        file.Delete();
                    }
                    db.Remove(db.images.First(x => x.product == id));
                    db.SaveChanges();
                }
                db.Remove(db.adverts.First(x => x.id == id));
                db.SaveChanges();
                return RedirectToPage("adverts");
            }
            //Burla
            else if (adverts.status == 11)
            {
                var updateAdvert = db.adverts.Where(x => x.id == id).First();
                account = db.accounts.Where(x => x.id == updateAdvert.account).First();
                updateAdvert.status = adverts.status;
                db.Update(updateAdvert);
                db.SaveChanges();
                var userAdverts = db.adverts.Where(x => x.account == account.id).ToList();
                foreach (var item in userAdverts)
                {
                    item.status = 11;
                    db.Update(item);
                    db.SaveChanges();
                }
                account.status = 11;
                db.Update(account);
                db.SaveChanges();
                //Email
                if (!string.IsNullOrEmpty(account.email))
                {
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                    smtp.EnableSsl = true;
                    MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                    MailAddress to = new MailAddress(account.email);
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = "Conta bloqueada";
                    mailMessage.IsBodyHtml = true;
                    reader = new StreamReader("wwwroot/templates/emails/account-blocked.htm");
                    body = reader.ReadToEnd();
                    body = body.Replace("{username}", account.username);
                    body = body.Replace("{advert}", updateAdvert.title);
                    mailMessage.Body = body;
                    smtp.Send(mailMessage);
                }
                return RedirectToPage("adverts");
            }
            else if (email == 1)
            {
                if (!string.IsNullOrEmpty(body_text))
                {
                    adverts = db.adverts.Where(x => x.id == id).First();
                    account = db.accounts.Where(x => x.id == adverts.account).First();
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                    smtp.EnableSsl = true;
                    MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                    MailAddress to = new MailAddress(account.email);
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = "Validação de anúncios";
                    mailMessage.IsBodyHtml = true;
                    reader = new StreamReader("wwwroot/templates/emails/advert-contact.htm");
                    body = reader.ReadToEnd();
                    body = body.Replace("{username}", account.username);
                    body = body.Replace("{body}", body_text);
                    body = body.Replace("{advert_id}", adverts.id.ToString());
                    body = body.Replace("{advert_title}", adverts.title);
                    body = body.Replace("{advert_date}", adverts.date.ToShortDateString());
                    mailMessage.Body = body;
                    smtp.Send(mailMessage);
                    return RedirectToPage("edit-advert", new { Msg = "Email1" });
                }
                else
                {
                    return RedirectToPage("edit-advert", new { Msg = "Email2" });
                }
            }
            else
            {
                var updateAdvert = db.adverts.First(x => x.id == id);
                var user = db.accounts.First(x => x.id == updateAdvert.account);

                var advert_name = updateAdvert.title.Replace(" ", "_");
                advert_name = advert_name.Replace("/", "_");
                advert_name = advert_name.Replace("\"", "&quot;");
                advert_name = advert_name.Replace("'", "&apos;");
                advert_name = advert_name.Replace("<", "&lt;");
                advert_name = advert_name.Replace(">", "&gt;");
                advert_name = advert_name.Replace("&", "&amp;");

                if (adverts.status == 1 && updateAdvert.status != 1 && !string.IsNullOrEmpty(user.email))
                {
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                    MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                    MailAddress to = new MailAddress(user.email);
                    MailMessage mailMessage = new MailMessage(from, to);
                    mailMessage.Subject = "O seu anúncio foi aprovado";
                    mailMessage.IsBodyHtml = true;
                    reader = new StreamReader("wwwroot/templates/emails/advert-accepted.htm");
                    body = reader.ReadToEnd();
                    body = body.Replace("{username}", user.username);
                    body = body.Replace("{advert_id}", updateAdvert.id.ToString());
                    body = body.Replace("{advert_title}", updateAdvert.title);
                    body = body.Replace("{advert_date}", updateAdvert.date.ToShortDateString());
                    body = body.Replace("{url}", "https://www.openmarket.com/anuncio?" + advert_name + "&id=" + updateAdvert.id);
                    mailMessage.Body = body;
                    smtp.Send(mailMessage);
                }

                updateAdvert.title = formatter.removeEmojis(adverts.title.Trim());
                updateAdvert.description = adverts.description.Trim();
                updateAdvert.price = adverts.price;
                updateAdvert.price_min = adverts.price_min;
                updateAdvert.price_max = adverts.price_max;
                updateAdvert.groupName = adverts.groupName;
                updateAdvert.category = adverts.category;
                updateAdvert.city = adverts.city;
                updateAdvert.municipality = adverts.municipality;
                updateAdvert.locality = formatter.removeEmojis(adverts.locality.Trim());
                updateAdvert.contact = formatter.removeEmojis(adverts.contact.Replace(" ", "").Trim());
                if (adverts.groupName == "Emprego")
                {
                    updateAdvert.type = "Emprego";
                }
                else
                {
                    updateAdvert.type = adverts.type;
                }
                updateAdvert.negotiable = adverts.negotiable;
                updateAdvert.email_visible = adverts.email_visible;
                updateAdvert.vendor = adverts.vendor;
                if (adverts.groupName == "Emprego")
                {
                    updateAdvert.type = "Emprego";
                }
                else
                {
                    updateAdvert.type = adverts.type;
                }
                updateAdvert.orc = adverts.orc;
                updateAdvert.period = adverts.period;
                updateAdvert.contract = adverts.contract;
                if (adverts.status == 1)
                {
                    updateAdvert.expiration = DateTime.Now.AddDays(180);
                }
                if (updateAdvert.status == 0 && adverts.status == 1)
                {
                    //Adiciona pontos à carteira
                    wallet wallet = new wallet(db);
                    wallet.insertMov(1, updateAdvert.account, 0, true);
                    //Publica no Facebook
                    var url = "https://openmarket.com/anuncio?";
                    var advert_id = "&id=" + updateAdvert.id;
                    url = url + formatter.urlFormat(updateAdvert.title) + advert_id;
                    string token = "Your_Token";
                    string pageID = "Page_Id";
                    long scheduled_publish_time = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600;
                    Facebook facebook = new Facebook(token, pageID);
                    string result = facebook.PublishToFacebook("Novidade no OpenMarket: " + adverts.title + ".\nSabe mais sobre este e outros anúncios em www.openmarket.com \nNo OpenMarket publicar anúncios é grátis!", url, "false", scheduled_publish_time.ToString());
                }
                updateAdvert.status = adverts.status;
                db.Update(updateAdvert);
                db.SaveChanges();

                int totalImages = db.images.Where(x => x.product == id).Count();
                for (int i = 0; i < uploadImages.Count(); i++)
                {
                    var filename = id + "-" + (i + totalImages) + ".jpeg";
                    int number = 1;
                    while (db.images.Any(x => x.filename == filename))
                    {
                        filename = id + "-" + (i + totalImages + number) + ".jpeg";
                        number++;
                    }
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "anuncios", filename);
                    var image = Image.Load(uploadImages[i].OpenReadStream());
                    int width = 0;
                    int height = 0;
                    if ((image.Width < 2001 && image.Width > 500) || (image.Height < 2001 && image.Height > 500))
                    {
                        width = image.Width / 2;
                        height = image.Height / 2;
                    }
                    else if (image.Width > 8000 || image.Height > 8000)
                    {
                        width = image.Width / 9;
                        height = image.Height / 9;
                    }
                    else if (image.Width > 7000 || image.Height > 7000)
                    {
                        width = image.Width / 8;
                        height = image.Height / 8;
                    }
                    else if (image.Width > 6000 || image.Height > 6000)
                    {
                        width = image.Width / 7;
                        height = image.Height / 7;
                    }
                    else if (image.Width > 5000 || image.Height > 5000)
                    {
                        width = image.Width / 6;
                        height = image.Height / 6;
                    }
                    else if (image.Width > 4000 || image.Height > 4000)
                    {
                        width = image.Width / 5;
                        height = image.Height / 5;
                    }
                    else if (image.Width > 3000 || image.Height > 3000)
                    {
                        width = image.Width / 4;
                        height = image.Height / 4;
                    }
                    else if (image.Width > 2000 || image.Height > 2000)
                    {
                        width = image.Width / 3;
                        height = image.Height / 3;
                    }
                    image.Mutate(x => x.Resize(width, height));
                    MemoryStream ms = new MemoryStream();
                    image.SaveAsJpeg(ms);
                    image.Save(path);
                    var images = new images
                    {
                        product = id,
                        status = 1,
                        filename = filename
                    };
                    db.images.Add(images);
                }
                db.SaveChanges();
                return RedirectToPage("adverts");
            }
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
    }
}
