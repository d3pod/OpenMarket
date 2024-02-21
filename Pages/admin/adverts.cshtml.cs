using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Text;
using System.Xml;
using Microsoft.EntityFrameworkCore;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class advertsModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment appEnvironment;
        public advertsModel(AppDbContext _db, IWebHostEnvironment _appEnvironment)
        {
            db = _db;
            appEnvironment = _appEnvironment;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        [BindProperty(SupportsGet = true)] public string searchText { get; set; }
        [BindProperty] public int TotalAdverts { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAdverts, PageSize));
        public IList<_adverts> adverts;
        public int status { get; set; } = 1;
        public string username { get; set; }
        public void OnGet(string searchText)
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

            if (Convert.ToInt32(Request.Query["status"]) != 1 && !string.IsNullOrEmpty(Request.Query["status"]))
            {
                status = Convert.ToInt32(Request.Query["status"]);
            }
            IQueryable<_adverts> filter = from x in db.adverts
                                          join y in db.accounts on x.account equals y.id
                                          where x.status == status
                                          select new _adverts
                                          {
                                              id = x.id,
                                              title = x.title,
                                              category = x.category,
                                              city = x.city,
                                              date_text = x.date.ToShortDateString(),
                                              username = y.username,
                                              status = x.status
                                          };
            if (!string.IsNullOrEmpty(Request.Query["user"]))
            {
                username = Request.Query["user"];
                filter = filter.Where(x => x.username.Replace(" ", "") == username);
            }
            adverts = filter.ToList();
            if (!string.IsNullOrEmpty(searchText))
            {
                adverts = adverts.Where(x => x.title.ToUpper().Contains(searchText.ToUpper())).OrderByDescending(x => x.id).ToList();
            }
            TotalAdverts = adverts.Count();
            adverts = adverts.OrderByDescending(x => x.id)
                                                .Skip((currentpage - 1) * PageSize)
                                                .Take(PageSize).ToList();
        }
        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Remove("adminID");
            if(Request.Cookies["fzadminid"] != null)
            {
                var cookieOptions = new CookieOptions{
                    Expires = DateTime.Now.AddDays(-1)
                };
                Response.Cookies.Append("fzadminid", "0", cookieOptions);
            }
            return Redirect("~/admin/login");
        }
        public IActionResult OnPostSearch(string searchText)
        {
            return Redirect("adverts?searchText=" + searchText);
        }
    }
}
