using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class carteiraModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Cookies { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public carteiraModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        public IList<generic> movsList;
        [BindProperty(SupportsGet = true)] public int walletValue { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalMovs { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalMovs, PageSize));
        public IList<alerts> alerts_list;
        public IActionResult OnGet()
        {
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
                if (Request.Cookies["fz_wlt"] == null)
                {
                    alerts_list = db.alerts.Where(x => x.page == "carteira" && x.status == 1).ToList();
                }
            }
            if (!string.IsNullOrEmpty(Request.Query["pagina"]))
            {
                currentpage = Convert.ToInt32(Request.Query["pagina"]);
            }
            walletValue = db.accounts.Where(x => x.id == SessionUser).Select(x => x.wallet).Single();
            IQueryable<generic> filter = from x in db.wallet_movs
                                         join y in db.wallet_movs_origin on x.origin equals y.id
                                         where x.user == SessionUser
                                         select new generic
                                         {
                                             id = x.id,
                                             description = x.description,
                                             value = x.value,
                                             type = y.type,
                                             origin = x.origin,
                                             date = x.date,
                                         };
            TotalMovs = filter.ToList().Count();
            movsList = filter.OrderByDescending(x => x.id).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
            return null;
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
