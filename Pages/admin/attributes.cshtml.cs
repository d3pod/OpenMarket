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
    public class attributesModel : PageModel
    {
        public AppDbContext db;
        public attributesModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Permission { get; set; }
        public IList<categories> categories;
        public IList<attributes> attributes_list;
        public IList<tables> tables;
        [BindProperty] public attributes attributes { get; set; }
        [BindProperty] public categories_attributes categories_attributes { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalAttributes { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 30;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAttributes, PageSize));
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
            categories = db.categories.OrderBy(x => x.name).ToList();
            tables = db.tables.OrderBy(x => x.name).ToList();
            getAttributes();
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
        public IActionResult OnPostAdd(IFormFile uploadImage, string name)
        {
            if (attributes.name.Trim() == null)
            {
                attributes.name = name.Trim();
            }
            if (attributes.name != null)
            {
                var newAttribute = new attributes
                {
                    name = attributes.name.Trim(),
                    order = attributes.order,
                    type = attributes.type,
                    ads = attributes.ads
                };
                byte[] attributeImage;
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
                    attributeImage = ms.ToArray();
                    newAttribute.picture = attributeImage;
                }
                db.attributes.Add(newAttribute);
                db.SaveChanges();
                categories_attributes.attribute = db.attributes.Where(x => x.name == attributes.name.Trim()).Select(x => x.id).First();
                var AddCategory_Attribute = new categories_attributes
                {
                    category = categories_attributes.category,
                    attribute = categories_attributes.attribute
                };
                db.categories_attributes.Add(AddCategory_Attribute);
                db.SaveChanges();
                return RedirectToPage("attributes");
            }
            else
            {
                Msg = "Introduza o nome do atributo!";
                getAttributes();
                return Page();
            }
        }
        public IActionResult OnPostDelete(int delete)
        {
            db.Remove(db.attributes.First(x => x.id == delete));
            db.Remove(db.categories_attributes.First(x => x.attribute == delete));
            db.SaveChanges();
            return RedirectToPage("attributes");
        }
        public IActionResult OnPostType(int attribute_id, string type)
        {
            var attribute = db.attributes.First(x => x.id == attribute_id);
            attribute.type = type;
            db.Update(attribute);
            db.SaveChanges();
            return RedirectToPage("attributes");
        }
        public IActionResult OnPostOrder(int attribute_id, int order)
        {
            var attribute = db.attributes.First(x => x.id == attribute_id);
            attribute.order = order;
            db.Update(attribute);
            db.SaveChanges();
            return RedirectToPage("attributes");
        }
        public IActionResult OnPostAds(int attribute_id, int ads)
        {
            var attribute = db.attributes.First(x => x.id == attribute_id);
            attribute.ads = ads;
            db.Update(attribute);
            db.SaveChanges();
            return RedirectToPage("attributes");
        }
        public IActionResult OnPostImage(int attribute_id, IFormFile uploadImage)
        {
            var attribute = db.attributes.First(x => x.id == attribute_id);
            if (uploadImage != null)
            {
                byte[] attributeImage;
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
                attributeImage = ms.ToArray();
                attribute.picture = attributeImage;
                db.Update(attribute);
                db.SaveChanges();
            }
            return RedirectToPage("attributes");
        }
        public void getAttributes()
        {
            IQueryable<attributes> filterAttributes;
            filterAttributes = (from x in db.attributes
                                select new attributes
                                {
                                    id = x.id,
                                    name = x.name,
                                    type = x.type,
                                    order = x.order,
                                    picture = x.picture
                                }).OrderBy(x => x.name).Skip((currentpage - 1) * PageSize).Take(PageSize);
            attributes_list = filterAttributes.OrderBy(x => x.name).ToList();
            TotalAttributes = attributes_list.Count();
        }
    }
}
