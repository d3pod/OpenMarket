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
    public class municipalitiesModel : PageModel
    {
        public AppDbContext db;
        public municipalitiesModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<cities> cities;
        public IList<generic> municipalities_list;
        [BindProperty] public municipalities municipalities { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalMunicipalities { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalMunicipalities, PageSize));
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
            cities = db.cities.OrderBy(x => x.name).ToList();
            getMunicipalities();
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
            if (municipalities.name.Trim() != null)
            {
                var newMunicipality = new municipalities
                {
                    name = municipalities.name.Trim(),
                    city = municipalities.city
                };
                db.municipalities.Add(newMunicipality);
                db.SaveChanges();
                return RedirectToPage("municipalities");
            }
            else
            {
                Msg = "Introduza o nome do município!";
                getMunicipalities();
                return Page();
            }
        }
        public IActionResult OnPostDelete(int delete)
        {
            db.Remove(db.municipalities.First(x => x.id == delete));
            db.SaveChanges();
            return RedirectToPage("municipalities");
        }
        public void getMunicipalities()
        {
            IQueryable<generic> filterMunicipalities;
            filterMunicipalities = (from x in db.municipalities
                                    join w in db.cities on x.city equals w.id
                                    select new generic
                                    {
                                        id = x.id,
                                        name = x.name,
                                        father_element = w.name
                                    });
            TotalMunicipalities = filterMunicipalities.Count();
            municipalities_list = filterMunicipalities.OrderBy(x => x.name).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
        }
    }
}
