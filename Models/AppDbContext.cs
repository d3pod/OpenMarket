using Microsoft.EntityFrameworkCore;
namespace openmarket.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) :base(options){}
        public DbSet <accounts_ratings> accounts_ratings {get; set;}
        public DbSet <accounts> accounts {get; set;}
        public DbSet <adverts> adverts {get; set;}
        public DbSet <attributes> attributes {get; set;}
        public DbSet <categories_attributes> categories_attributes {get; set;}
        public DbSet <categories> categories {get; set;}
        public DbSet <cities> cities {get; set;}
        public DbSet <conversations> conversations {get; set;}
        public DbSet <countries> countries {get; set;}
        public DbSet <groups> groups {get; set;}
        public DbSet <images> images {get; set;}
        public DbSet <login_logs> login_logs {get; set;}
        public DbSet <marks> marks {get; set;}
        public DbSet <models> models {get; set;}
        public DbSet <municipalities> municipalities {get; set;}
        public DbSet <search_register> search_register {get; set;}
        public DbSet <tables> tables {get; set;}
        public DbSet <users> users {get; set;}
        public DbSet <views> views {get; set;}
        public DbSet <favorites> favorites {get; set;}
        public DbSet <news> news {get; set;}
        public DbSet <month> month {get; set;}
        public DbSet <ads_plans> ads_plans {get; set;}
        public DbSet <wallet_movs> wallet_movs {get; set;}
        public DbSet <wallet_movs_origin> wallet_movs_origin {get; set;}
        public DbSet <ads_movs> ads_movs {get; set;}
        public DbSet <alerts> alerts {get; set;}
    }
}