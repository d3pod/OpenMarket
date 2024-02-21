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
    public class conversationsModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment appEnvironment;
        public conversationsModel(AppDbContext _db, IWebHostEnvironment _appEnvironment)
        {
            db = _db;
            appEnvironment = _appEnvironment;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<accounts> accounts;
        public void OnGet()
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
                        Redirect("~/admin/login");
                    }
                }
                else
                {
                    Redirect("~/admin/login");
                }
            }
            Permission = db.users.Where(x => x.id == SessionUser).Select(x => x.rule).FirstOrDefault();
            accounts = db.accounts.OrderBy(x => x.username).ToList();
        }
        public IActionResult OnPostSend(int user_select, string message_body)
        {
            accounts = db.accounts.OrderBy(x => x.username).ToList();
            var newPost = new conversations();
            if (user_select != 0)
            {
                newPost = new conversations
                {
                    sender = 1,
                    receiver = user_select,
                    message = message_body,
                    date = DateTime.Now,
                    status = "Por ler"
                };
                db.conversations.Add(newPost);
                db.SaveChanges();
            }
            else
            {
                foreach (var item in accounts)
                {
                    newPost = new conversations
                    {
                        sender = 1,
                        receiver = item.id,
                        message = message_body,
                        date = DateTime.Now,
                        status = "Por ler"
                    };
                    db.conversations.Add(newPost);
                    db.SaveChanges();
                }
            }
            return RedirectToPage("conversations");
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
