using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.IO;
using System.Net.Mail;

namespace openmarket.Models
{
    public class wallet
    {
        public int id { get; set; }
        public int user { get; set; }
        public int value { get; set; }
        public string description { get; set; }
        public int origin { get; set; }
        public DateTime date { get; set; }
        public string type { get; set; }
        public AppDbContext db;
        public string subject { get; set; }
        public string body { get; set; }
        StreamReader reader;
        public wallet(AppDbContext _db)
        {
            db = _db;
        }
        public int walletValue(int user)
        {
            int value = db.accounts.Where(x => x.id == user).Select(x => x.wallet).Single();
            return value;
        }
        public void walletCalculate(int user)
        {
            int value = 0;
            var walletMovs = from x in db.wallet_movs
                             join y in db.wallet_movs_origin on x.origin equals y.id
                             where x.user == user
                             select new wallet(db)
                             {
                                 id = x.id,
                                 user = x.user,
                                 value = x.value,
                                 description = x.description,
                                 origin = x.origin,
                                 date = x.date,
                                 type = y.type
                             };

            foreach (var item in walletMovs)
            {
                if (item.type == "Entrada")
                {
                    value = value + item.value;
                }
                else if (item.type == "Saida")
                {
                    value = value - item.value;
                }
            }

            var updateAccount = db.accounts.Where(x => x.id == user).First();
            if (value > 0)
            {
                updateAccount.wallet = value;
            }
            else
            {
                updateAccount.wallet = 0;
            }
            db.Update(updateAccount);
            db.SaveChanges();
        }
        public void insertPlan(int plan_id, int user)
        {
            var plan = db.ads_plans.Where(x => x.id == plan_id).First();
            var newMov = new wallet_movs();

            newMov = new wallet_movs()
            {
                user = user,
                value = plan.price,
                description = plan.name,
                origin = 12,
                date = DateTime.Now
            };
            db.wallet_movs.Add(newMov);
            db.SaveChanges();
            walletCalculate(user);
            sendMail(user, plan.price.ToString(), "Saida");
        }
        public void insertMov(int origin, int user, int value, bool sendEmail)
        {
            var account = db.accounts.First(x => x.id == user);
            var originMov = db.wallet_movs_origin.First(x => x.id == origin);
            var newMov = new wallet_movs();
            if (value > 0)
            {
                newMov = new wallet_movs()
                {
                    user = user,
                    value = value,
                    description = originMov.name,
                    origin = originMov.id,
                    date = DateTime.Now
                };
            }
            else
            {
                newMov = new wallet_movs()
                {
                    user = user,
                    value = originMov.value,
                    description = originMov.name,
                    origin = originMov.id,
                    date = DateTime.Now
                };
            }
            db.wallet_movs.Add(newMov);
            db.SaveChanges();
            walletCalculate(user);
            if (sendEmail)
            {
                if (!string.IsNullOrEmpty(account.email))
                {
                    if (value > 0)
                    {
                        sendMail(user, value.ToString(), originMov.type);
                    }
                    else
                    {
                        sendMail(user, originMov.value.ToString(), originMov.type);
                    }
                }
            }
        }
        public void sendMail(int userID, string value, string type)
        {
            var user = db.accounts.First(x => x.id == userID);

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential("openmarket@gmail.com", "YourPassword");
            smtp.EnableSsl = true;
            MailAddress from = new MailAddress("openmarket@gmail.com", "OpenMarket");
            MailAddress to = new MailAddress(user.email);
            MailMessage mailMessage = new MailMessage(from, to);
            mailMessage.IsBodyHtml = true;

            if (type == "Entrada")
            {
                mailMessage.Subject = "Recebeu pontos na sua conta OpenMarket";
                reader = new StreamReader("wwwroot/templates/emails/added-points.htm");
                body = reader.ReadToEnd();
                body = body.Replace("{username}", user.username);
                body = body.Replace("{value}", value);
            }
            else
            {
                mailMessage.Subject = "Utilizou pontos da sua conta OpenMarket";
                reader = new StreamReader("wwwroot/templates/emails/used-points.htm");
                body = reader.ReadToEnd();
                body = body.Replace("{username}", user.username);
                body = body.Replace("{value}", value);
            }

            mailMessage.Body = body;
            smtp.Send(mailMessage);
        }
    }
}