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
    public class anunciosModel : PageModel
    {
        public AppDbContext db { get; set; }
        public anunciosModel(AppDbContext _db)
        {
            db = _db;
        }
        [BindProperty(SupportsGet =true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        public string country { get; set; }
        [BindProperty(SupportsGet = true)] public string location_name { get; set; }
        [BindProperty(SupportsGet = true)] public string location_type { get; set; }
        [BindProperty(SupportsGet = true)] public string city { get; set; }
        [BindProperty(SupportsGet = true)] public string municipality { get; set; }
        [BindProperty(SupportsGet = true)] public string text { get; set; }
        [BindProperty(SupportsGet = true)] public string group_name { get; set; }
        [BindProperty(SupportsGet = true)] public string category_name { get; set; }

        [BindProperty(SupportsGet = true)] public string searchText { get; set; }
        public IList<cities> cities;
        public IList<municipalities> municipalities;
        public IList<categories> categories;
        public IList<groups> groups;
        public IList<_adverts> adverts;
        [BindProperty(SupportsGet = true)] public int TotalAdverts { get; set; }
        [BindProperty(SupportsGet = true)] public int currentpage { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public int TotalPages => (int)Math.Ceiling(decimal.Divide(TotalAdverts, PageSize));
        public IList<alerts> alerts_list;
        public void OnGet()
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
            cities = db.cities.OrderBy(x => x.name).ToList();
            municipalities = db.municipalities.OrderBy(x => x.name).ToList();

            country = Request.Query["pais"];
            city = Request.Query["provincia"];
            municipality = Request.Query["municipio"];
            text = Request.Query["pesquisa"];
            searchText = text;
            group_name = Request.Query["categoria"];
            category_name = Request.Query["sub_categoria"];

            int page = Convert.ToInt32(Request.Query["pagina"]);
            if (page > 0)
            {
                currentpage = page;
            }
            
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join j in db.categories on x.category equals j.name
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  where x.status == 1
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      date = x.date,
                                                      municipality = x.municipality,
                                                      city = x.city,
                                                      country = w.name,
                                                      date_month = x.date.ToString("MMM"),
                                                      image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                                      category = x.category,
                                                      groupName = x.groupName,
                                                      type = x.type,
                                                      indexed = j.indexed,
                                                      price_min = x.price_min,
                                                      price_max = x.price_max
                                                  };
            adverts = filter_adverts.ToList();

            if (!string.IsNullOrEmpty(category_name))
            {
                var group_id = db.categories.Where(x => x.name == category_name).Select(x => x.group_id).FirstOrDefault();
                group_name = db.groups.Where(x => x.id == group_id).Select(x => x.name).FirstOrDefault();
                categories = db.categories.Where(x => x.group_id == group_id).ToList();
                if (adverts.Count() > 0)
                {
                    adverts = adverts.Where(x => x.category == category_name).ToList();
                }
            }
            else if (!string.IsNullOrEmpty(group_name))
            {
                var group_id = db.groups.Where(x => x.name == group_name).Select(x => x.id).FirstOrDefault();
                categories = db.categories.Where(x => x.group_id == group_id).ToList();
                if (adverts.Count() > 0)
                {
                    adverts = adverts.Where(x => x.groupName == group_name).ToList();
                }
            }
            else
            {
                groups = db.groups.ToList();
            }

            if (!string.IsNullOrEmpty(text) && adverts.Count() > 0)
            {
                var words = text.Trim().Split(" ");

                foreach (var word in words.Where(x => x.Length > 2))
                {
                    adverts = adverts.Where(x => x.title.ToLower().Contains(word.ToLower())).ToList();
                }
                if (SessionUser != 0)
                {
                    var searchRegister = new search_register
                    {
                        text = text.Trim(),
                        date = DateTime.Now,
                        account = SessionUser
                    };
                    db.search_register.Add(searchRegister);
                    db.SaveChanges();
                }
                else
                {
                    var searchRegister = new search_register
                    {
                        text = text.Trim(),
                        date = DateTime.Now
                    };
                    db.search_register.Add(searchRegister);
                    db.SaveChanges();
                }
            }
            if (!string.IsNullOrEmpty(municipality))
            {
                adverts = adverts.Where(x => x.municipality == municipality).ToList();
                location_name = municipality;
                location_type = "municipio";
            }
            else if (!string.IsNullOrEmpty(city))
            {
                adverts = adverts.Where(x => x.city == city).ToList();
                location_name = city;
                location_type = "provincia";
            }
            else if (!string.IsNullOrEmpty(country))
            {
                adverts = adverts.Where(x => x.country == country).ToList();
                location_name = country;
                location_type = "pais";
            }

            List<int> listID = new List<int>();
            foreach (var item in adverts)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (adverts.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var id = adverts.FirstOrDefault(x => x.id == listID[i]);
                    adverts.Remove(id);
                }
            }
            TotalAdverts = adverts.Count();
            adverts = adverts.OrderByDescending(x => x.id).Skip((currentpage - 1) * PageSize)
                                                    .Take(PageSize).ToList();
            if (Request.Cookies["fz_advs"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "anuncios" && x.status == 1).ToList();
            }
        }
        public IActionResult OnGetAnuncios(string pagina, string pesquisa, string pais, string provincia, string municipio, string estado, int min_preco, int max_preco, string ordenar, string categoria, string sub_categoria, string type, string negotiable, string user)
        {
            string sessionID = HttpContext.Session.GetString("userID");
            SessionUser = Convert.ToInt32(sessionID);
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join j in db.categories on x.category equals j.name
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  where x.status == 1
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      date = x.date,
                                                      municipality = x.municipality,
                                                      city = x.city,
                                                      country = w.name,
                                                      date_month = x.date.ToString("MMM"),
                                                      image_filename = (from r in db.images where r.product == x.id select r).First().filename,
                                                      category = x.category,
                                                      groupName = x.groupName,
                                                      condition = x.condition,
                                                      negotiable = x.negotiable,
                                                      type = x.type,
                                                      indexed = j.indexed,
                                                      price_min = x.price_min,
                                                      price_max = x.price_max,
                                                      vendor = x.vendor
                                                  };
            adverts = filter_adverts.ToList();

            if (!string.IsNullOrEmpty(pagina))
            {
                currentpage = Convert.ToInt32(pagina);
            }

            if (!string.IsNullOrEmpty(categoria))
            {
                adverts = adverts.Where(x => x.groupName == categoria).ToList();
            }
            else if (!string.IsNullOrEmpty(sub_categoria))
            {
                adverts = adverts.Where(x => x.category == sub_categoria).ToList();
            }

            if (!string.IsNullOrEmpty(pesquisa) && adverts.Count() > 0)
            {
                var words = pesquisa.Trim().Split(" ");
                foreach (var word in words.Where(x => x.Length > 2))
                {
                    adverts = adverts.Where(x => x.title.ToLower().Contains(word.ToLower())).ToList();
                }
                if (SessionUser != 0)
                {
                    var searchRegister = new search_register
                    {
                        text = pesquisa.Trim(),
                        date = DateTime.Now,
                        account = SessionUser
                    };
                    db.search_register.Add(searchRegister);
                    db.SaveChanges();
                }
                else
                {
                    var searchRegister = new search_register
                    {
                        text = pesquisa.Trim(),
                        date = DateTime.Now
                    };
                    db.search_register.Add(searchRegister);
                    db.SaveChanges();
                }
            }

            List<int> listID = new List<int>();
            foreach (var item in adverts)
            {
                if (!listID.Contains(item.id))
                {
                    listID.Add(item.id);
                }
            }
            for (int i = 0; i < listID.Count(); i++)
            {
                while (adverts.Where(x => x.id == listID[i]).Count() > 1)
                {
                    var id = adverts.FirstOrDefault(x => x.id == listID[i]);
                    adverts.Remove(id);
                }
            }
            if (adverts.Count() > 0)
            {
                if (!string.IsNullOrEmpty(negotiable))
                {
                    adverts = adverts.Where(x => x.negotiable == Convert.ToInt32(negotiable)).ToList();
                }
                if (!string.IsNullOrEmpty(user))
                {
                    adverts = adverts.Where(x => x.vendor == user).ToList();
                }
                if (!string.IsNullOrEmpty(provincia))
                {
                    adverts = adverts.Where(x => x.city == provincia).ToList();
                }
                else if (!string.IsNullOrEmpty(municipio))
                {
                    adverts = adverts.Where(x => x.municipality == municipio).ToList();
                }
                else
                {
                    adverts = adverts.Where(x => x.country == "Portugal").ToList();
                }
                switch (estado)
                {
                    case "Usados":
                        adverts = adverts.Where(x => x.condition == 0).ToList();
                        break;
                    case "Novos":
                        adverts = adverts.Where(x => x.condition == 1).ToList();
                        break;
                    case null:
                        adverts = adverts.ToList();
                        break;
                }
                if (min_preco > 0)
                {
                    adverts = adverts.Where(x => x.price >= min_preco).ToList();
                }
                if (max_preco > 0)
                {
                    adverts = adverts.Where(x => x.price <= max_preco).ToList();
                }
                switch (ordenar)
                {
                    case "preco-baixo":
                        adverts = adverts.OrderBy(x => x.price).ToList();
                        break;

                    case "preco-alto":
                        adverts = adverts.OrderByDescending(x => x.price).ToList();
                        break;

                    case null:
                        adverts = adverts.OrderByDescending(x => x.id).ToList();
                        break;

                }
            }
            adverts = adverts.ToList();
            return new JsonResult(adverts);
        }
        public IActionResult OnGetSugestoes(string pesquisa, string tipo, string localizacao, string categoria, string sub_categoria)
        {
            IQueryable<_adverts> filter_adverts = from x in db.adverts
                                                  join y in db.images on x.id equals y.product into image
                                                  from y in image.DefaultIfEmpty()
                                                  join q in db.cities on x.city equals q.name
                                                  join w in db.countries on q.country equals w.id
                                                  where x.status == 1
                                                  select new _adverts
                                                  {
                                                      id = x.id,
                                                      title = x.title,
                                                      price = x.price,
                                                      municipality = x.municipality,
                                                      city = x.city,
                                                      country = w.name,
                                                      image_filename = y.filename,
                                                      groupName = x.groupName
                                                  };
            adverts = filter_adverts.ToList();
            if (!string.IsNullOrEmpty(categoria))
            {
                adverts = adverts.Where(x => x.groupName == categoria).ToList();
            }
            else if (!string.IsNullOrEmpty(sub_categoria))
            {
                adverts = adverts.Where(x => x.category == sub_categoria).ToList();
            }
            if (!string.IsNullOrEmpty(pesquisa) && adverts.Count() > 0)
            {
                var words = pesquisa.Split(" ");
                foreach (var word in words.Where(x => x.Length > 2))
                {
                    adverts = adverts.Where(x => x.title.ToLower().Contains(word.ToLower())).ToList();
                }
            }
            if (adverts.Count() > 0)
            {
                if (string.IsNullOrEmpty(localizacao))
                {
                    localizacao = "Portugal";
                }
                switch (tipo)
                {
                    case "pais":
                        adverts = adverts.Where(x => x.country == localizacao).ToList();
                        break;

                    case "cidade":
                        adverts = adverts.Where(x => x.city == localizacao).ToList();
                        break;

                    case "municipio":
                        adverts = adverts.Where(x => x.municipality == localizacao).ToList();
                        break;
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
    }
}
