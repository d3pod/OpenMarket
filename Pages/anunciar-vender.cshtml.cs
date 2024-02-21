using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Microsoft.AspNetCore.Hosting;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class anunciar_venderModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public IWebHostEnvironment env { get; set; }
        public anunciar_venderModel(AppDbContext _db, IHttpContextAccessor _accessor, IWebHostEnvironment _env)
        {
            db = _db;
            accessor = _accessor;
            env = _env;
        }
        [BindProperty] public adverts adverts { get; set; }
        public IList<groups> groups;
        public IList<categories> categories;
        public IList<cities> cities;
        public IList<municipalities> municipalities;
        public IList<images> images;
        [BindProperty(SupportsGet = true)] public string _Msg { get; set; }
        [BindProperty] public IFormFile[] uploadImages { get; set; }
        [BindProperty] public int TotalImages { get; set; } = 0;
        [BindProperty(SupportsGet = true)] public int advert_id { get; set; } = 0;
        [BindProperty(SupportsGet = true)] public int image_id { get; set; } = 0;
        public IList<alerts> alerts_list;
        public IActionResult OnGet(string Msg)
        {
            _Msg = Msg;
            if (Request.Cookies["Accepted"] != null)
            {
                Cookies = 1;
            }
            else
            {
                Cookies = 0;
            }
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("userID")))
            {
                SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
                if (db.accounts.Where(x => x.id == SessionUser).Select(x => x.status).Single() == 11)
                {
                    HttpContext.Session.Remove("userID");
                    if (Request.Cookies["fzid"] != null)
                    {
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(-1)
                        };
                        Response.Cookies.Append("fzid", "0", cookieOptions);
                    }
                    return Redirect("~/entrar");
                }
            }
            else
            {
                valueFormat formatter = new valueFormat();
                string userCookie = Request.Cookies["fzid"];
                if (userCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(userCookie));
                    if (db.accounts.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("userID", Convert.ToString(SessionUser));
                    }
                    else if (!string.IsNullOrEmpty(Request.Query["backUrl"]))
                    {
                        return Redirect("~/entrar?backUrl=" + Request.Query["backUrl"]);
                    }
                    else
                    {
                        return Redirect("~/entrar?");
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(Request.Query["backUrl"]))
                    {
                        return Redirect("~/entrar?backUrl=" + Request.Query["backUrl"]);
                    }
                    else
                    {
                        return Redirect("~/entrar?");
                    }
                }
            }
            if (Request.Cookies["fz_av"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "anunciar-vender" && x.status == 1).ToList();
            }
            var user = from x in db.accounts where x.id == SessionUser select x.picture;
            UserPicture = user.FirstOrDefault();
            if (!string.IsNullOrEmpty(Request.Query["anuncio"]))
            {
                advert_id = Convert.ToInt32(Request.Query["anuncio"]);

                if (db.adverts.Where(x => x.id == advert_id).Count() > 0)
                {
                    adverts = db.adverts.First(x => x.id == advert_id);
                    if (adverts.account == SessionUser)
                    {
                        groups = db.groups.OrderBy(x => x.name).ToList();
                        cities = db.cities.OrderBy(x => x.name).ToList();
                        var group = db.groups.Where(x => x.name == adverts.groupName).Select(x => x.id).First();
                        categories = db.categories.Where(x => x.group_id == group).OrderBy(x => x.name).ToList();
                        var city = db.cities.Where(x => x.name == adverts.city).Select(x => x.id).First();
                        municipalities = db.municipalities.Where(x => x.city == city).OrderBy(x => x.name).ToList();
                        images = db.images.Where(x => x.product == adverts.id).ToList();
                        TotalImages = images.Count();
                        return null;
                    }
                    else
                    {
                        return RedirectToPage("minha-conta");
                    }
                }
                else
                {
                    return RedirectToPage("minha-conta");
                }
            }
            else
            {
                groups = db.groups.OrderBy(x => x.name).ToList();
                cities = db.cities.OrderBy(x => x.name).ToList();
                return null;
            }
        }
        public IActionResult OnGetCategories(string group)
        {
            var group_id = db.groups.Where(x => x.name == group).Select(x => x.id).First();
            var categories = db.categories.Where(x => x.group_id == group_id).OrderBy(x => x.name).Select(x => x.name).ToList();
            return new JsonResult(categories);
        }
        public IActionResult OnGetMunicipalities(string city)
        {
            var city_id = db.cities.Where(x => x.name == city).Select(x => x.id).First();
            var municipalities = db.municipalities.Where(x => x.city == city_id).OrderBy(x => x.name).Select(x => x.name).ToList();
            return new JsonResult(municipalities);
        }
        public IActionResult OnPostAdd()
        {
            valueFormat formatter = new valueFormat();
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            if (image_id > 0)
            {
                int advert_id = db.images.Where(x => x.id == image_id).Select(x => x.product).FirstOrDefault();
                var path = env.WebRootPath + "/anuncios/" + db.images.Where(x => x.id == image_id).Select(x => x.filename).FirstOrDefault();
                FileInfo file = new FileInfo(path);
                if (file.Exists)
                {
                    file.Delete();
                }
                db.Remove(db.images.First(x => x.id == image_id));
                db.SaveChanges();
                return Redirect("anunciar-vender?anuncio=" + advert_id);
            }
            else if (advert_id > 0)
            {
                if (string.IsNullOrEmpty(adverts.title))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Titulo" });
                }
                else if (string.IsNullOrEmpty(adverts.groupName))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Categoria" });
                }
                else if (string.IsNullOrEmpty(adverts.description))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Descricao" });
                }
                else if ((string.IsNullOrEmpty(adverts.price.ToString()) || adverts.price < 1) && adverts.orc == "false")
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Preco" });
                }
                else if (string.IsNullOrEmpty(adverts.city))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Cidade" });
                }
                else if (string.IsNullOrEmpty(adverts.locality))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Localidade" });
                }
                else if (string.IsNullOrEmpty(adverts.contact))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Contacto" });
                }

                if (adverts.groupName == "Emprego")
                {
                    if (string.IsNullOrEmpty(adverts.period) || adverts.period == "Selecionar")
                    {
                        return RedirectToPage("anunciar-vender", new { Msg = "Emprego" });
                    }
                    else if (string.IsNullOrEmpty(adverts.contract) || adverts.contract == "Selecionar")
                    {
                        return RedirectToPage("anunciar-vender", new { Msg = "Contracto" });
                    }
                }
                var updateAdvert = db.adverts.First(x => x.id == advert_id);
                updateAdvert.title = formatter.removeEmojis(adverts.title.Trim());
                updateAdvert.description = adverts.description.Trim();
                updateAdvert.price = adverts.price;
                updateAdvert.price_min = adverts.price_min;
                updateAdvert.price_max = adverts.price_max;
                updateAdvert.groupName = adverts.groupName;
                updateAdvert.category = adverts.category;
                updateAdvert.city = adverts.city;
                updateAdvert.municipality = adverts.municipality;
                updateAdvert.locality = formatter.removeEmojis(adverts.locality.Trim());
                updateAdvert.contact = formatter.removeEmojis(adverts.contact.Replace(" ", "").Trim());
                if (updateAdvert.status == 1)
                {
                    updateAdvert.status = 10;
                }
                if (adverts.groupName == "Emprego")
                {
                    updateAdvert.condition = 3;
                }
                else
                {
                    updateAdvert.condition = adverts.condition;
                }
                updateAdvert.condition = adverts.condition;
                updateAdvert.negotiable = adverts.negotiable;
                updateAdvert.email_visible = adverts.email_visible;
                updateAdvert.vendor = adverts.vendor;
                if (adverts.groupName == "Emprego")
                {
                    updateAdvert.type = "Emprego";
                }
                else
                {
                    updateAdvert.type = adverts.type;
                }
                updateAdvert.orc = adverts.orc;
                updateAdvert.period = adverts.period;
                updateAdvert.contract = adverts.contract;
                db.Update(updateAdvert);
                db.SaveChanges();

                int totalImages = db.images.Where(x => x.product == advert_id).Count();
                for (int i = 0; i < uploadImages.Count(); i++)
                {
                    var filename = advert_id + "-" + (i + totalImages) + ".jpeg";
                    int number = 1;
                    while (db.images.Any(x => x.filename == filename))
                    {
                        filename = advert_id + "-" + (i + totalImages + number) + ".jpeg";
                        number++;
                    }
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "anuncios", filename);
                    var image = Image.Load(uploadImages[i].OpenReadStream());
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
                    image.Save(path);
                    var images = new images
                    {
                        product = advert_id,
                        status = 1,
                        filename = filename
                    };
                    db.images.Add(images);
                    db.SaveChanges();
                }
                return Redirect("minha-conta");
            }
            else
            {
                if (string.IsNullOrEmpty(adverts.title))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Titulo" });
                }
                else if (string.IsNullOrEmpty(adverts.groupName))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Categoria" });
                }
                else if (string.IsNullOrEmpty(adverts.description))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Descricao" });
                }
                else if (string.IsNullOrEmpty(adverts.price.ToString()))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Preco" });
                }
                else if (string.IsNullOrEmpty(adverts.city))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Cidade" });
                }
                else if (string.IsNullOrEmpty(adverts.locality))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Localidade" });
                }
                else if (string.IsNullOrEmpty(adverts.contact))
                {
                    return RedirectToPage("anunciar-vender", new { Msg = "Contacto" });
                }
                if (adverts.groupName == "Emprego")
                {
                    adverts.type = "Emprego";
                    if (string.IsNullOrEmpty(adverts.period))
                    {
                        return RedirectToPage("anunciar-vender", new { Msg = "Emprego" });
                    }
                    else if (string.IsNullOrEmpty(adverts.contract))
                    {
                        return RedirectToPage("anunciar-vender", new { Msg = "Contracto" });
                    }
                }
                if (adverts.groupName == "Emprego")
                {
                    adverts.condition = 3;
                }
                var newAdvert = new adverts
                {
                    title = formatter.removeEmojis(adverts.title.Trim()),
                    description = adverts.description.Trim(),
                    price = adverts.price,
                    price_min = adverts.price_min,
                    price_max = adverts.price_max,
                    groupName = adverts.groupName,
                    category = adverts.category,
                    city = adverts.city,
                    municipality = adverts.municipality,
                    locality = formatter.removeEmojis(adverts.locality.Trim()),
                    contact = formatter.removeEmojis(adverts.contact.Replace(" ", "").Trim()),
                    status = 0,
                    condition = adverts.condition,
                    negotiable = adverts.negotiable,
                    date = DateTime.Now,
                    expiration = DateTime.Now.AddDays(28),
                    email_visible = adverts.email_visible,
                    account = SessionUser,
                    vendor = adverts.vendor,
                    type = adverts.type,
                    orc = adverts.orc,
                    period = adverts.period,
                    contract = adverts.contract
                };
                db.adverts.Add(newAdvert);
                db.SaveChanges();
                advert_id = db.adverts.Where(x => x.account == SessionUser).OrderByDescending(x => x.id).Select(x => x.id).FirstOrDefault();

                for (int i = 0; i < uploadImages.Count(); i++)
                {
                    var filename = advert_id + "-" + i + ".jpeg";
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "anuncios", filename);
                    var image = Image.Load(uploadImages[i].OpenReadStream());
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
                    image.SaveAsJpegAsync(ms);
                    image.SaveAsync(path);
                    var images = new images
                    {
                        product = advert_id,
                        status = 1,
                        filename = filename
                    };
                    db.images.Add(images);
                    db.SaveChanges();

                }
                _Msg = "Submetido";
                return RedirectToPage("anunciar-vender", new { Msg = _Msg });
            }
        }
    }
}
