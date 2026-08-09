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

            if (!File.Exists(_modelPath))
            {
                throw new FileNotFoundException($"Файл моделі не знайдено в {_modelPath}. Спочатку запустіть тренування!");
            }

            using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                _model = _mlContext.Model.Load(stream, out var modelInputSchema);
            }

            _predictionEngine = _mlContext.Model.CreatePredictionEngine<RaceData, RacePrediction>(_model);
        }

        public float Predict(
            float driverId, 
            float teamId, 
            float gridPosition, 
            float circuitId, 
            float tyreType = 1.0f, 
            float trackTemp = 30.0f, 
            float circuitType = 2.0f)
        {
            var inputData = new RaceData
            {
                DriverId = driverId,
                ConstructorId = teamId,
                Grid = gridPosition,
                CircuitId = circuitId,
                TyreType = tyreType,
                TrackTemperature = trackTemp,
                CircuitType = circuitType
            };

            var prediction = _predictionEngine.Predict(inputData);
            return prediction.Position;
        }
    }
}