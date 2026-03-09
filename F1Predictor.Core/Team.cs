namespace F1Predictor.Core
{
    public class Team
    {
        public float ConstructorId { get; set; }
        public string Name { get; set; }

        public override string ToString() => Name;
    }
}