using System.IO;
using System.Windows;
using System.Windows.Controls;
using F1Predictor.Core;
using F1Predictor.Data; // Наш новий проект
using F1Predictor.ML;
using System.Windows.Media.Imaging;
using LiveCharts;
using LiveCharts.Wpf;
namespace F1Predictor.UI;

public partial class MainWindow : Window
{
    private readonly ModelPredictor? _predictor;
    private readonly HistoryService _historyService; // Додай using F1Predictor.Data;
    public MainWindow()
    {
        InitializeComponent();
        /* string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        try {
            var trainer = new ModelTrainer();
            trainer.Train(
                Path.Combine(baseDir, "Data", "results.csv"), 
                Path.Combine(baseDir, "Data", "races.csv") // Додали другий файл
            );
            MessageBox.Show("Модель оновлено з урахуванням ТРАС!");
        } catch (Exception ex) { MessageBox.Show("Error training: " + ex.Message); } */
        // 1. Завантаження моделі
        try 
        {
            _predictor = new ModelPredictor();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка ML: {ex.Message}");
        }
        _historyService = new HistoryService();
        // 2. Заповнення списків (Новий код)
        LoadFormData();
    }

    private void LoadFormData()
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Завантажуємо дані через наш CsvDataLoader
            var drivers = CsvDataLoader.LoadDrivers(Path.Combine(baseDir, "Data", "drivers.csv"));
            var teams = CsvDataLoader.LoadTeams(Path.Combine(baseDir, "Data", "constructors.csv"));
            var circuits = CsvDataLoader.LoadCircuits(Path.Combine(baseDir, "Data", "circuits.csv"));
            CircuitCombo.ItemsSource = circuits;
            CircuitCombo.SelectedIndex = 0;
            // Прив'язуємо дані до ComboBox
            DriverCombo.ItemsSource = drivers;
            TeamCombo.ItemsSource = teams;
            DriverACombo.ItemsSource = drivers;
            DriverBCombo.ItemsSource = drivers;
            // Вибираємо перші значення за замовчуванням
            DriverCombo.SelectedIndex = 0;
            TeamCombo.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка завантаження списків: {ex.Message}");
        }
    }

    private void ButtonPredict_Click(object sender, RoutedEventArgs e)
    {
        // Перевірка на завантаження ШІ
        if (_predictor == null)
        {
            ResultText.Text = "Помилка: Модель не завантажена";
            ResultText.Foreground = System.Windows.Media.Brushes.Red;
            return;
        }

        try
        {
            // Перевірка на вибір користувача
            if (DriverCombo.SelectedValue == null || TeamCombo.SelectedValue == null)
            {
                MessageBox.Show("Будь ласка, оберіть пілота та команду!");
                return;
            }

            // 1. Отримуємо дані (Безпечний метод!)
            float driverId = Convert.ToSingle(DriverCombo.SelectedValue);
            float teamId = Convert.ToSingle(TeamCombo.SelectedValue);
            float grid = (float)GridSlider.Value;
            if (CircuitCombo.SelectedValue == null) 
            {
                MessageBox.Show("Оберіть трасу!"); return;
            }
            float circuitId = Convert.ToSingle(CircuitCombo.SelectedValue);
            // 2. Робимо прогноз
            float result = _predictor.Predict(driverId, teamId, grid, circuitId);
            var driverObj = DriverCombo.SelectedItem as Driver; // using F1Predictor.Core
            var teamObj = TeamCombo.SelectedItem as Team;
            var circuitObj = CircuitCombo.SelectedItem as Circuit;

            var record = new PredictionHistory
            {
                DriverName = driverObj?.FullName ?? "Unknown",
                TeamName = teamObj?.Name ?? "Unknown",
                CircuitName = circuitObj?.Name ?? "Unknown",
                GridPosition = (int)grid,
                PredictedPosition = result,
                Date = DateTime.Now
            };

            _historyService.AddRecord(record);
            // 3. Виводимо результат
            // Округляємо до цілого числа (наприклад, 3.2 -> 3 місце)
            int position = (int)Math.Round(result);
        
            ResultText.Text = $"{position} місце";
        
            // Можна додати колір залежно від місця
            if (position == 1) 
                ResultText.Text += " 🏆 (Перемога!)";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}");
        }
    }
    private void HistoryTab_Selected(object sender, RoutedEventArgs e)
    {
        // Завантажуємо дані з БД і показуємо в таблиці
        HistoryGrid.ItemsSource = _historyService.GetAll();
    }
    private void TeamCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Якщо нічого не вибрано - виходимо
        if (TeamCombo.SelectedValue == null) return;

        // Отримуємо назву обраної команди
        // (TeamCombo.SelectedItem повертає об'єкт Team, який ми створили в Core)
        var selectedTeam = TeamCombo.SelectedItem as Team;
        if (selectedTeam == null) return;

        string url = GetTeamLogoUrl(selectedTeam.Name);

        // Завантажуємо картинку за посиланням
        try
        {
            TeamLogo.Source = new BitmapImage(new Uri(url));
        }
        catch
        {
            // Якщо посилання бите - ігноруємо
        }
    }
    private void DriverCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateChart();
    
        if (DriverCombo.SelectedItem is Driver selectedDriver)
        {
            // Фото
            try { DriverPhoto.Source = new BitmapImage(new Uri(GetDriverPhotoUrl(selectedDriver.FullName))); } catch { }
        
            // НОВЕ: Оновлення піт-стопів (викликаємо через Task, щоб не зависало)
            Task.Run(() => 
            {
                string avgStop = GetAvgPitStopTime(selectedDriver.DriverId);
                Dispatcher.Invoke(() => PitStopText.Text = avgStop);
            });
        }
    }
    private void UpdateChart()
    {
        if (DriverCombo.SelectedValue == null) return;
        float driverId = Convert.ToSingle(DriverCombo.SelectedValue);

        // Отримуємо подвійні дані (Кваліфікація + Гонка)
        var performance = GetDriverPerformance(driverId);

        if (performance.Count == 0) return;

        StatsChart.Series = new SeriesCollection
        {
            // Лінія 1: Кваліфікація (Синя, пунктирна)
            new LineSeries
            {
                Title = "Кваліфікація",
                Values = new ChartValues<double>(performance.Select(x => x.QualiPos)),
                PointGeometry = DefaultGeometries.Square,
                PointGeometrySize = 10,
                Stroke = System.Windows.Media.Brushes.DodgerBlue,
                Fill = System.Windows.Media.Brushes.Transparent, // Без заливки
                StrokeDashArray = new System.Windows.Media.DoubleCollection { 2 } // Пунктирна лінія
            },
            
            // Лінія 2: Гонка (Червона, суцільна)
            new LineSeries
            {
                Title = "Фініш",
                Values = new ChartValues<double>(performance.Select(x => x.RacePos)),
                PointGeometry = DefaultGeometries.Circle,
                PointGeometrySize = 12,
                Stroke = System.Windows.Media.Brushes.Red,
                Fill = System.Windows.Media.Brushes.Transparent, // Можна додати легку червону заливку
                StrokeThickness = 3
            }
        };
        
        // Оновлюємо підписи осей, якщо треба
    }
    private void CircuitCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateChart(); // Твій старий метод

        // НОВЕ: Оновлення карти
        if (CircuitCombo.SelectedItem is Circuit selectedCircuit)
        {
            try { CircuitImage.Source = new BitmapImage(new Uri(GetCircuitMapUrl(selectedCircuit.Name))); }
            catch { }
        }
    }
    private void ButtonDelete_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 1. Дізнаємося, яку саме кнопку натиснули
            var button = sender as Button;
            
            // 2. Отримуємо дані рядка, в якому ця кнопка знаходиться
            // (DataContext кнопки - це і є наш об'єкт PredictionHistory)
            var record = button.DataContext as PredictionHistory;

            if (record == null) return;

            // 3. Питаємо підтвердження (щоб не видалити випадково)
            var result = MessageBox.Show($"Видалити запис про {record.DriverName}?", 
                "Підтвердження", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                // 4. Видаляємо з бази
                _historyService.DeleteRecord(record.Id);

                // 5. Оновлюємо таблицю на екрані
                HistoryGrid.ItemsSource = _historyService.GetAll();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка видалення: {ex.Message}");
        }
    }
    private void ButtonEvaluate_Click(object sender, RoutedEventArgs e)
    {
        // Показуємо, що процес пішов (бо це може зайняти пару секунд)
        ResultText.Text = "Обчислення метрик...";
        
        // Запускаємо в окремому потоці, щоб вікно не зависло (Task.Run)
        Task.Run(() => 
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resultsPath = Path.Combine(baseDir, "Data", "results.csv");
            string racesPath = Path.Combine(baseDir, "Data", "races.csv");

            // Створюємо оцінювач
            var evaluator = new F1Predictor.ML.ModelEvaluator();
            string report = evaluator.Evaluate(resultsPath, racesPath);

            // Повертаємося в головний потік, щоб показати результат
            Dispatcher.Invoke(() => 
            {
                MessageBox.Show(report, "Наукова оцінка моделі", MessageBoxButton.OK, MessageBoxImage.Information);
                ResultText.Text = "---"; // Повертаємо текст назад
            });
        });
    }
    private string GetAvgPitStopTime(float driverId)
    {
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "pit_stops.csv");
    
        // Це може зайняти час, тому в реальному проекті це кешують, але для курсової ок
        var allStops = CsvDataLoader.LoadPitStops(path);
    
        var driverStops = allStops
            .Where(p => p.DriverId == driverId)
            .Select(p => p.Seconds)
            .ToList();

        if (driverStops.Count == 0) return "Немає даних";

        // Відфільтруємо аномалії (наприклад, піт-стопи довші 40 секунд - це ремонт, а не заміна коліс)
        var cleanStops = driverStops.Where(t => t < 40).ToList();
    
        if (cleanStops.Count == 0) return "---";

        double avg = cleanStops.Average();
        return $"{avg:F2} сек";
    }
    // Додай цей клас-модель прямо всередині MainWindow.xaml.cs або окремо
    public class RacePerformance
    {
        public int RaceNumber { get; set; } // Просто 1, 2, 3... для осі X
        public double QualiPos { get; set; }
        public double RacePos { get; set; }
    }
    private DriverStats CalculateStats(float driverId, string driverName)
    {
        string resultsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "results.csv");

        // Читаємо файл і фільтруємо по водію
        var driverResults = File.ReadAllLines(resultsPath)
            .Skip(1)
            .Select(line => line.Split(','))
            .Where(p => 
            {
                // Перевіряємо, чи це наш водій
                return float.TryParse(p[2], out float dId) && dId == driverId;
            })
            .Select(p => new
            {
                Grid = int.Parse(p[5]),
                PositionOrder = int.Parse(p[8]) // Фінальна позиція
            })
            .ToList();

        if (driverResults.Count == 0) return new DriverStats { DriverName = driverName };

        // Рахуємо статистику (LINQ Aggregation)
        return new DriverStats
        {
            DriverName = driverName,
            TotalRaces = driverResults.Count,
            Wins = driverResults.Count(r => r.PositionOrder == 1),
            Podiums = driverResults.Count(r => r.PositionOrder <= 3),
            Poles = driverResults.Count(r => r.Grid == 1),
            AvgPosition = driverResults.Average(r => r.PositionOrder)
        };
    }
