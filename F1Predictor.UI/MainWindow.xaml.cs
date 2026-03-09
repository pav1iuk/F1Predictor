using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using F1Predictor.Core;
using F1Predictor.Data;
using F1Predictor.ML;
using System.Windows.Media.Imaging;
using LiveCharts;
using LiveCharts.Wpf;
namespace F1Predictor.UI;

public partial class MainWindow : Window
{
    private readonly ModelPredictor? _predictor;
    private readonly HistoryService _historyService;
    public MainWindow()
    {
        InitializeComponent();
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
        // 2. Заповнення списків
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
    // 1. Перевірка на завантаження ШІ
    if (_predictor == null)
    {
        ResultText.Text = "Помилка: Модель не готова";
        ResultText.Foreground = System.Windows.Media.Brushes.Red;
        return;
    }

    try
    {
        // 2. Перевірка чи все обрано
        if (DriverCombo.SelectedValue == null || 
            TeamCombo.SelectedValue == null || 
            CircuitCombo.SelectedValue == null)
        {
            MessageBox.Show("Будь ласка, оберіть пілота, команду та трасу!", "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        // 3. Збираємо вхідні дані
        float driverId = Convert.ToSingle(DriverCombo.SelectedValue);
        float teamId = Convert.ToSingle(TeamCombo.SelectedValue);
        float circuitId = Convert.ToSingle(CircuitCombo.SelectedValue);
        float grid = (float)GridSlider.Value;

        // 4. Робимо прогноз (отримуємо "сире" число, наприклад 2.45)
        float rawResult = _predictor.Predict(driverId, teamId, grid, circuitId);
        if (WeatherToggle.IsChecked == true)
        {
            // Дощ вносить хаос!
            Random rainChaos = new Random();
    
            // Генеруємо випадкове число від -1.5 до +3.5
            // Це означає: пілот може випадково відіграти 1.5 місця (майстерність)
            // Або втратити 3.5 місця (помилка на слизькій трасі)
            float chaosFactor = (float)(rainChaos.NextDouble() * 5.0 - 1.5);
    
            rawResult += chaosFactor;

            // Штраф за погану надійність у дощ
        }
        // --- ЛОГІКА ОБРОБКИ РЕЗУЛЬТАТУ ---
        int finalPosition;
        
        if (rawResult <= 1.6f)
        {
            finalPosition = 1;
        }
        else
        {
            finalPosition = (int)Math.Round(rawResult);
        }

        // ЗАХИСТ: Місце не може бути менше 1 і більше 20
        if (finalPosition < 1) finalPosition = 1;
        if (finalPosition > 20) finalPosition = 20;

        // 5. Зберігаємо в історію
        var driverObj = DriverCombo.SelectedItem as Driver;
        var teamObj = TeamCombo.SelectedItem as Team;
        var circuitObj = CircuitCombo.SelectedItem as Circuit;

        var record = new PredictionHistory
        {
            DriverName = driverObj?.FullName ?? "Unknown",
            TeamName = teamObj?.Name ?? "Unknown",
            CircuitName = circuitObj?.Name ?? "Unknown",
            GridPosition = (int)grid,
            PredictedPosition = rawResult,
            Date = DateTime.Now
        };

        _historyService.AddRecord(record);

        // !!! ОНОВЛЮЄМО ТАБЛИЦЮ ІСТОРІЇ МИТТЄВО !!!
        if (HistoryGrid != null)
        {
            HistoryGrid.ItemsSource = _historyService.GetAll();
        }

        // 6. Виводимо красивий результат на екран
        ResultText.Text = $"{finalPosition} місце";
        var animation = new System.Windows.Media.Animation.DoubleAnimation
        {
            From = 0.0,   
            To = 1.0,     
            Duration = TimeSpan.FromMilliseconds(500), 
            EasingFunction = new System.Windows.Media.Animation.BackEase { Amplitude = 0.5 } 
        };

// Запускаємо анімацію для ширини і висоти
        ResultText.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
        ResultText.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
        // Змінюємо колір тексту залежно від результату
        if (finalPosition == 1)
        {
            ResultText.Foreground = System.Windows.Media.Brushes.Gold;
            ResultText.Text += " 🏆";
        }
        else if (finalPosition <= 3)
        {
            ResultText.Foreground = System.Windows.Media.Brushes.LightGreen; 
        }
        else if (finalPosition >= 15)
        {
            ResultText.Foreground = System.Windows.Media.Brushes.OrangeRed;
        }
        else
        {
            ResultText.Foreground = System.Windows.Media.Brushes.White;
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Критична помилка: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
    private void HistoryTab_Selected(object sender, RoutedEventArgs e)
    {
        HistoryGrid.ItemsSource = _historyService.GetAll();
    }
    private void TeamCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (TeamCombo.SelectedValue == null) return;

        var selectedTeam = TeamCombo.SelectedItem as Team;
        if (selectedTeam == null) return;

        string url = GetTeamLogoUrl(selectedTeam.Name);

        try
        {
            TeamLogo.Source = new BitmapImage(new Uri(url));
        }
        catch
        {
        }
    }
    private void DriverCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    UpdateChart();

    if (DriverCombo.SelectedItem is Driver selectedDriver)
    {
        // --- ОНОВЛЕННЯ ФОТО ---
        try
        {
            DriverPhoto.Source = new BitmapImage(new Uri(GetDriverPhotoUrl(selectedDriver.FullName)));
        }
        catch 
        { 
            // Ігноруємо помилки завантаження фото
        }

        // --- ЗЧИТУВАННЯ НАЛАШТУВАНЬ (UI Thread) ---
        bool isTrackSpecific = TrackFilterToggle.IsChecked == true;
        bool isRain = WeatherToggle.IsChecked == true;

        float currentCircuitId = -1;
        if (CircuitCombo.SelectedValue != null)
        {
            currentCircuitId = Convert.ToSingle(CircuitCombo.SelectedValue);
        }

        // --- ОБЧИСЛЕННЯ В ФОНІ (Task.Run) ---
        Task.Run(() =>
        {
            // А. Отримуємо середній піт-стоп
            string avgStop = GetAvgPitStopTime(selectedDriver.DriverId);

            // Б. Отримуємо базову надійність
            double reliability = GetReliabilityStats(selectedDriver.DriverId, isTrackSpecific ? currentCircuitId : -1);

            // --- В. ЗАСТОСОВУЄМО ЕФЕКТ ДОЩУ ---
            if (isRain)
            {
                // Якщо йде дощ, надійність падає на 20%
                reliability = reliability * 0.8; 
            }

            // --- ОНОВЛЕННЯ UI (Dispatcher) ---
            Dispatcher.Invoke(() =>
            {
                // 1. Виводимо час піт-стопу
                if (PitStopText != null)
                    PitStopText.Text = avgStop;

                // 2. Виводимо надійність
                if (ReliabilityText != null && ReliabilityBar != null)
                {
                    ReliabilityText.Text = $"{reliability:F0}%";
                    ReliabilityBar.Value = reliability;         

                    // 3. Змінюємо колір залежно від відсотка
                    if (reliability >= 90)
                        ReliabilityBar.Foreground = System.Windows.Media.Brushes.LightGreen; 
                    else if (reliability >= 75)
                        ReliabilityBar.Foreground = System.Windows.Media.Brushes.Orange;     
                    else
                        ReliabilityBar.Foreground = System.Windows.Media.Brushes.Red;        
                }
            });
        });
    }
}
    private void UpdateChart()
{
    // Перевірки на null
    if (DriverCombo.SelectedValue == null || CircuitCombo.SelectedValue == null) return;
    
    // Щоб уникнути помилок при завантаженні форми
    if (StatsChart == null) return;

    float driverId = Convert.ToSingle(DriverCombo.SelectedValue);
    float circuitId = Convert.ToSingle(CircuitCombo.SelectedValue);
    bool isTrackSpecific = TrackFilterToggle.IsChecked == true;

    // Отримуємо детальну статистику (новий метод)
    var performance = GetDetailedPerformance(driverId, circuitId, isTrackSpecific);

    if (performance.Count == 0) 
    {
        StatsChart.Series = null;
        return;
    }

    // Малюємо ДВІ лінії
    StatsChart.Series = new SeriesCollection
    {
        // Лінія 1: Кваліфікація (залишаємо як є, пунктиром)
        new LineSeries 
        { 
            Title = "Кваліфікація", 
            Values = new ChartValues<double>(performance.Select(x => x.QualiPos)),
            Stroke = System.Windows.Media.Brushes.DodgerBlue,
            Fill = System.Windows.Media.Brushes.Transparent,
            PointGeometry = DefaultGeometries.Square,
            StrokeDashArray = new System.Windows.Media.DoubleCollection { 2 }
        },

        // Лінія 2: Гонка
        new LineSeries
        {
            Title = "Фініш",
            Values = new ChartValues<double>(performance.Select(x => x.RacePos)),
            PointGeometry = DefaultGeometries.Circle,
            PointGeometrySize = 15, 
            StrokeThickness = 4,    
            Stroke = System.Windows.Media.Brushes.Red,
        
            // ГРАДІЄНТНА ЗАЛИВКА ПІД ЛІНІЄЮ
            Fill = new System.Windows.Media.LinearGradientBrush
            {
                StartPoint = new System.Windows.Point(0, 0),
                EndPoint = new System.Windows.Point(0, 1),
                GradientStops = new System.Windows.Media.GradientStopCollection
                {
                    new System.Windows.Media.GradientStop(System.Windows.Media.Color.FromArgb(80, 255, 82, 82), 0), 
                    new System.Windows.Media.GradientStop(System.Windows.Media.Colors.Transparent, 1)
                }
            }
        }
    };
}
    private void CircuitCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateChart(); 
        
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
            var record = button.DataContext as PredictionHistory;

            if (record == null) return;

            // 3. Питаємо підтвердження
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
        // Показуємо, що процес пішов
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
        try
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "pit_stops.csv");
            if (!File.Exists(path)) return "---";

            var validStops = File.ReadAllLines(path)
                .Skip(1)
                .Select(line => line.Split(','))
                // 1. Шукаємо нашого водія (index 1)
                .Where(p => int.Parse(p[1]) == (int)driverId)
                // 2. Беремо час у мілісекундах (index 6)
                .Select(p => int.Parse(p[6]))
                // 3. ФІЛЬТР: Відкидаємо ремонти і простої (все, що довше 35 секунд / 35000 мс)
                .Where(ms => ms < 35000) 
                .ToList();

            if (validStops.Count == 0) return "Немає даних";
            
            double avgSeconds = validStops.Average() / 1000.0;
        
            return $"{avgSeconds:F2} с";
        }
        catch
        {
            return "---";
        }
    }
    public class RacePerformance
    {
        public int RaceNumber { get; set; }
        public string RaceName { get; set; }
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
    private List<RacePerformance> GetDriverPerformance(float driverId)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string resultsPath = Path.Combine(baseDir, "Data", "results.csv");
        string qualiPath = Path.Combine(baseDir, "Data", "qualifying.csv");

