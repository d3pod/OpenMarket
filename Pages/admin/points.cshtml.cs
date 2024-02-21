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
    public class pointsModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment appEnvironment;
        public pointsModel(AppDbContext _db, IWebHostEnvironment _appEnvironment)
        {
            db = _db;
            appEnvironment = _appEnvironment;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<generic> movsList;
        public IList<wallet_movs_origin> movsType;
        public IList<accounts> accounts;
        [BindProperty] public int TotalMovs { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalMovs, PageSize));
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
            Permission = db.users.Where(x => x.id == SessionUser).Select(x => x.rule).FirstOrDefault();
            accounts = db.accounts.OrderBy(x => x.username).ToList();
            movsType = db.wallet_movs_origin.OrderBy(x => x.name).ToList();
            TotalMovs = db.wallet_movs.OrderByDescending(x => x.id).ToList().Count();
            IQueryable<generic> filter = from x in db.wallet_movs
                                         join y in db.wallet_movs_origin on x.origin equals y.id
                                         join z in db.accounts on x.user equals z.id
                                         select new generic
                                         {
                                             id = x.id,
                                             description = x.description,
                                             value = x.value,
                                             type = y.type,
                                             origin = x.origin,
                                             date = x.date,
                                             userID = x.id,
                                             username = z.username
                                         };
            movsList = filter.OrderByDescending(x => x.id)
                                                    .Skip((currentpage - 1) * PageSize)
                                                    .Take(PageSize).ToList();
        }
        public IActionResult OnPostUpdate(int user_select, int type, int value)
        {
            accounts = db.accounts.OrderBy(x => x.username).ToList();
            wallet wallet = new wallet(db);
            if (user_select != 0)
            {
                if (value > 0)
                {
                    wallet.insertMov(type, user_select, value, true);
                }
                else
                {
                    wallet.insertMov(type, user_select, 0, true);
                }
            }
            else
            {
                foreach (var item in accounts)
                {
                    if (value > 0)
                    {
                        wallet.insertMov(type, item.id, value, false);
                    }
                    else
                    {
                        wallet.insertMov(type, item.id, 0, false);
                    }
                }
            }
            return RedirectToPage("points");
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
