using F1Predictor.Core;

namespace F1Predictor.Data
{
    public class HistoryService
    {
        private readonly AppDbContext _context;

        public HistoryService()
        {
            _context = new AppDbContext();
            // Ця команда створить файл history.db, якщо його немає
            _context.Database.EnsureCreated();
        }

        public void AddRecord(PredictionHistory record)
        {
            _context.Predictions.Add(record);
            _context.SaveChanges();
        }

        public List<PredictionHistory> GetAll()
        {
            // Завантажуємо останні спочатку
            return _context.Predictions
                .OrderByDescending(x => x.Date)
                .ToList();
        }
        public void DeleteRecord(int id)
        {
            // Знаходимо запис за ID
            var record = _context.Predictions.FirstOrDefault(x => x.Id == id);
            
            // Якщо знайшли - видаляємо
            if (record != null)
            {
                _context.Predictions.Remove(record);
                _context.SaveChanges();
            }
        }
    }
}