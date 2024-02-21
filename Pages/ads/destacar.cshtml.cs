using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class destacarModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Cookies { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public destacarModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty] public string Title { get; set; }
        public IList<ads_plans> plans;
        [BindProperty] public ads_movs ads_movs { get; set; }
        public IList<alerts> alerts_list;
        [BindProperty(SupportsGet = true)] public int advert_id { get; set; }
        [BindProperty] public string _Msg { get; set; }
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
            if (Request.Cookies["fz_dstc"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "destacar" && x.status == 1).ToList();
            }
            advert_id = Convert.ToInt32(Request.Query["anuncio"]);
            if (advert_id > 0)
            {
                if (db.adverts.Where(x => x.id == advert_id).Select(x => x.account).Single() != Convert.ToInt32(SessionUser))
                {
                    return Redirect("~/minha-conta");
                }
                else
                {
                    Title = db.adverts.Where(x => x.id == advert_id).Select(x => x.title).Single();
                    string categoryName = db.adverts.Where(x => x.id == advert_id).Select(x => x.category).First();
                    plans = db.ads_plans.Where(x => x.category == categoryName).ToList();
                    return null;
                }
            }
            else
            {
                return Redirect("~/minha-conta");
            }
        }
        public IActionResult OnPostSubscribe(int id, int plan_id)
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            if (SessionUser == db.adverts.Where(x => x.id == id).Select(x => x.account).First())
            {
                if (db.ads_movs.Any(x => x.advert == id && x.expiration.Date > DateTime.Now.Date))
                {
                    return Redirect("~/minha-conta?Msg=PlanoDecorrer");
                }
                else
                {
                    int plan_price = db.ads_plans.Where(x => x.id == plan_id).Select(x => x.price).First();
                    int plan_days = db.ads_plans.Where(x => x.id == plan_id).Select(x => x.days_principal).First();
                    int user_wallet = db.accounts.Where(x => x.id == SessionUser).Select(x => x.wallet).First();
                    if (user_wallet >= plan_price)
                    {
                        var newMov = new ads_movs()
                        {
                            plan = plan_id,
                            date = DateTime.Now,
                            advert = id,
                            expiration = DateTime.Now.AddDays(plan_days)
                        };
                        db.ads_movs.Add(newMov);
                        db.SaveChanges();

                        wallet wallet = new wallet(db);
                        wallet.insertPlan(plan_id, SessionUser);
                        return Redirect("~/minha-conta?Msg=NovoPlano");
                    }
                    else
                    {
                        return Redirect("~/ads/destacar?anuncio=" + id + "&Msg=SemPontos");
                    }
                }
            }
            else
            {
                return Redirect("~/minha-conta");
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
