using Microsoft.ML;
using Microsoft.ML.Data;

namespace F1Predictor.ML
{
    public class ModelEvaluator
    {
        private readonly MLContext _mlContext;
        private static string _modelPath = Path.Combine(Environment.CurrentDirectory, "F1Model.zip");

        public ModelEvaluator()
        {
            _mlContext = new MLContext();
        }

        public string Evaluate(string resultsPath, string racesPath)
        {
            try
            {
                // 1. Завантажуємо реальні дані (тестовий набір)
                var data = DataProcessor.LoadAndJoinData(resultsPath, racesPath);
                IDataView dataView = _mlContext.Data.LoadFromEnumerable(data);

                // 2. Завантажуємо навчену модель
                if (!File.Exists(_modelPath)) return "Модель не знайдена!";

                ITransformer model;
                using (var stream = new FileStream(_modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    model = _mlContext.Model.Load(stream, out var schema);
                }

                // 3. Робимо прогнози для ВСІХ даних відразу
                var predictions = model.Transform(dataView);

                // 4. Порівнюємо Прогноз (Score) з Реальність (PositionOrder)
                // LabelColumnName - це те, що ми намагалися вгадати
                var metrics = _mlContext.Regression.Evaluate(predictions, labelColumnName: "PositionOrder", scoreColumnName: "Score");

                // 5. Формуємо красивий звіт
                return $"📊 ЗВІТ ТОЧНОСТІ:\n" +
                       $"-----------------------------------\n" +
                       $"R-Squared (Коефіцієнт детермінації): {metrics.RSquared:0.##}\n" +
                       $"MAE (Середня абсолютна похибка): {metrics.MeanAbsoluteError:0.##} місця\n" +
                       $"RMSE (Середньоквадратична похибка): {metrics.RootMeanSquaredError:0.##}";
            }
            catch (Exception ex)
            {
                return $"Помилка оцінки: {ex.Message}";
            }
        }
    }
}