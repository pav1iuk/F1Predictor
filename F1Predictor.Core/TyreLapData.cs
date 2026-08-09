namespace F1Predictor.Core
{
    public class TyreLapData
    {
        public int LapNumber { get; set; }
        public double LapTimePenalty { get; set; } // Втрата часу в секундах через знос
        public double TyreHealth { get; set; }    // Залишок ресурсу гуми у % (100 - 0)
    }
}