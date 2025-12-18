using System.Globalization;

namespace F1Predictor.ML
{
    // Тимчасові класи для читання "сирих" CSV
    public class RawResult
    {
        public float RaceId { get; set; }
        public float DriverId { get; set; }
        public float ConstructorId { get; set; }
        public float Grid { get; set; }
        public float PositionOrder { get; set; }
    }

    public class RawRace
    {
        public float RaceId { get; set; }
        public float CircuitId { get; set; }
    }

    public static class DataProcessor
    {
        public static List<RaceData> LoadAndJoinData(string resultsPath, string racesPath)
        {
            // 1. Читаємо RESULTS.CSV
            // results.csv: resultId(0), raceId(1), driverId(2), constructorId(3), ..., grid(5), ..., positionOrder(8)
            var rawResults = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new RawResult
                {
                    RaceId = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    DriverId = float.Parse(parts[2], CultureInfo.InvariantCulture),
                    ConstructorId = float.Parse(parts[3], CultureInfo.InvariantCulture),
                    Grid = float.Parse(parts[5], CultureInfo.InvariantCulture),
                    PositionOrder = float.Parse(parts[8], CultureInfo.InvariantCulture)
                })
                .ToList();

            // 2. Читаємо RACES.CSV
            // races.csv: raceId(0), year(1), round(2), circuitId(3)...
            var rawRaces = File.ReadAllLines(racesPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new RawRace
                {
                    RaceId = float.Parse(parts[0], CultureInfo.InvariantCulture),
                    CircuitId = float.Parse(parts[3], CultureInfo.InvariantCulture)
                })
                .ToList();

            // 3. РОБИМО JOIN (Магія LINQ)
            // З'єднуємо результати з трасами по спільному полю RaceId
            var joinedData = from result in rawResults
                             join race in rawRaces on result.RaceId equals race.RaceId
                             select new RaceData
                             {
                                 DriverId = result.DriverId,
                                 ConstructorId = result.ConstructorId,
                                 Grid = result.Grid,
                                 PositionOrder = result.PositionOrder,
                                 CircuitId = race.CircuitId // <--- Ось тут ми дістали трасу!
                             };

            return joinedData.ToList();
        }
        public static List<float> GetDriverRecentResults(float driverId, string resultsPath, int count = 10)
        {
            // Читаємо файл, шукаємо нашого водія
            var history = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => float.Parse(parts[2], CultureInfo.InvariantCulture) == driverId) // parts[2] = driverId
                .Select(parts => new 
                {
                    RaceId = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    Position = float.Parse(parts[8], CultureInfo.InvariantCulture) // parts[8] = positionOrder
                })
                .OrderByDescending(x => x.RaceId) // Сортуємо: від нових до старих
                .Take(count) // Беремо останні 10
                .Select(x => x.Position)
                .Reverse() // Розвертаємо, щоб на графіку було зліва направо (старі -> нові)
                .ToList();

            return history;
        }
        // Додай у клас DataProcessor або CsvDataLoader
        public static List<float> GetDriverResultsAtCircuit(float driverId, float circuitId, string resultsPath, string racesPath)
        {
            // 1. Читаємо всі гонки і фільтруємо ті, що були на потрібній ТРАСІ
            var racesAtCircuit = File.ReadAllLines(racesPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new 
                {
                    RaceId = float.Parse(parts[0], CultureInfo.InvariantCulture),
                    CircuitId = float.Parse(parts[3], CultureInfo.InvariantCulture),
                    Year = int.Parse(parts[1]) // Можна взяти рік для сортування
                })
                .Where(r => r.CircuitId == circuitId) // Фільтр по трасі
                .ToList();

            // Робимо список ID гонок, щоб швидко шукати
            var raceIds = new HashSet<float>(racesAtCircuit.Select(r => r.RaceId));

            // 2. Читаємо результати і беремо тільки ті, що співпадають з нашими гонками + водієм
            var history = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => 
                {
                    float rId = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float dId = float.Parse(parts[2], CultureInfo.InvariantCulture);
                
                    // Перевіряємо: чи це наш водій І чи ця гонка була на нашій трасі
                    return dId == driverId && raceIds.Contains(rId);
                })
                .Select(parts => new
                {
                    RaceId = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    Position = float.Parse(parts[8], CultureInfo.InvariantCulture)
                })
                .OrderBy(x => x.RaceId) // Від старих до нових
                .Select(x => x.Position)
                .ToList();

            return history;
        }
    }
    
}