using F1Predictor.Core; // Додай цей using (Rider підкаже Alt+Enter)

namespace F1Predictor.Data
{
    public class CsvDataLoader
    {
        public static List<Driver> LoadDrivers(string path)
        {
            var list = new List<Driver>();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                // Обережно: в CSV бувають коми всередині імен, але поки віримо, що структура проста
                string fName = parts[4].Trim('"'); 
                string lName = parts[5].Trim('"');

                list.Add(new Driver
                {
                    DriverId = float.Parse(parts[0]),
                    FullName = $"{fName} {lName}" 
                });
            }
            
            // СОРТУВАННЯ: Спочатку по прізвищу, потім по імені
            return list.OrderBy(d => d.FullName).ToList();
        }

        public static List<Team> LoadTeams(string path)
        {
            var list = new List<Team>();
            var lines = File.ReadAllLines(path);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                
                list.Add(new Team
                {
                    ConstructorId = float.Parse(parts[0]),
                    Name = parts[2].Trim('"')
                });
            }

            // СОРТУВАННЯ: По назві команди
            return list.OrderBy(t => t.Name).ToList();
        }
        public static List<Circuit> LoadCircuits(string path)
        {
            var list = new List<Circuit>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1))
            {
                // CSV парсинг може бути складним через коми в назвах, 
                // але для базового варіанту circuits.csv Kaggle:
                // circuitId(0), circuitRef(1), name(2), location(3)...
                var parts = line.Split(',');

                list.Add(new Circuit
                {
                    CircuitId = float.Parse(parts[0]),
                    Name = parts[2].Trim('"'),
                    Location = parts[3].Trim('"')
                });
            }
            return list.OrderBy(c => c.Name).ToList();
        }
        public static List<PitStop> LoadPitStops(string path)
        {
            var list = new List<PitStop>();
            // pit_stops.csv: raceId(0), driverId(1), stop(2), lap(3), time(4), duration(5), milliseconds(6)
            var lines = File.ReadAllLines(path).Skip(1);
    
            foreach (var line in lines)
            {
                var parts = line.Split(',');
                // Інколи бувають помилки в CSV, тому try-catch
                try 
                {
                    list.Add(new PitStop
                    {
                        RaceId = int.Parse(parts[0]),
                        DriverId = int.Parse(parts[1]),
                        Milliseconds = int.Parse(parts[6])
                    });
                }
                catch { /* ігноруємо биті рядки */ }
            }
            return list;
        }
        public static List<Qualifying> LoadQualifying(string path)
        {
            var list = new List<Qualifying>();
            // qualifying.csv: qualifyId(0), raceId(1), driverId(2), ..., position(5)
            var lines = File.ReadAllLines(path).Skip(1);

            foreach (var line in lines)
            {
                var parts = line.Split(',');
                try
                {
                    list.Add(new Qualifying
                    {
                        RaceId = int.Parse(parts[1]),
                        DriverId = int.Parse(parts[2]),
                        Position = int.Parse(parts[5])
                    });
                }
                catch { }
            }
            return list;
        }
    }
}