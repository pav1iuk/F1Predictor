using Microsoft.ML.Data;

namespace F1Predictor.ML
{
    public class RaceData
    {
        [LoadColumn(1)] // RaceId (ідентифікатор конкретної гонки)
        public float RaceId { get; set; }   
        // Хто їде (Індекс 2)
        [LoadColumn(2)] 
        public float DriverId { get; set; } 

        // На чому їде (Індекс 3)
        [LoadColumn(3)] 
        public float ConstructorId { get; set; }

        // Стартова позиція (Індекс 5)
        [LoadColumn(5)] 
        public float Grid { get; set; }

        // Фінішна позиція (Індекс 8) - це те, що ми вчимося передбачати (Label)
        [LoadColumn(8)] 
        public float PositionOrder { get; set; }
    }

    public class RacePrediction
    {
        [ColumnName("Score")]
        public float Position { get; set; }
    }
}