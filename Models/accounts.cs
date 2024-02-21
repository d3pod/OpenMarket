using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
namespace openmarket.Models
{
    public class accounts
    {
        public int id { get; set; }
        public string type { get; set; }
        public string username { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string password { get; set; }
        public DateTime register { get; set; }
        public DateTime birthday { get; set; }
        public byte[] picture { get; set; }
        public string municipality { get; set; }
        public string locality { get; set; }
        public string city { get; set; }
        public int country { get; set; }
        public long facebook_id { get; set; }
        public int status { get; set; }
        public bool newsletter { get; set; }
        public int wallet { get; set; }
        public string id_share { get; set; }
    }
    public class accounts_functions
    {
        public AppDbContext db;
        public accounts_functions(AppDbContext _db)
        {
            db = _db;
        }
        public string linkShare(int userID)
        {
            string url = string.Empty;
            var user = db.accounts.First(x => x.id == userID);
            string userCode = user.id_share;
            if (!string.IsNullOrEmpty(userCode))
            {
                url = userCode;
            }
            else
            {
                Random rdm = new Random();
                var characters = "ABCDEFGHIJKLMNOPQRSTUVXZ1234567890";
                var id_share = new char[12];
                for (int i = 0; i < id_share.Length; i++)
                {
                    id_share[i] = characters[rdm.Next(characters.Length)];
                }
                user.id_share = new string(id_share);
                db.Update(user);
                db.SaveChanges();
                url = new string(id_share);
            }
            return url;
        }
    }
}