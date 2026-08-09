using Microsoft.ML;
using System.IO;

namespace F1Predictor.ML
{
    public class ModelTrainer
    {
        private static string _modelPath = Path.Combine(Environment.CurrentDirectory, "F1Model.zip");

        public void Train(string resultsPath, string racesPath)
        {
            var mlContext = new MLContext(seed: 0);

            Console.WriteLine("1. Об'єднання даних (Results + Races)...");
            var trainingDataList = DataProcessor.LoadAndJoinData(resultsPath, racesPath);

            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingDataList);

            Console.WriteLine("2. Побудова Pipeline...");
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PositionOrder")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DriverEncoded", "DriverId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TeamEncoded", "ConstructorId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("CircuitEncoded", "CircuitId"))
                // Застосовуємо OneHotEncoding для дискретних категорій (шини та тип траси)
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TyreEncoded", nameof(RaceData.TyreType)))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("CircuitTypeEncoded", nameof(RaceData.CircuitType)))

                // Об'єднуємо всі закодовані та числові значення у підсумковий вектор Features
                .Append(mlContext.Transforms.Concatenate("Features", 
                    "DriverEncoded", 
                    "TeamEncoded", 
                    "CircuitEncoded", 
                    "TyreEncoded", 
                    "CircuitTypeEncoded", 
                    "Grid", 
                    nameof(RaceData.TrackTemperature)))
                
                .Append(mlContext.Regression.Trainers.FastTree());

            Console.WriteLine("3. Тренування моделі...");
            var model = pipeline.Fit(dataView);

            mlContext.Model.Save(model, dataView.Schema, _modelPath);
            Console.WriteLine($"Модель збережено: {_modelPath}");
        }
    }
}