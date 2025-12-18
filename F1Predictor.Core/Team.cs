namespace F1Predictor.Core
{
    public class Team
    {
        public float ConstructorId { get; set; }
        public string Name { get; set; } // Тут буде "Ferrari"

        public override string ToString() => Name;
    }
}