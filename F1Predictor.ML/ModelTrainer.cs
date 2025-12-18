using Microsoft.ML;
using System.IO;

namespace F1Predictor.ML
{
    public class ModelTrainer
    {
        private static string _modelPath = Path.Combine(Environment.CurrentDirectory, "F1Model.zip");

        public void Train(string resultsPath, string racesPath) // Тепер приймаємо два шляхи
        {
            var mlContext = new MLContext(seed: 0);

            Console.WriteLine("1. Об'єднання даних (Results + Races)...");
            var trainingDataList = DataProcessor.LoadAndJoinData(resultsPath, racesPath);

            // Завантажуємо дані в пам'ять ML.NET прямо зі списку (LoadFromEnumerable)
            IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingDataList);

            Console.WriteLine("2. Побудова Pipeline...");
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PositionOrder")
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("DriverEncoded", "DriverId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("TeamEncoded", "ConstructorId"))
                
                // Додаємо кодування для ТРАСИ
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("CircuitEncoded", "CircuitId")) 

                // Додаємо CircuitEncoded у фінальний вектор
                .Append(mlContext.Transforms.Concatenate("Features", "DriverEncoded", "TeamEncoded", "CircuitEncoded", "Grid"))
                
                .Append(mlContext.Regression.Trainers.FastTree());

            Console.WriteLine("3. Тренування моделі...");
            var model = pipeline.Fit(dataView);

            mlContext.Model.Save(model, dataView.Schema, _modelPath);
            Console.WriteLine($"Модель збережено: {_modelPath}");
        }
    }
}