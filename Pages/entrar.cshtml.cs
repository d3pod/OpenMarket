using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.IO;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Net.Mail;
using openmarket.Models;

namespace MyApp.Namespace
{
    public class entrarModel : PageModel
    {
        public AppDbContext db;
        [BindProperty(SupportsGet = true)] public int SessionUser { get; set; } = 0;
        [BindProperty] public byte[] UserPicture { get; set; }
        [BindProperty] public int Cookies { get; set; }

        public IHttpContextAccessor accessor { get; set; }
        public entrarModel(AppDbContext _db, IHttpContextAccessor _accessor)
        {
            db = _db;
            accessor = _accessor;
        }
        [BindProperty] public string _Msg { get; set; }
        [BindProperty] public accounts accounts { get; set; }
        StreamReader reader;
        [BindProperty(SupportsGet = true)] public string subject { get; set; }
        [BindProperty(SupportsGet = true)] public string body { get; set; }
        [BindProperty(SupportsGet = true)] public string backUrl { get; set; }
        [BindProperty(SupportsGet = true)] public string convite { get; set; }
        [BindProperty(SupportsGet = true)] public bool register { get; set; } = false;
        public IList<alerts> alerts_list;
        valueFormat formatter = new valueFormat();

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
                else
                {
                    return Redirect("~/minha-conta");
                }
            }
            else if (!string.IsNullOrEmpty(Request.Cookies["fzid"]))
            {
                string userCookie = Request.Cookies["fzid"];
                valueFormat formatter = new valueFormat();
                int id = Convert.ToInt32(formatter.decrypter(userCookie));
                SessionUser = id;
                return Redirect("~/minha-conta");
            }
            if (!string.IsNullOrEmpty(Request.Query["backUrl"]))
            {
                if (!string.IsNullOrEmpty(Request.Query["name"]))
                {
                    backUrl = Request.Query["backUrl"] + "?" + formatter.urlFormat(Request.Query["name"]) + "&id=" + Request.Query["id"];
                }
                else if (!string.IsNullOrEmpty(Request.Query["id"]))
                {
                    backUrl = Request.Query["backUrl"] + "?id=" + Request.Query["id"];
                }
                else
                {
                    backUrl = Request.Query["backUrl"];
                }
            }
            if (!string.IsNullOrEmpty(Request.Query["convite"]))
            {
                convite = Request.Query["convite"];
                register = true;
            }
            if (Request.QueryString.ToString().Contains("?registar"))
            {
                register = true;
            }
            if (Request.Cookies["fz_lg"] == null)
            {
                alerts_list = db.alerts.Where(x => x.page == "entrar" && x.status == 1).ToList();
            }
            return null;
        }
        public IActionResult OnPostLogin(string backUrl)
        {
            if (!string.IsNullOrEmpty(accounts.email) && db.accounts.Where(x => x.email == accounts.email.Trim()).Count() > 0)
            {
                var password = db.accounts.Where(x => x.email == accounts.email.Trim()).Select(x => x.password).FirstOrDefault().ToString();
                if (!string.IsNullOrEmpty(accounts.password) && accounts.password.Trim() == password)
                {
                    string userid = db.accounts.Where(x => x.email == accounts.email.Trim()).Select(x => x.id).First().ToString();
                    int accountStatus = db.accounts.Where(x => x.id == Convert.ToInt32(userid)).Select(x => x.status).Single();
                    if (accountStatus == 11)
                    {
                        return Redirect("entrar?Msg=conta-bloqueada");
                    }
                    else
                    {
                        HttpContext.Session.SetString("userID", userid);
                        var cookieOptions = new CookieOptions
                        {
                            Expires = DateTime.Now.AddDays(30)
                        };
                        Response.Cookies.Append("fzid", formatter.encrypter(userid), cookieOptions);
                        //Registo de sessão
                        newLog(int.Parse(userid));
                        if (!string.IsNullOrEmpty(backUrl))
                        {
                            return Redirect("~/" + backUrl);
                        }
                        else
                        {
                            return Redirect("~/entrar");
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(backUrl))
                {
                    return Redirect("entrar?backUrl=" + backUrl + "&Msg=login-incorrecto");
                }
                else
                {
                    return Redirect("entrar?Msg=login-incorrecto");
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(backUrl))
                {
                    return Redirect("entrar?backUrl=" + backUrl + "&Msg=nao-encontrado");
                }
                else
                {
                    return Redirect("entrar?Msg=nao-encontrado");
                }
            }
        }
        public IActionResult OnPostRegister(string invite, string backUrl)
        {
            wallet wallet = new wallet(db);
            int email_count = db.accounts.Where(x => x.email == accounts.email.Trim()).Count();
            int user_count = db.accounts.Where(x => x.username == accounts.username.Trim()).Count();
            if (email_count > 0)
            {
                return Redirect("~/entrar?registar&Msg=email-registado");
            }
            else if (user_count > 0)
            {
                return Redirect("~/entrar?registar&Msg=nome-utilizador-registado");
            }
            else
            {
                var username = accounts.username.Replace(" ", "");
                var account = new accounts
                {
                    username = formatter.stringFormat(formatter.removeEmojis((accounts.username.Trim()))),
                    email = formatter.stringFormat(formatter.removeEmojis(accounts.email.Trim())),
                    password = accounts.password.Trim(),
                    newsletter = accounts.newsletter,
                    register = DateTime.Now,
                    status = 1
                };
                db.accounts.Add(account);
                db.SaveChanges();
                string userid = db.accounts.Where(x => x.username == account.username).Select(x => x.id).First().ToString();
                HttpContext.Session.SetString("userID", userid);
                var cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(30)
                };
                Response.Cookies.Append("fzid", formatter.encrypter(userid), cookieOptions);
                wallet.insertMov(6, Convert.ToInt32(userid), 0, true);
                //Email de registo
                SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
                smtp.EnableSsl = true;
                MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
                MailAddress to = new MailAddress(accounts.email);
                MailMessage mailMessage = new MailMessage(from, to);
                mailMessage.Subject = "Registo de conta";
                mailMessage.IsBodyHtml = true;
                reader = new StreamReader("wwwroot/templates/emails/create-account.htm");
                body = reader.ReadToEnd();
                body = body.Replace("{username}", username);
                mailMessage.Body = body;
                smtp.Send(mailMessage);
                //Mensagem para o utilizador
                string message = "Bem-vindo ao OpenMarket, o teu site de classificados onde podes vender ou comprar praticamente tudo. Caso tenhas alguma questão poderás usar esta conversa para comunicar com a nossa equipa. Obrigado pela preferência, a equipa do OpenMarket!";
                var newPost = new conversations
                {
                    sender = 1,
                    receiver = Convert.ToInt32(userid),
                    message = message,
                    date = DateTime.Now,
                    status = "Por ler"
                };
                db.conversations.Add(newPost);
                db.SaveChanges();
                //Registo de sessão
                newLog(int.Parse(userid));
                if (!string.IsNullOrEmpty(invite))
                {
                    string userCode = invite;
                    int userLinkId = db.accounts.Where(x => x.id_share == userCode).Select(x => x.id).SingleOrDefault();
                    if (userLinkId > 0)
                    {
                        wallet.insertMov(5, userLinkId, 0, true);
                        wallet.insertMov(4, Convert.ToInt32(userid), 0, true);
                    }
                    return Redirect("~/minha-conta");
                }
                else if (!string.IsNullOrEmpty(backUrl))
                {
                    return Redirect("~/" + backUrl);
                }
                else
                {
                    return RedirectToPage("minha-conta");
                }
            }
        }
        public void newLog(int userID)
        {
            string ip = Request.HttpContext.Connection.RemoteIpAddress.ToString();
            var log = new login_logs
            {
                account = userID,
                ip = ip,
                date = DateTime.Now
            };
            db.login_logs.Add(log);
            db.SaveChanges();
        }
        public IActionResult OnPostFacebook(string backUrl)
        {
            var sessionCookie = HttpContext.Request.Cookies["uinef"];
            string email;
            string[] url = sessionCookie.Split("/");
            if (url[2].ToString() == "undefined" || url[2].ToString() == null)
            {
                email = "";
            }
            else
            {
                email = url[2];
            }
            int checkUser = db.accounts.Where(x => x.facebook_id == Convert.ToInt64(url[0])).Select(x => x.id).Count();
            var rdm = new Random();
            string username = url[1].Trim().Replace(" ", "");
            if (checkUser == 0)
            {
                //username
                while (db.accounts.Where(x => x.username == username).Select(x => x.id).Count() > 0)
                {
                    var numbers = "1234567890";
                    var stringNumbers = new char[2];
                    rdm = new Random();
                    for (int i = 0; i < stringNumbers.Length; i++)
                    {
                        stringNumbers[i] = numbers[rdm.Next(numbers.Length)];
                    }
                    string usernameID = new string(stringNumbers);
                    username = username + usernameID;
                }
                var newAccount = new accounts
                {
                    username = username,
                    facebook_id = Convert.ToInt64(url[0]),
                    email = email,
                    password = "",
                    register = DateTime.Now,
                    country = 0
                };
                db.accounts.Add(newAccount);
                db.SaveChanges();
                //Mensagem para o utilizador
                string userid = db.accounts.Where(x => x.facebook_id == Convert.ToInt64(url[0])).Select(x => x.id).First().ToString();
                wallet wallet = new wallet(db);
                wallet.insertMov(6, Convert.ToInt32(userid), 0, false);
                string message = "Bem-vindo ao OpenMarket, o teu site de classificados onde podes vender ou comprar praticamente tudo. Caso tenhas alguma questão poderás usar esta conversa para comunicar com a nossa equipa. Obrigado pela preferência, a equipa do OpenMarket!";
                var newPost = new conversations
                {
                    sender = 1,
                    receiver = Convert.ToInt32(userid),
                    message = message,
                    date = DateTime.Now,
                    status = "Por ler"
                };
                db.conversations.Add(newPost);
                db.SaveChanges();
            }

            int account = db.accounts.Where(x => x.facebook_id == Convert.ToInt64(url[0])).Select(x => x.id).First();
            HttpContext.Session.SetString("userID", Convert.ToString(account));
            var cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(30)
            };
            Response.Cookies.Append("fzid", formatter.encrypter(Convert.ToString(account)), cookieOptions);
            newLog(account);
            var accountStatus = db.accounts.Where(x => x.id == account).Select(x => x.status).Single();
            if (accountStatus == 11)
            {
                return Redirect("entrar?Msg=conta-bloqueada");
            }
            else if (!string.IsNullOrEmpty(backUrl))
            {
                return Redirect("~/" + backUrl);
            }
            else
            {
                return RedirectToPage("minha-conta");
            }
        }
    }
}
