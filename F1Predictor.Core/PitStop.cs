namespace F1Predictor.Core
{
    public class PitStop
    {
        public int RaceId { get; set; }
        public int DriverId { get; set; }
        public int Stop { get; set; }
        public int Milliseconds { get; set; } // Час у мілісекундах (напр. 25000)
        
        public double Seconds => Milliseconds / 1000.0;
    }
}