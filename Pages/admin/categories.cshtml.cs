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
    public class categoriesModel : PageModel
    {
        public AppDbContext db;
        public categoriesModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<groups> groups_list;
        public IList<generic> categories_list;
        [BindProperty] public categories categories { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalCategories { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalCategories, PageSize));
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
            groups_list = db.groups.OrderBy(x => x.name).ToList();
            getCategories();
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
        public IActionResult OnPostAdd(IFormFile uploadImage)
        {
            if (categories.name.Trim() != null)
            {
                var newCategory = new categories
                {
                    name = categories.name.Trim(),
                    group_id = categories.group_id,
                    indexed = categories.indexed
                };
                byte[] categoryImage;
                if (uploadImage != null)
                {
                    var image = SixLabors.ImageSharp.Image.Load(uploadImage.OpenReadStream());
                    int width = 0;
                    int height = 0;
                    if ((image.Width < 2001 && image.Width > 500) || (image.Height < 2001 && image.Height > 500))
                    {
                        width = image.Width / 2;
                        height = image.Height / 2;
                    }
                    else if (image.Width > 8000 || image.Height > 8000)
                    {
                        width = image.Width / 9;
                        height = image.Height / 9;
                    }
                    else if (image.Width > 7000 || image.Height > 7000)
                    {
                        width = image.Width / 8;
                        height = image.Height / 8;
                    }
                    else if (image.Width > 6000 || image.Height > 6000)
                    {
                        width = image.Width / 7;
                        height = image.Height / 7;
                    }
                    else if (image.Width > 5000 || image.Height > 5000)
                    {
                        width = image.Width / 6;
                        height = image.Height / 6;
                    }
                    else if (image.Width > 4000 || image.Height > 4000)
                    {
                        width = image.Width / 5;
                        height = image.Height / 5;
                    }
                    else if (image.Width > 3000 || image.Height > 3000)
                    {
                        width = image.Width / 4;
                        height = image.Height / 4;
                    }
                    else if (image.Width > 2000 || image.Height > 2000)
                    {
                        width = image.Width / 3;
                        height = image.Height / 3;
                    }
                    image.Mutate(x => x.Resize(width, height));
                    MemoryStream ms = new MemoryStream();
                    image.SaveAsJpeg(ms);
                    categoryImage = ms.ToArray();
                    newCategory.picture = categoryImage;
                }
                db.categories.Add(newCategory);
                db.SaveChanges();
                return RedirectToPage("categories");
            }
            else
            {
                Msg = "Introduza o nome da categoria!";
                getCategories();
                return Page();
            }
        }
        public IActionResult OnPostDelete(int delete)
        {
            db.Remove(db.categories.First(x => x.id == delete));
            db.SaveChanges();
            return RedirectToPage("categories");
        }
        public IActionResult OnPostAds(int category_id, int ads)
        {
            var category = db.categories.First(x => x.id == category_id);
            db.Update(category);
            db.SaveChanges();
            return RedirectToPage("categories");
        }
        public IActionResult OnPostImage(int category_id, IFormFile uploadImage)
        {
            var category = db.categories.First(x => x.id == category_id);
            if (uploadImage != null)
            {
                byte[] categoryImage;
                var image = SixLabors.ImageSharp.Image.Load(uploadImage.OpenReadStream());
                int width = 0;
                int height = 0;
                if ((image.Width < 2001 && image.Width > 500) || (image.Height < 2001 && image.Height > 500))
                {
                    width = image.Width / 2;
                    height = image.Height / 2;
                }
                else if (image.Width > 8000 || image.Height > 8000)
                {
                    width = image.Width / 9;
                    height = image.Height / 9;
                }
                else if (image.Width > 7000 || image.Height > 7000)
                {
                    width = image.Width / 8;
                    height = image.Height / 8;
                }
                else if (image.Width > 6000 || image.Height > 6000)
                {
                    width = image.Width / 7;
                    height = image.Height / 7;
                }
                else if (image.Width > 5000 || image.Height > 5000)
                {
                    width = image.Width / 6;
                    height = image.Height / 6;
                }
                else if (image.Width > 4000 || image.Height > 4000)
                {
                    width = image.Width / 5;
                    height = image.Height / 5;
                }
                else if (image.Width > 3000 || image.Height > 3000)
                {
                    width = image.Width / 4;
                    height = image.Height / 4;
                }
                else if (image.Width > 2000 || image.Height > 2000)
                {
                    width = image.Width / 3;
                    height = image.Height / 3;
                }
                image.Mutate(x => x.Resize(width, height));
                MemoryStream ms = new MemoryStream();
                image.SaveAsJpeg(ms);
                categoryImage = ms.ToArray();
                category.picture = categoryImage;
                db.Update(category);
                db.SaveChanges();
            }
            return RedirectToPage("categories");
        }
        public void getCategories()
        {
            IQueryable<generic> filterCategories = from x in db.categories
                                                   join w in db.groups on x.group_id equals w.id
                                                   select new generic
                                                   {
                                                       id = x.id,
                                                       name = x.name,
                                                       father_element = w.name,
                                                       picture = x.picture
                                                   };
            TotalCategories = filterCategories.Count();
            categories_list = filterCategories.OrderBy(x => x.name).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
        }
    }
}
