using Microsoft.EntityFrameworkCore;
using F1Predictor.Core;

namespace F1Predictor.Data
{
    public class AppDbContext : DbContext
    {
        // Це наша таблиця в базі
        public DbSet<PredictionHistory> Predictions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Вказуємо, що це файл SQLite і він лежить поруч з програмою
            optionsBuilder.UseSqlite("Data Source=history.db");
        }
    }
}