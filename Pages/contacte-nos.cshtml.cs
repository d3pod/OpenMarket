using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Mail;
using System.IO;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class contacte_nosModel : PageModel
    {
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public string From { get; set; }
        [BindProperty] public string Name { get; set; }
        [BindProperty] public string Subject { get; set; }
        [BindProperty(SupportsGet = true)] public string Body { get; set; }
        [BindProperty(SupportsGet = true)] public int advert_id { get; set; } = 0;
        [BindProperty] public _adverts advert { get; set; }
        [BindProperty(SupportsGet = true)] public string subject { get; set; }
        public IList<alerts> alerts_list;
        StreamReader reader;
        string body = string.Empty;
        public AppDbContext db { get; set; }
        public contacte_nosModel(AppDbContext _db)
        {
            db = _db;
        }
        public void OnGet()
        {
            if (Request.Cookies["Accepted"] != null)
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
            advert_id = Convert.ToInt32(Request.Query["anuncio"]);
            subject = Request.Query["assunto"];
            if (advert_id > 0)
            {
                IQueryable<_adverts> filter = from x in db.adverts
                                              join y in db.images on x.id equals y.product into image
                                              from y in image.DefaultIfEmpty()
                                              where x.status == 1 && x.id == advert_id
                                              select new _adverts
                                              {
                                                  id = x.id,
                                                  title = x.title,
                                                  price = x.price,
                                                  date = x.date,
                                                  municipality = x.municipality,
                                                  city = x.city,
                                                  image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                                  groupName = x.groupName,
                                                  price_min = x.price_min,
                                                  price_max = x.price_max,
                                                  orc = x.orc
                                              };
                advert = filter.First();
            }
            if (Request.Cookies["fz_ctc"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "contacte-nos" && x.status == 1).ToList();
            }
        }

        public IActionResult OnPostSend(string user_email, string Name)
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
            smtp.EnableSsl = true;
            MailAddress from = new MailAddress(user_email.Trim());
            MailAddress to = new MailAddress("openmarket@gmail.com");
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.Subject = Subject;
            mailMessage.IsBodyHtml = true;
            reader = new StreamReader("wwwroot/templates/emails/contact-us.htm");
            body = reader.ReadToEnd();
            if (SessionUser > 0)
            {
                string username = db.accounts.Where(x => x.id == SessionUser).Select(x => x.username).First();
                body = body.Replace("{name}", username);
                body = body.Replace("{email}", db.accounts.Where(x => x.id == SessionUser).Select(x => x.email).First());
            }
            else
            {
                body = body.Replace("{name}", Name.Trim());
                body = body.Replace("{email}", user_email.Trim());
            }
            body = body.Replace("{subject}", Subject);
            body = body.Replace("{message}", Body.Trim());
            mailMessage.Body = body;
            smtp.Send(mailMessage);
            return RedirectToPage("contacte-nos");
        }
    }
}
