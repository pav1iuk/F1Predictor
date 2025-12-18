using System.IO;
using System.Windows;
using System.Windows.Controls;
using F1Predictor.Core;
using F1Predictor.Data; // Наш новий проект
using F1Predictor.ML;
using System.Windows.Media.Imaging;
namespace F1Predictor.UI;

public partial class MainWindow : Window
{
    private readonly ModelPredictor? _predictor;

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
            float circuitId = Convert.ToSingle(CircuitCombo.SelectedValue);
            // 2. Робимо прогноз
            float result = _predictor.Predict(driverId, teamId, grid, circuitId);

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