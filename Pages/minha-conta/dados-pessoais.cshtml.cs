using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class dados_pessoaisModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public string _Msg { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        [BindProperty] public accounts accounts { get; set; }
        [BindProperty(SupportsGet = true)] public IFormFile uploadImages { get; set; }
        public IList<cities> cities;
        public IList<municipalities> municipalities;
        public IList<alerts> alerts_list;
        public dados_pessoaisModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
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
            if (Request.Cookies["fz_dp"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "dados-pessoais" && x.status == 1).ToList();
            }
            accounts = db.accounts.First(x => x.id == SessionUser);
            cities = db.cities.OrderBy(x => x.name).ToList();
            if (!string.IsNullOrEmpty(accounts.city))
            {
                int city = db.cities.Where(x => x.name == accounts.city).Select(x => x.id).First();
                municipalities = db.municipalities.Where(x => x.city == city).OrderBy(x => x.name).ToList();
            }
            return null;
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
        public IActionResult OnPostData()
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            var user = db.accounts.First(x => x.id == SessionUser);
            if (uploadImages != null)
            {
                var image = SixLabors.ImageSharp.Image.Load(uploadImages.OpenReadStream());
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
                user.picture = ms.ToArray();
            }
            user.first_name = accounts.first_name;
            user.last_name = accounts.last_name;
            if (!string.IsNullOrEmpty(accounts.email))
            {
                user.email = accounts.email;
            }
            if (accounts.birthday.ToShortDateString() != "01/01/0001")
            {
                user.birthday = accounts.birthday;
            }
            user.city = accounts.city;
            user.locality = accounts.locality;
            user.municipality = accounts.municipality;
            db.Update(user);
            db.SaveChanges();
            return RedirectToPage("dados-pessoais", new { Msg = "Sucesso" });
        }
        public IActionResult OnPostPassword(string password, string newPassword, string newPasswordR)
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            string userPassword = db.accounts.Where(x => x.id == SessionUser).Select(x => x.password).First().ToString();
            if (string.IsNullOrEmpty(userPassword) && !string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(newPasswordR))
            {
                var user = db.accounts.First(x => x.id == SessionUser);
                user.password = newPassword;
                db.Update(user);
                db.SaveChanges();
                return RedirectToPage("dados-pessoais", new { Msg = "Sucesso" });
            }
            else if (!string.IsNullOrEmpty(userPassword) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(newPasswordR))
            {
                var user = db.accounts.First(x => x.id == SessionUser);
                user.password = newPassword;
                db.Update(user);
                db.SaveChanges();
                return RedirectToPage("dados-pessoais", new { Msg = "Sucesso" });
            }
            else
            {
                return RedirectToPage("dados-pessoais", new { Msg = "Password" });
            }
        }
        public IActionResult OnPostLogout()
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
            return Redirect("~/index");
        }
    }
}
