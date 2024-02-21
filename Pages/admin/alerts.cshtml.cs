using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class alertsModel : PageModel
    {
        public AppDbContext db;
        public alertsModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        [BindProperty(SupportsGet = true)] public int TotalAlerts { get; set; }
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAlerts, PageSize));
        [BindProperty] public string Msg { get; set; }
        [BindProperty(SupportsGet = true)] public alerts alerts { get; set; }
        public IList<alerts> alerts_list;
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
            if (Convert.ToInt32(Request.Query["page"]) > 0)
            {
                currentpage = Convert.ToInt32(Request.Query["page"]);
            }
            Permission = db.users.Where(x => x.id == SessionUser).Select(x => x.rule).First(); 
            alerts_list = db.alerts.ToList();
        }
        public IActionResult OnPostAdd()
        {
            var newAlert = new alerts
            {
                title = alerts.title,
                text = alerts.text,
                button_text = alerts.button_text,
                button_url = alerts.button_url,
                validation = alerts.validation,
                status = 1,
                page = alerts.page,
                cookie_time = alerts.cookie_time
            };
            db.alerts.Add(newAlert);
            db.SaveChanges();
            return Page();
        }
        public IActionResult OnPostActive(int id)
        {
            var alert = db.alerts.Where(x => x.id == id).First();
            alert.status = 1;
            db.Update(alert);
            db.SaveChanges();
            return RedirectToPage("alerts");
        }
        public IActionResult OnPostDisable(int id)
        {
            var alert = db.alerts.Where(x => x.id == id).First();
            alert.status = 0;
            db.Update(alert);
            db.SaveChanges();
            return RedirectToPage("alerts");
        }
        public IActionResult OnPostDelete(int id)
        {
            db.Remove(db.alerts.First(x => x.id == id));
            db.SaveChanges();
            return RedirectToPage("alerts");
        }
    }
}
