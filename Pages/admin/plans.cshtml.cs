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
    public class plansModel : PageModel
    {
        public AppDbContext db;
        public plansModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalPlans { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalPlans, PageSize));
        public IList<ads_plans> plansList;
        public IList<categories> categoriesList;
        [BindProperty] public ads_plans plans { get; set; }
        [BindProperty] public IFormFile uploadImage { get; set; }
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
            getPlans();
            getCategories();
        }
        public IActionResult OnPostDelete(int delete)
        {
            db.Remove(db.ads_plans.First(x => x.id == delete));
            db.SaveChanges();
            return RedirectToPage("plans");
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
        public void getPlans()
        {
            plansList = db.ads_plans.OrderBy(x => x.category).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
            TotalPlans = db.ads_plans.Count();
        }
        public void getCategories()
        {
            categoriesList = db.categories.ToList();
        }
        public IActionResult OnPostAdd()
        {
            var filename = plans.category + "_" + plans.name + ".jpeg";
            if (uploadImage != null)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "static/planos", filename);
                var image = Image.Load(uploadImage.OpenReadStream());
                int width = 100;
                int height = 100;
                image.Mutate(x => x.Resize(width, height));
                MemoryStream ms = new MemoryStream();
                image.SaveAsJpeg(ms);
                image.Save(path);
            }
            var newPlan = new ads_plans
            {
                name = plans.name,
                days_top = plans.days_top,
                days_featured = plans.days_featured,
                days_principal = plans.days_principal,
                category = plans.category,
                price = plans.price,
                filename = filename
            };
            db.ads_plans.Add(newPlan);
            db.SaveChanges();
            return RedirectToPage("plans");
        }
        public IActionResult OnPostPlans(string plan_name, int days_top, int days_featured, int days_principal, int price)
        {
            var categorias = db.categories.ToList();
            foreach (var item in categorias)
            {
                var newPlan = new ads_plans
                {
                    name = plan_name,
                    days_top = days_top,
                    days_featured = days_featured,
                    days_principal = days_principal,
                    category = item.name,
                    price = price
                };
                db.ads_plans.Add(newPlan);
                db.SaveChanges();
            }
            return RedirectToPage("plans");
        }
    }
}
