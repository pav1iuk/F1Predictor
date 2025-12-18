namespace F1Predictor.Core
{
    public class DriverStats
    {
        public string DriverName { get; set; }
        public int TotalRaces { get; set; }
        public int Wins { get; set; }      // 1 місце
        public int Podiums { get; set; }   // Топ-3
        public int Poles { get; set; }     // Старт з 1 місця (Grid = 1)
        public double AvgPosition { get; set; } // Середнє місце на фініші
    }
}