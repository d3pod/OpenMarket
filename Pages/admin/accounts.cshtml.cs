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
    public class accountsModel : PageModel
    {
        public AppDbContext db;
        public IWebHostEnvironment appEnvironment;
        public accountsModel(AppDbContext _db, IWebHostEnvironment _appEnvironment)
        {
            db = _db;
            appEnvironment = _appEnvironment;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        [BindProperty(SupportsGet = true)] public string searchText { get; set; }
        [BindProperty] public int TotalAccounts { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAccounts, PageSize));
        public IList<accounts> accounts;

        public async Task OnGetAsync()
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
            IQueryable<accounts> filter = from x in db.accounts
                                          select new accounts
                                          {
                                              id = x.id,
                                              username = x.username,
                                              email = x.email,
                                              city = x.city
                                          };
            TotalAccounts = filter.Count();
            accounts = await filter.OrderBy(x => x.username)
                                                    .Skip((currentpage - 1) * PageSize)
                                                    .Take(PageSize).ToListAsync();
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
        public IActionResult OnPostSearch()
        {
            if (Convert.ToInt32(Request.Query["page"]) > 0)
            {
                currentpage = Convert.ToInt32(Request.Query["page"]);
            }
            IQueryable<accounts> filter = from x in db.accounts
                                          where x.username.ToUpper().Contains(searchText.ToUpper())
                                          select new accounts
                                          {
                                              id = x.id,
                                              username = x.username,
                                              email = x.email,
                                              city = x.city
                                          };
            TotalAccounts = filter.Count();
            accounts = filter.OrderBy(x => x.username)
                                                    .Skip((currentpage - 1) * PageSize)
                                                    .Take(PageSize).ToList();
            return Page();
        }
    }
}