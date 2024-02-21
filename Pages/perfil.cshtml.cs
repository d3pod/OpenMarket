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
    public class perfilModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public perfilModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty(SupportsGet = true)] public string username { get; set; }
        [BindProperty(SupportsGet = true)] public int user_id { get; set; }
        [BindProperty(SupportsGet = true)] public int TotalAdverts { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAdverts, PageSize));
        public IList<_adverts> adverts_list;
        public IList<accounts> user;
        [BindProperty(SupportsGet = true)] public DateTime last_login { get; set; }
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
                }
            }
            if (!string.IsNullOrEmpty(Request.Query["pagina"]))
            {
                currentpage = Convert.ToInt32(Request.Query["pagina"]);
            }
            if (!string.IsNullOrEmpty(Request.Query["id"]))
            {
                user_id = Convert.ToInt32(Request.Query["id"]);
                username = db.accounts.Where(x => x.id == user_id).Select(x => x.username).First();
                user = db.accounts.Where(x => x.id == user_id).ToList();
                last_login = db.login_logs.Where(x => x.account == user_id).OrderByDescending(x => x.id).Select(x => x.date).FirstOrDefault();
                IQueryable<_adverts> filterAdverts;
                filterAdverts = (from x in db.adverts
                                 join y in db.images on x.id equals y.product into images
                                 from y in images.DefaultIfEmpty()
                                 where x.account == user_id && x.status == 1
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
                if (Request.Cookies["fz_pro"] == null)
                {
                    alerts_list = db.alerts.Where(x => x.page == "perfil" && x.status == 1).ToList();
                }
                return null;
            }
            else
            {
                return RedirectToPage("~/index");
            }
        }
    }
}
