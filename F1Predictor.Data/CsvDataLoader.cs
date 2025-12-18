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
    }
}