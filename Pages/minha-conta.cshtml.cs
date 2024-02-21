using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text;
using SixLabors.ImageSharp;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class minha_contaModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public int cookieAlert { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public minha_contaModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty] public string _Msg { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalAdverts { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        [BindProperty(SupportsGet = true)] public int estado { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAdverts, PageSize));
        public IList<_adverts> adverts_list;
        public IList<alerts> alerts_list;

        public IActionResult OnGet()
        {
            _Msg = Request.Query["Msg"];
            if (Request.Cookies["Accepted"] != null)
            {
                Cookies = 1;
            }
            else
            {
                Cookies = 0;
            }
            //Sessão do utilizador
            valueFormat formatter = new valueFormat();
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
                string userCookie = Request.Cookies["fzid"];
                if (userCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(userCookie));
                    if (db.accounts.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("userID", Convert.ToString(SessionUser));
                    }
                    else
                    {
                        return Redirect("~/entrar");
                    }
                }
                else
                {
                    return Redirect("~/entrar");
                }
            }
            if (!string.IsNullOrEmpty(Request.Query["pagina"]))
            {
                currentpage = Convert.ToInt32(Request.Query["pagina"]);
            }
            IQueryable<_adverts> filterAdverts;
            filterAdverts = (from x in db.adverts
                             join y in db.images on x.id equals y.product into images
                             from y in images.DefaultIfEmpty()
                             where x.account == SessionUser && x.status != 11
                             select new _adverts
                             {
                                 id = x.id,
                                 title = x.title,
                                 locality = x.locality,
                                 municipality = x.municipality,
                                 city = x.city,
                                 image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                 date = x.date,
                                 date_month = x.date.ToString("MMM"),
                                 price = x.price,
                                 status = x.status,
                                 groupName = x.groupName,
                                 type = x.type
                             });
            adverts_list = filterAdverts.ToList();
            List<int> listID = new List<int>();
            foreach (var item in adverts_list)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (adverts_list.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var id = adverts_list.FirstOrDefault(x => x.id == listID[i]);
                    adverts_list.Remove(id);
                }
            }
            TotalAdverts = adverts_list.Count();
            adverts_list = adverts_list.OrderByDescending(x => x.id).Skip((currentpage - 1) * PageSize).Take(PageSize).ToList();
            if (Request.Cookies["fz_ma"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "minha-conta" && x.status == 1).ToList();
            }
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
