using System;
namespace openmarket.Models
{
    public class accounts_ratings
    {
        public int id { get; set; }
        public int account_evaluator { get; set; }
        public int account_rated { get; set; }
        public int value { get; set; }
        public DateTime date { get; set; }
        public int status { get; set; }
    }
}