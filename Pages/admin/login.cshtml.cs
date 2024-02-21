using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class loginModel : PageModel
    {
        private AppDbContext db;
        private IHttpContextAccessor accessor;
        public loginModel(AppDbContext _db,  IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty] public users users { get; set; }
        [BindProperty] public int SessionUser { get; set; } = 0;
        public string Msg { get; set; }
        valueFormat formatter = new valueFormat();
        public IActionResult OnGet()
        {
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("userID")))
            {
                SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
                return Redirect("~/admin/dashboard");
            }
            else
            {
                valueFormat formatter = new valueFormat();
                string adminCookie = Request.Cookies["fzadminid"];
                if (adminCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(adminCookie));
                    if (db.users.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("adminID", Convert.ToString(SessionUser));
                        return Redirect("~/admin/dashboard");
                    }
                }
            }
            return null;
        }
        public IActionResult OnPostLogin()
        {
            int id_account = (from x in db.users where x.email == users.email select x.id).FirstOrDefault();
            string mail = (from y in db.users where y.email == users.email select y.email).FirstOrDefault();
            if(!string.IsNullOrEmpty(mail))
            {
                var password = (from x in db.users where x.email == users.email select x.password).FirstOrDefault();
                if(password == users.password)
                {
                    HttpContext.Session.SetString("adminID", Convert.ToString(id_account));
                    var cookieOptions = new CookieOptions
                    {
                        Expires = DateTime.Now.AddDays(30)
                    };
                    Response.Cookies.Append("fzadminid", formatter.encrypter(Convert.ToString(id_account)), cookieOptions);
                    return RedirectToPage("/admin/dashboard");
                }
                else
                {
                    Msg = "Email ou password incorretos";
                    return Page();
                }
            }
            else
            {
                Msg = "Email ou password incorretos";
                return Page();
            }
        }
    }
}
