using Microsoft.EntityFrameworkCore;
using F1Predictor.Core;

namespace F1Predictor.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<PredictionHistory> Predictions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=history.db");
        }
    }
}