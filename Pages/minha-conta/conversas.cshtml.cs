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
    public class conversasModel : PageModel
    {
        public AppDbContext db;
        [BindProperty] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }
        [BindProperty] public string _Msg { get; set; }
        [BindProperty] public int advert { get; set; }
        public IHttpContextAccessor accessor { get; set; }
        public IList<_chat> chats;
        public conversations posts { get; set; }
        public IList<alerts> alerts_list;
        public conversasModel(AppDbContext _db, IHttpContextAccessor _accessor)
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
            if (Request.Cookies["fz_cht"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "conversas" && x.status == 1).ToList();
            }
            advert = Convert.ToInt32(Request.Cookies["advert"]);

            IQueryable<_chat> filter_read = from x in db.conversations
                                            join y in db.accounts on x.sender equals y.id
                                            where x.status == "Por ler" && (x.receiver == SessionUser)
                                            select new _chat
                                            {
                                                id = x.id,
                                                receiver = x.receiver,
                                                sender = x.sender,
                                                sender_username = y.username,
                                                status = x.status,
                                                date = x.date
                                            };
            chats = filter_read.ToList();

            IQueryable<_chat> filter_readed = from x in db.conversations
                                              join y in db.accounts on x.sender equals y.id
                                              where x.status == "Lido" && (x.receiver == SessionUser)
                                              select new _chat
                                              {
                                                  id = x.id,
                                                  receiver = x.receiver,
                                                  sender = x.sender,
                                                  sender_username = y.username,
                                                  status = x.status,
                                                  date = x.date
                                              };
            IList<_chat> filter_temporary = filter_readed.ToList();

            IQueryable<_chat> filter_sended = from x in db.conversations
                                              join y in db.accounts on x.receiver equals y.id
                                              where x.status != "Concluido" && (x.sender == SessionUser)
                                              select new _chat
                                              {
                                                  id = x.id,
                                                  receiver = x.receiver,
                                                  sender = x.sender,
                                                  sender_username = y.username,
                                                  status = x.status,
                                                  date = x.date
                                              };
            IList<_chat> sended = filter_sended.ToList();

            foreach (var item in chats)
            {
                while (filter_temporary.Where(x => x.sender == item.sender).Count() > 0)
                {
                    filter_temporary.Remove(filter_temporary.Where(x => x.sender == item.sender).FirstOrDefault());
                }
            }
            chats = chats.Concat(filter_temporary).ToList();

            List<int> senders = new List<int>();
            foreach (var item in chats)
            {
                if (!senders.Contains(item.sender))
                {
                    senders.Add(item.sender);
                }
            }
            for (int i = 0; i < senders.Count(); i++)
            {
                while (chats.Where(x => x.sender == senders[i]).Count() > 1)
                {
                    var id = chats.FirstOrDefault(x => x.sender == senders[i]);
                    chats.Remove(id);
                }
            }
            chats = chats.OrderByDescending(x => x.date).ToList();
            return null;
        }
        public IActionResult OnGetConversas()
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            IQueryable<_chat> filter_read = from x in db.conversations
                                            join y in db.accounts on x.sender equals y.id
                                            where x.status == "Por ler" && (x.receiver == SessionUser)
                                            select new _chat
                                            {
                                                id = x.id,
                                                sender = x.sender,
                                                sender_username = y.username,
                                                status = x.status,
                                                date = x.date
                                            };
            chats = filter_read.ToList();

            IQueryable<_chat> filter_readed = from x in db.conversations
                                              join y in db.accounts on x.sender equals y.id
                                              where x.status == "Lido" && (x.receiver == SessionUser)
                                              select new _chat
                                              {
                                                  id = x.id,
                                                  sender = x.sender,
                                                  sender_username = y.username,
                                                  status = x.status,
                                                  date = x.date
                                              };
            IList<_chat> filter_temporary = filter_readed.ToList();

            foreach (var item in chats)
            {
                while (filter_temporary.Where(x => x.sender == item.sender).Count() > 0)
                {
                    filter_temporary.Remove(filter_temporary.Where(x => x.sender == item.sender).FirstOrDefault());
                }
            }
            chats = chats.Concat(filter_temporary).ToList();

            List<int> senders = new List<int>();
            foreach (var item in chats)
            {
                if (!senders.Contains(item.sender))
                {
                    senders.Add(item.sender);
                }
            }
            for (int i = 0; i < senders.Count(); i++)
            {
                while (chats.Where(x => x.sender == senders[i]).Count() > 1)
                {
                    var id = chats.FirstOrDefault(x => x.sender == senders[i]);
                    chats.Remove(id);
                }
            }
            chats = chats.OrderByDescending(x => x.date).ToList();
            return new JsonResult(chats);
        }
        public IActionResult OnGetMensagens(string user, string message)
        {
            SessionUser = Convert.ToInt32(HttpContext.Session.GetString("userID"));
            var update_status = db.conversations.Where(x => x.receiver == SessionUser && x.sender == Convert.ToInt32(user)).ToList();
            foreach (var item in update_status)
            {
                item.status = "Lido";
                db.Update(item);
                db.SaveChanges();
            }
            if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(user))
            {
                var new_post = new conversations
                {
                    message = message,
                    sender = SessionUser,
                    receiver = Convert.ToInt32(user),
                    date = DateTime.Now,
                    status = "Por ler"
                };
                db.conversations.Add(new_post);
                db.SaveChanges();
            }

            IQueryable<_chat> filter = from x in db.conversations
                                       join y in db.accounts on x.sender equals y.id
                                       where x.status != "Concluido" && (x.sender == Convert.ToInt32(user) || x.receiver == Convert.ToInt32(user)) && (x.sender == SessionUser || x.receiver == SessionUser)
                                       select new _chat
                                       {
                                           id = x.id,
                                           receiver = x.receiver,
                                           sender = x.sender,
                                           sender_username = y.username,
                                           message = x.message,
                                           date = x.date,
                                           date_month = x.date.ToString("MMM"),
                                           status = x.status,
                                           account = SessionUser
                                       };
            chats = filter.OrderBy(x => x.date).ToList();
            return new JsonResult(chats);
        }
    }
}
