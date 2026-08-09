using System.Net.Http;
using System.Text.Json;
using F1Predictor.Data;

namespace F1Predictor.Services;

public class OpenF1ApiService
{
    private readonly HttpClient _httpClient;

    public OpenF1ApiService()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.openf1.org/v1/")
        };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "F1PredictorApp");
    }

    // 1. Отримання етапів (трас)
    public async Task<List<OpenF1Meeting>> GetMeetingsAsync(int year = 2024)
    {
        try
        {
            var response = await _httpClient.GetAsync($"meetings?year={year}");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<OpenF1Meeting>>(json) ?? new List<OpenF1Meeting>();
        }
        catch { return new List<OpenF1Meeting>(); }
    }

    // 2. Отримання останнього складу пілотів та команд
    public async Task<List<OpenF1Driver>> GetLatestDriversAsync()
    {
        try
        {
            // Беремо останню доступну сесію (latest)
            var response = await _httpClient.GetAsync("drivers?session_key=latest");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            
            var drivers = JsonSerializer.Deserialize<List<OpenF1Driver>>(json) ?? new List<OpenF1Driver>();
            
            // Фільтруємо дублікати пілотів у межах однієї сесії
            return drivers
                .Where(d => !string.IsNullOrEmpty(d.FullName))
                .GroupBy(d => d.DriverNumber)
                .Select(g => g.First())
                .ToList();
        }
        catch { return new List<OpenF1Driver>(); }
    }
    // Отримання телеметрії для конкретного пілота
    // Завантаження телеметрії з прив'язкою до пілота та траси
    public async Task<List<OpenF1CarData>> GetCarTelemetryAsync(int driverNumber, string countryOrCircuitName)
    {
        try
        {
            // 1. Отримуємо список останніх сесій для вказаного року
            var sessionsResp = await _httpClient.GetAsync("sessions?year=2024&session_name=Race");
            int sessionKey = 9158; // Резервна сесія

            if (sessionsResp.IsSuccessStatusCode)
            {
                string json = await sessionsResp.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(json);
            
                // Шукаємо сесію, яка відповідає обраній країні/трасі
                foreach (var element in doc.RootElement.EnumerateArray())
                {
                    string location = element.GetProperty("location").GetString() ?? "";
                    string country = element.GetProperty("country_name").GetString() ?? "";

                    if (location.Contains(countryOrCircuitName, StringComparison.OrdinalIgnoreCase) ||
                        country.Contains(countryOrCircuitName, StringComparison.OrdinalIgnoreCase))
                    {
                        sessionKey = element.GetProperty("session_key").GetInt32();
                        break;
                    }
                }
            }

            // 2. Завантажуємо телеметрію для конкретно знайденої сесії цієї траси
            var response = await _httpClient.GetAsync($"car_data?driver_number={driverNumber}&session_key={sessionKey}");
            if (!response.IsSuccessStatusCode) return new List<OpenF1CarData>();

            string carJson = await response.Content.ReadAsStringAsync();
            var data = System.Text.Json.JsonSerializer.Deserialize<List<OpenF1CarData>>(carJson) ?? new List<OpenF1CarData>();

            // Повертаємо робочий відрізок активної їзди (швидкість > 40 км/год)
            return data.Where(d => d.Speed > 40).Take(300).ToList();
        }
        catch
        {
            return new List<OpenF1CarData>();
        }
    }
    // Отримання даних про шинні стінти пілота
    // Отримання даних про шинні стінти пілота
    public async Task<List<OpenF1Stint>> GetDriverStintsAsync(int driverNumber)
    {
        try
        {
            // 1. Спочатку робимо запит за останніми стінтами
            var response = await _httpClient.GetAsync($"stints?driver_number={driverNumber}&session_key=latest");
        
            if (!response.IsSuccessStatusCode) return new List<OpenF1Stint>();

            string json = await response.Content.ReadAsStringAsync();
            var stints = JsonSerializer.Deserialize<List<OpenF1Stint>>(json) ?? new List<OpenF1Stint>();

            // 2. Якщо для "latest" порожньо — робимо резервний запит по фіксованій робочій сесії (session_key = 9158)
            if (stints.Count == 0)
            {
                var fallbackResp = await _httpClient.GetAsync($"stints?driver_number={driverNumber}&session_key=9158");
                if (fallbackResp.IsSuccessStatusCode)
                {
                    string fallbackJson = await fallbackResp.Content.ReadAsStringAsync();
                    stints = JsonSerializer.Deserialize<List<OpenF1Stint>>(fallbackJson) ?? new List<OpenF1Stint>();
                }
            }

            return stints;
        }
        catch
        {
            return new List<OpenF1Stint>();
        }
    }
    // Отримання кіл пілота для аналізу стабільності
    public async Task<List<OpenF1Lap>> GetDriverLapsAsync(int driverNumber, string circuitName = "")
    {
        try
        {
            // 1. Отримуємо список сесій 2024 року
            var sessionResp = await _httpClient.GetAsync("sessions?year=2024&session_name=Race");
            int sessionKey = 9158; // Резервна сесія

            if (sessionResp.IsSuccessStatusCode)
            {
                string sessionJson = await sessionResp.Content.ReadAsStringAsync();
                using var doc = System.Text.Json.JsonDocument.Parse(sessionJson);
                var sessions = doc.RootElement.EnumerateArray().ToList();

                if (sessions.Count > 0)
                {
                    // Шукаємо за назвою, якщо передано, або беремо першу доступну сесію
                    var matchedSession = sessions.FirstOrDefault(s => 
                        !string.IsNullOrEmpty(circuitName) && 
                        (s.GetProperty("location").GetString()?.Contains(circuitName, StringComparison.OrdinalIgnoreCase) == true ||
                         s.GetProperty("country_name").GetString()?.Contains(circuitName, StringComparison.OrdinalIgnoreCase) == true));

                    sessionKey = matchedSession.ValueKind != System.Text.Json.JsonValueKind.Undefined 
                        ? matchedSession.GetProperty("session_key").GetInt32() 
                        : sessions[0].GetProperty("session_key").GetInt32();
                }
            }

            // 2. Запитуємо кола пілота
            var response = await _httpClient.GetAsync($"laps?driver_number={driverNumber}&session_key={sessionKey}");
            if (!response.IsSuccessStatusCode) return new List<OpenF1Lap>();

            string json = await response.Content.ReadAsStringAsync();
            var laps = JsonSerializer.Deserialize<List<OpenF1Lap>>(json) ?? new List<OpenF1Lap>();

            // Очищаємо від заторів та боксів
            return laps.Where(l => l.LapDuration.HasValue && l.LapDuration > 30 && !l.IsPitOutLap).ToList();
        }
        catch
        {
            return new List<OpenF1Lap>();
        }
    }
    // Отримання погодних умов заїзду
    public async Task<OpenF1Weather?> GetSessionWeatherAsync(string circuitName = "")
    {
        try
        {
            int sessionKey = 9158; // Резервна сесія

            // 1. Динамічно шукаємо сесію за назвою обраної траси
            if (!string.IsNullOrEmpty(circuitName))
            {
                var sessionsResp = await _httpClient.GetAsync("sessions?year=2024&session_name=Race");
                if (sessionsResp.IsSuccessStatusCode)
                {
                    string sessionJson = await sessionsResp.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(sessionJson);
                    string cleanName = circuitName.ToLower();

                    foreach (var element in doc.RootElement.EnumerateArray())
                    {
                        string location = element.GetProperty("location").GetString()?.ToLower() ?? "";
                        string country = element.GetProperty("country_name").GetString()?.ToLower() ?? "";

                        if (location.Contains(cleanName) || country.Contains(cleanName))
                        {
                            sessionKey = element.GetProperty("session_key").GetInt32();
                            break;
                        }
                    }
                }
            }

            // 2. Запитуємо погоду для ЗНАЙДЕНОЇ сесії конкретного треку
            var response = await _httpClient.GetAsync($"weather?session_key={sessionKey}");
            if (!response.IsSuccessStatusCode) return null;

            string json = await response.Content.ReadAsStringAsync();
            var weatherList = JsonSerializer.Deserialize<List<OpenF1Weather>>(json);

            return weatherList?.LastOrDefault();
        }
        catch
        {
            return null;
        }
    }
}