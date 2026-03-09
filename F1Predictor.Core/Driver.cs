namespace F1Predictor.Core
{
    public class Driver
    {
        public float DriverId { get; set; }
        public string FullName { get; set; }
        
        public override string ToString() => FullName; 
    }
}