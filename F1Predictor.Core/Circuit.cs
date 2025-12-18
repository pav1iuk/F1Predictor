namespace F1Predictor.Core
{
    public class Circuit
    {
        public float CircuitId { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }

        public string DisplayName => $"{Name} ({Location})"; // Для красивого списку
    }
}