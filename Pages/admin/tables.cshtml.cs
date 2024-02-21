using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class tablesModel : PageModel
    {
        public AppDbContext db;
        public tablesModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<tables> tables_list;
        [BindProperty] public tables tables { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalTables { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalTables, PageSize));
        [BindProperty] public string Msg { get; set; }
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
            getTables();
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
        public IActionResult OnPostAdd(IFormFile uploadImage)
        {
            if (tables.name != null)
            {
                var newTable = new tables
                {
                    name = tables.name,
                    table = tables.table
                };
                db.tables.Add(newTable);
                db.SaveChanges();
                return RedirectToPage("tables");
            }
            else
            {
                Msg = "Insira o nome da tabela!";
                getTables();
                return Page();
            }
        }
        public IActionResult OnPostDelete(int delete)
        {
            db.Remove(db.tables.First(x => x.id == delete));
            db.SaveChanges();
            return RedirectToPage("tables");
        }
        public void getTables()
        {
            IQueryable<tables> filterTables;
            filterTables = (from x in db.tables
                            select new tables
                            {
                                id = x.id,
                                name = x.name
                            });
            TotalTables = filterTables.Count();
            tables_list = filterTables.OrderBy(x => x.name).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
        }
    }
}