// Додай цей метод у клас MainWindow
    private List<RacePerformance> GetDriverPerformance(float driverId)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string resultsPath = Path.Combine(baseDir, "Data", "results.csv");
        string qualiPath = Path.Combine(baseDir, "Data", "qualifying.csv");

        // 1. Вантажимо Кваліфікації пілота
        var allQualis = CsvDataLoader.LoadQualifying(qualiPath)
            .Where(q => q.DriverId == driverId)
            .ToList();

        // 2. Вантажимо Результати гонок (використовуємо твій існуючий лоадер або пишемо простий тут)
        // Швидкий варіант читання results.csv тільки для цього графіку:
        var allRaces = File.ReadAllLines(resultsPath).Skip(1)
            .Select(line => line.Split(','))
            .Where(p => float.Parse(p[2]) == driverId)
            .Select(p => new { 
                RaceId = int.Parse(p[1]), 
                Pos = int.Parse(p[8]) 
            })
            .OrderByDescending(r => r.RaceId) // Спочатку нові
            .Take(15) // Беремо останні 15 гонок
            .ToList();

        var data = new List<RacePerformance>();
        int counter = 1;

        // 3. З'єднуємо (Join)
        foreach (var race in allRaces.OrderBy(r => r.RaceId)) // Сортуємо від старих до нових для графіка
        {
            var quali = allQualis.FirstOrDefault(q => q.RaceId == race.RaceId);
        
            data.Add(new RacePerformance
            {
                RaceNumber = counter++,
                RacePos = race.Pos,
                // Якщо даних про кваліфікацію немає (старі гонки), беремо те саме, що в гонці, або 0
                QualiPos = quali != null ? quali.Position : race.Pos 
            });
        }

        return data;
    }
    private void ButtonCompare_Click(object sender, RoutedEventArgs e)
    {
        // 1. Перевірка вибору
        if (DriverACombo.SelectedItem == null || DriverBCombo.SelectedItem == null)
        {
            MessageBox.Show("Оберіть обох пілотів для битви!");
            return;
        }

        // 2. Отримуємо дані з ComboBox
        var driverA = DriverACombo.SelectedItem as Driver;
        var driverB = DriverBCombo.SelectedItem as Driver;

        // 3. Рахуємо статистику (це може зайняти секунду, тому краще Task.Run, але можна і так)
        var statsA = CalculateStats(driverA.DriverId, driverA.FullName);
        var statsB = CalculateStats(driverB.DriverId, driverB.FullName);

        // 4. Малюємо Стовпчикову діаграму (Column Chart)
        VsChart.Series = new SeriesCollection
        {
            // Стовпчики Пілота А
            new ColumnSeries
            {
                Title = statsA.DriverName,
                Values = new ChartValues<int> { statsA.TotalRaces, statsA.Wins, statsA.Podiums, statsA.Poles },
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.DodgerBlue // Синій колір
            },
            
            // Стовпчики Пілота B
            new ColumnSeries
            {
                Title = statsB.DriverName,
                Values = new ChartValues<int> { statsB.TotalRaces, statsB.Wins, statsB.Podiums, statsB.Poles },
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.Red // Червоний колір
            }
        };
    }
    private string GetDriverPhotoUrl(string driverName)
    {
        string name = driverName.ToLower().Trim();

        if (name.Contains("verstappen")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/M/MAXVER01_Max_Verstappen/maxver01.png.transform/2col/image.png";
        if (name.Contains("hamilton")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/LEWHAM01_Lewis_Hamilton/lewham01.png.transform/2col/image.png";
        if (name.Contains("leclerc")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/C/CHALEC01_Charles_Leclerc/chalec01.png.transform/2col/image.png";
        if (name.Contains("norris")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/兰NOR01_Lando_Norris/lannor01.png.transform/2col/image.png"; // URL може змінюватися, це приклад
        if (name.Contains("alonso")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/F/FERALO01_Fernando_Alonso/feralo01.png.transform/2col/image.png";
        if (name.Contains("sainz")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/C/CARSAI01_Carlos_Sainz/carsai01.png.transform/2col/image.png";
        if (name.Contains("russell")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/G/GEORUS01_George_Russell/georus01.png.transform/2col/image.png";
        if (name.Contains("piastri")) return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/O/OSCPIA01_Oscar_Piastri/oscpia01.png.transform/2col/image.png";
    
        // Заглушка (шолом)
        return "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/driver_fallback_image.png";
    }
    private string GetCircuitMapUrl(string circuitName)
    {
        string name = circuitName.ToLower();

        if (name.Contains("monaco")) return "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244984/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Monaco_Circuit.png";
        if (name.Contains("monza")) return "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244987/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Italy_Circuit.png";
        if (name.Contains("silverstone")) return "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Great_Britain_Circuit.png";
        if (name.Contains("spa")) return "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244982/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Belgium_Circuit.png";
        if (name.Contains("suzuka")) return "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Japan_Circuit.png";
    
        // Заглушка (карта світу)
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/e/ec/World_Map_Blank.svg/640px-World_Map_Blank.svg.png";
    }
    private string GetTeamLogoUrl(string teamName)
{
    string name = teamName.ToLower().Trim();

    // --- АКТУАЛЬНІ КОМАНДИ (PNG посилання) ---

    // Red Bull
    if (name.Contains("red bull")) 
        return "https://upload.wikimedia.org/wikipedia/de/thumb/c/c4/Red_Bull_Racing_logo.svg/2560px-Red_Bull_Racing_logo.svg.png";

    // Ferrari
    if (name.Contains("ferrari")) 
        return "https://upload.wikimedia.org/wikipedia/de/thumb/c/c0/Scuderia_Ferrari_Logo.svg/1024px-Scuderia_Ferrari_Logo.svg.png";

    // Mercedes
    if (name.Contains("mercedes") || name.Contains("brawn") || name.Contains("tyrrell") || name.Contains("bar")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/f/fb/Mercedes_AMG_Petronas_F1_Logo.svg/2560px-Mercedes_AMG_Petronas_F1_Logo.svg.png";

    // McLaren
    if (name.Contains("mclaren")) 
        return "https://upload.wikimedia.org/wikipedia/en/thumb/6/66/McLaren_Racing_logo.svg/2560px-McLaren_Racing_logo.svg.png";

    // Aston Martin (Racing Point, Force India, Jordan)
    if (name.Contains("aston martin") || name.Contains("racing point") || name.Contains("force india") || name.Contains("jordan")) 
        return "https://upload.wikimedia.org/wikipedia/fr/thumb/7/72/Aston_Martin_Aramco_Cognizant_F1.svg/2560px-Aston_Martin_Aramco_Cognizant_F1.svg.png";

    // Alpine (Renault, Benetton)
    if (name.Contains("alpine") || name.Contains("renault") || name.Contains("benetton")) 
        return "https://upload.wikimedia.org/wikipedia/fr/thumb/6/60/Alpine_F1_Team_2021_Logo.svg/2560px-Alpine_F1_Team_2021_Logo.svg.png";

    // Williams
    if (name.Contains("williams")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/f/f2/Williams_Racing_2020_logo.png/800px-Williams_Racing_2020_logo.png";

    // Haas
    if (name.Contains("haas")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/d/d4/Haas_F1_Team_logo.svg/2560px-Haas_F1_Team_logo.svg.png";

    // RB (AlphaTauri, Toro Rosso, Minardi)
    if (name.Contains("rb") || name.Contains("visa") || name.Contains("alphatauri") || name.Contains("toro rosso") || name.Contains("minardi"))
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/e/e9/Visa_Cash_App_RB_F1_Team_logo.svg/2560px-Visa_Cash_App_RB_F1_Team_logo.svg.png";

    // Kick Sauber (Alfa Romeo, Sauber)
    if (name.Contains("kick") || name.Contains("sauber") || name.Contains("alfa romeo") || name.Contains("bmw"))
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/1/12/Kick_Sauber_F1_Team_logo.svg/2560px-Kick_Sauber_F1_Team_logo.svg.png";

    // --- ІСТОРИЧНІ ---

    // Team Lotus
    if (name.Contains("lotus")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/4/46/Lotus_F1_Team_Logo.svg/800px-Lotus_F1_Team_Logo.svg.png";

    // Brabham
    if (name.Contains("brabham")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/0/01/Brabham_Logo.png/800px-Brabham_Logo.png";

    // Toyota
    if (name.Contains("toyota")) 
        return "https://upload.wikimedia.org/wikipedia/commons/thumb/1/1c/Panasonic_Toyota_Racing_logo.svg/2560px-Panasonic_Toyota_Racing_logo.svg.png";

    // Заглушка (PNG)
    return "https://upload.wikimedia.org/wikipedia/commons/thumb/3/33/F1.svg/2560px-F1.svg.png";
}
}