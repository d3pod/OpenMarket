using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Net;
using System.Net.Mail;
using System.IO;
using Microsoft.AspNetCore.Http;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class recuperar_passwordModel : PageModel
    {
        public AppDbContext db;
        public recuperar_passwordModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public int SessionUser { get; set; } = 0;
        public string _Msg { get; set; }
        StreamReader reader;
        string body = string.Empty;
        public IList<alerts> alerts_list;
        public void OnGet(string Msg)
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
            SessionUser = 0;
            if (Request.Cookies["fzid"] != null)
            {
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Append("fzid", "0", cookieOptions);
            }
            if (Request.Cookies["fz_rp"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "recuperar-password" && x.status == 1).ToList();
            }
        }
        public IActionResult OnPostMail(string email)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVXZabcdefghijklmnopqrstuvxz1234567890";
            var stringChars = new char[8];
            var rdm = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[rdm.Next(chars.Length)];
            }

            string password = new string(stringChars);

            if (db.accounts.Where(x => x.email == email).FirstOrDefault() != null)
            {
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                smtp.EnableSsl = true;
                MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                MailAddress to = new MailAddress(email);
                MailMessage mailMessage = new MailMessage(from, to);
                mailMessage.Subject = "OpenMarket - Nova Password";
                mailMessage.IsBodyHtml = true;
                reader = new StreamReader("wwwroot/templates/emails/new-password.htm");
                body = reader.ReadToEnd();
                body = body.Replace("{mail}", email);
                body = body.Replace("{password}", password);
                var userPassword = db.accounts.First(x => x.email == email);
                userPassword.password = password;
                db.Update(userPassword);
                db.SaveChanges();
                mailMessage.Body = body;
                smtp.Send(mailMessage);
                return RedirectToPage("recuperar-password", new { Msg = "Sucesso" });
            }
            else
            {
                return RedirectToPage("recuperar-password", new { Msg = email });
            }
        }
    }
}
