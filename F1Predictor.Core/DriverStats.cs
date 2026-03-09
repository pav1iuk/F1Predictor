namespace F1Predictor.Core
{
    public class DriverStats
    {
        public string DriverName { get; set; }
        public int TotalRaces { get; set; }
        public int Wins { get; set; }      
        public int Podiums { get; set; }  
        public int Poles { get; set; }   
        public double AvgPosition { get; set; }
    }
}