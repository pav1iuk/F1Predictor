namespace F1Predictor.Core
{
    public class PredictionHistory
    {
        public int Id { get; set; } // Унікальний номер запису
        public string DriverName { get; set; }
        public string TeamName { get; set; }
        public string CircuitName { get; set; }
        public int GridPosition { get; set; }
        public float PredictedPosition { get; set; }
        public DateTime Date { get; set; } // Коли зробили прогноз
    }
}