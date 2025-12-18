using Microsoft.ML;
using System.IO;

namespace F1Predictor.ML
{
    public class ModelPredictor
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly PredictionEngine<RaceData, RacePrediction> _predictionEngine;
        private static string _modelPath = Path.Combine(Environment.CurrentDirectory, "F1Model.zip");

        public ModelPredictor()
        {
            _mlContext = new MLContext();

            // 1. Завантажуємо збережений "мозок" (.zip файл)
            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException($"Файл моделі не знайдено в {_modelPath}. Спочатку запустіть тренування!");
            }

            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _model = _mlContext.Model.Load(stream, out var modelInputSchema);
            }

            // 2. Створюємо рушій прогнозування
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<RaceData, RacePrediction>(_model);
        }

        public float Predict(float driverId, float teamId, float gridPosition, float circuitId)
        {
            // Створюємо об'єкт з даними для прогнозу
            var inputData = new RaceData
            {
                DriverId = driverId,
                ConstructorId = teamId,
                Grid = gridPosition,
                CircuitId = circuitId
            };

            // Робимо прогноз
            var prediction = _predictionEngine.Predict(inputData);

            return prediction.Position; // Повертає прогнозоване місце (наприклад, 1.45)
        }
    }
}