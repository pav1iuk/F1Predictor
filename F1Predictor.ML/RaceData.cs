using Microsoft.ML.Data;

namespace F1Predictor.ML
{
    public class RaceData
    {
        public float DriverId { get; set; }
        public float ConstructorId { get; set; }
        public float Grid { get; set; }
        public float CircuitId { get; set; } // <--- НОВЕ ПОЛЕ!

        // Це те, що ми вгадуємо (Label)
        public float PositionOrder { get; set; }
    }

    public class RacePrediction
    {
        [ColumnName("Score")]
        public float Position { get; set; }
    }
}