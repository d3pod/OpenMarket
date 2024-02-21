using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class categoriasModel : PageModel
    {
        public AppDbContext db { get; set; }
        public categoriasModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet =true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public int Cookies { get; set; }
        public IList<groups> Groups;
        public IList<categories> Categories;
        public IList<alerts> alerts_list;
        public async Task OnGetAsync()
        {
            if (Request.Cookies["accepted"] != null)
            {
                Cookies = 1;
            }
            else
            {
                Cookies = 0;
            }
            //Sessão do utilizador
            valueFormat formatter = new valueFormat();
            if(!string.IsNullOrEmpty(HttpContext.Session.GetString("userID")))
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
                }
            }
            else
            {
                string userCookie = Request.Cookies["fzid"];
                if (userCookie != null)
                {
                    int id = Convert.ToInt32(formatter.decrypter(userCookie));
                    if(db.accounts.Where(x => x.id == id).Count() == 1)
                    {
                        SessionUser = id;
                        HttpContext.Session.SetString("userID", Convert.ToString(SessionUser));
                    }
                }
            }
            Groups = await db.groups.ToListAsync();
            Categories = await db.categories.ToListAsync();
            if (Request.Cookies["fz_ctg"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "categorias" && x.status == 1).ToList();
            }
        }
    }
}
