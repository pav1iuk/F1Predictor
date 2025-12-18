namespace F1Predictor.Core
{
    public class Driver
    {
        public float DriverId { get; set; }
        public string FullName { get; set; } // Тут буде "Lewis Hamilton"

        // Перевизначаємо ToString, щоб у списку відображалось ім'я, а не назва класу
        public override string ToString() => FullName; 
    }
}