        // 1. Вантажимо Кваліфікації пілота
        var allQualis = CsvDataLoader.LoadQualifying(qualiPath)
            .Where(q => q.DriverId == driverId)
            .ToList();

        // 2. Вантажимо Результати гонок
        var allRaces = File.ReadAllLines(resultsPath).Skip(1)
            .Select(line => line.Split(','))
            .Where(p => float.Parse(p[2]) == driverId)
            .Select(p => new { 
                RaceId = int.Parse(p[1]), 
                Pos = int.Parse(p[8]) 
            })
            .OrderByDescending(r => r.RaceId)
            .Take(15)
            .ToList();

        var data = new List<RacePerformance>();
        int counter = 1;

        // 3. З'єднуємо (Join)
        foreach (var race in allRaces.OrderBy(r => r.RaceId))
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

        // 3. Рахуємо статистику
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
                Fill = System.Windows.Media.Brushes.DodgerBlue
            },
            
            // Стовпчики Пілота B
            new ColumnSeries
            {
                Title = statsB.DriverName,
                Values = new ChartValues<int> { statsB.TotalRaces, statsB.Wins, statsB.Podiums, statsB.Poles },
                DataLabels = true,
                Fill = System.Windows.Media.Brushes.Red
            }
        };
    }
    private double GetReliabilityStats(float driverId, float circuitId = -1)
    {
        try
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string resultsPath = Path.Combine(baseDir, "Data", "results.csv");
            string racesPath = Path.Combine(baseDir, "Data", "races.csv");

            if (!File.Exists(resultsPath)) return 0;

            // 1. Читаємо результати
            var query = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(p => float.TryParse(p[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float id) && id == driverId)
                .Select(p => new 
                { 
                    RaceId = float.Parse(p[1], System.Globalization.CultureInfo.InvariantCulture), 
                    StatusId = int.Parse(p[p.Length - 1]) // Остання колонка
                });

            // 2. Якщо увімкнено фільтр по трасі - треба відсіяти зайві гонки
            if (circuitId != -1)
            {
                // Завантажуємо список ID гонок, які проходили на цій трасі
                var validRaceIds = File.ReadAllLines(racesPath)
                    .Skip(1)
                    .Select(line => line.Split(','))
                    .Where(p => float.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture) == circuitId)
                    .Select(p => float.Parse(p[0], System.Globalization.CultureInfo.InvariantCulture))
                    .ToHashSet();

                query = query.Where(x => validRaceIds.Contains(x.RaceId));
            }

            var races = query.ToList();

            if (races.Count == 0) return 0;

            int finishedCount = races.Count(s => s.StatusId == 1 || (s.StatusId >= 11 && s.StatusId <= 19));
            return (double)finishedCount / races.Count * 100.0;
        }
        catch { return 0; }
    }
    private void Filter_Changed(object sender, RoutedEventArgs e)
    {
        DriverCombo_SelectionChanged(null, null);
    }
    private List<RacePerformance> GetDetailedPerformance(float driverId, float circuitId, bool isTrackSpecific)
{
    string baseDir = AppDomain.CurrentDomain.BaseDirectory;
    string resultsPath = Path.Combine(baseDir, "Data", "results.csv");
    string qualiPath = Path.Combine(baseDir, "Data", "qualifying.csv");
    string racesPath = Path.Combine(baseDir, "Data", "races.csv");

    // 1. Вантажимо Кваліфікації
    var allQualis = CsvDataLoader.LoadQualifying(qualiPath)
        .Where(q => q.DriverId == driverId)
        .ToList();

    // 2. Вантажимо структуру Гонок
    var allRacesInfo = File.ReadAllLines(racesPath).Skip(1)
        .Select(line => line.Split(','))
        .Select(p => new 
        { 
            RaceId = int.Parse(p[0]), 
            Year = int.Parse(p[1]),
            CircuitId = int.Parse(p[3]),
            Name = p[4].Trim('"')
        })
        .ToList();

    // 3. Вантажимо Результати (Race Positions)
    var driverResults = File.ReadAllLines(resultsPath).Skip(1)
        .Select(line => line.Split(','))
        .Where(p => float.TryParse(p[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float id) && id == driverId)
        .Select(p => new 
        { 
            RaceId = int.Parse(p[1]), 
            Pos = int.Parse(p[8]) 
        })
        .ToList();

    // 4. ФІЛЬТРАЦІЯ
    List<int> targetRaceIds;

    if (isTrackSpecific)
    {
        // Беремо тільки гонки на цій трасі
        targetRaceIds = allRacesInfo
            .Where(r => r.CircuitId == (int)circuitId)
            .OrderBy(r => r.Year) 
            .Select(r => r.RaceId)
            .ToList();
    }
    else
    {
        // Беремо останні 15 гонок (Загальна форма)
        targetRaceIds = driverResults
            .OrderByDescending(r => r.RaceId)
            .Take(15)
            .Select(r => r.RaceId)
            .OrderBy(id => id)
            .ToList();
    }

    // 5. Збираємо фінальний список (JOIN)
    var data = new List<RacePerformance>();
    int counter = 1;

    foreach (var rId in targetRaceIds)
    {
        var raceRes = driverResults.FirstOrDefault(r => r.RaceId == rId);
        if (raceRes == null) continue;

        var raceInfo = allRacesInfo.FirstOrDefault(r => r.RaceId == rId);
        var qualiRes = allQualis.FirstOrDefault(q => q.RaceId == rId);

        data.Add(new RacePerformance
        {
            RaceNumber = counter++,
            RaceName = isTrackSpecific ? raceInfo?.Year.ToString() : raceInfo?.Name,
            RacePos = raceRes.Pos,
            QualiPos = qualiRes != null ? qualiRes.Position : raceRes.Pos 
        });
    }

    return data;
}
    private void Weather_Changed(object sender, RoutedEventArgs e)
    {
        bool isRain = WeatherToggle.IsChecked == true;

        // Змінюємо колір картки
        if (SettingsCard != null)
        {
            var brush = isRain 
                ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#1A237E")) // Rain Color
                : new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#303030")); // Default Color
            
            SettingsCard.Background = brush;
        }

        // Оновлюємо цифри
        DriverCombo_SelectionChanged(null, null);
    }
    private string GetDriverPhotoUrl(string driverName)
    {
        return ImageHelper.GetDriverUrl(driverName);
    }
    private string GetCircuitMapUrl(string circuitName)
    {
        return ImageHelper.GetCircuitUrl(circuitName);
    }
    private string GetTeamLogoUrl(string teamName)
    {
        return ImageHelper.GetTeamUrl(teamName);
    }
}