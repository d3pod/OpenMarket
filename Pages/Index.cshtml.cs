using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text;
using Microsoft.EntityFrameworkCore;
using openmarket.Models;

namespace openmarket.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty] public AppDbContext db { get; set; }
        public IndexModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public string searchText { get; set; }
        public IList<cities> cities;
        public IList<municipalities> municipalities;
        public IList<groups> groups;
        public IList<_adverts> adverts;
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

            cities = await db.cities.OrderBy(x => x.name).ToListAsync();
            municipalities = await db.municipalities.OrderBy(x => x.name).ToListAsync();
            groups = await db.groups.OrderBy(x => x.id).ToListAsync();
            _adverts_return advertsList = new _adverts_return(db);
            adverts = advertsList.principal_adverts(null, null);
            if (Request.Cookies["fz_idx"] == null)
                {
                    alerts_list = db.alerts.Where(x => x.page == "index" && x.status == 1).ToList();
                }
        }
        public IActionResult OnGetAnuncios(string type, string location)
        {
            _adverts_return advertsList = new _adverts_return(db);
            adverts = advertsList.principal_adverts(type, location);
            return new JsonResult(adverts);
        }
        public IActionResult OnGetSugestoes(string pesquisa, string tipo, string localizacao, string categoria, string sub_categoria)
        {
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  where x.status == 1
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      image_filename = y.filename,
                                                      groupName = x.groupName
                                                  };
            adverts = filter_adverts.ToList();
            if (!string.IsNullOrEmpty(pesquisa) && adverts.Count() > 0)
            {
                var words = pesquisa.Split(" ");
                foreach (var word in words.Where(x => x.Length > 2))
                {
                    adverts = adverts.Where(x => x.title.ToLower().Contains(word.ToLower())).ToList();
                }
            }
            List<string> titles = new List<string>();
            foreach (var item in adverts)
            {
                if (!titles.Contains(item.title))
                {
                    titles.Add(item.title);
                }
            }
            for (int i = 0; i < titles.Count(); i++)
            {
                while (adverts.Where(x => x.title == titles[i]).Count() > 1)
                {
                    var title = adverts.FirstOrDefault(x => x.title == titles[i]);
                    adverts.Remove(title);
                }
            }
            adverts = adverts.Take(5).OrderBy(x => x.title).ToList();
            return new JsonResult(adverts);
        }
        public IActionResult OnGetView(int anuncio, string pagina)
        {
            views views = new views(db);
            views.Register(anuncio, pagina);
            return null;
        }
    }
}
