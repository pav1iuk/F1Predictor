using Microsoft.ML;
using System.IO;

namespace F1Predictor.ML
{
    public class ModelTrainer
    {
        // Шлях, куди ми збережемо навчений "мозок" (файл .zip)
        private static string _modelPath = Path.Combine(Environment.CurrentDirectory, "F1Model.zip");

        public void Train(string dataPath)
        {
            // 1. Створення контексту (це середовище для всіх ML операцій)
            var mlContext = new MLContext(seed: 0);

            // 2. Завантаження даних
            // hasHeader: true, бо в нас є рядок "resultId,raceId..."
            // separatorChar: ',', бо це CSV (Comma Separated Values)
            IDataView dataView = mlContext.Data.LoadFromTextFile<RaceData>(dataPath, hasHeader: true, separatorChar: ',');

            // 3. Побудова конвеєра (Pipeline) навчання
            var pipeline = mlContext.Transforms.CopyColumns(outputColumnName: "Label", inputColumnName: "PositionOrder")
                
                // Перетворюємо числа ID (1, 2, 3) на категорії. 
                // Бо ID 4 не "більше" за ID 2, це просто інший пілот.
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "DriverEncoded", inputColumnName: "DriverId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding(outputColumnName: "TeamEncoded", inputColumnName: "ConstructorId"))
                .Append(mlContext.Transforms.Categorical.OneHotEncoding("RaceEncoded", "RaceId"))
                // Об'єднуємо всі ознаки (Features) в один вектор, який розуміє алгоритм
                .Append(mlContext.Transforms.Concatenate("Features", "DriverEncoded", "TeamEncoded","RaceEncoded" ,"Grid"))
                
                // Вибираємо алгоритм. FastTree - це крутий алгоритм для регресії (передбачення чисел)
                .Append(mlContext.Regression.Trainers.FastTree());

            // 4. Тренування моделі (Тут відбувається магія)
            // Комп'ютер проганяє дані і шукає закономірності
            var model = pipeline.Fit(dataView);

            // 5. Збереження моделі у файл
            mlContext.Model.Save(model, dataView.Schema, _modelPath);
            
            Console.WriteLine($"Модель успішно натренована і збережена в: {_modelPath}");
        }
    }
}