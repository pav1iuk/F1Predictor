using F1Predictor.Core;

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
                string fName = parts[4].Trim('"'); 
                string lName = parts[5].Trim('"');

                list.Add(new Driver
                {
                    DriverId = float.Parse(parts[0]),
                    FullName = $"{fName} {lName}" 
                });
            }

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
            
            return list.OrderBy(t => t.Name).ToList();
        }
        public static List<Circuit> LoadCircuits(string path)
        {
            var list = new List<Circuit>();
            var lines = File.ReadAllLines(path);
            foreach (var line in lines.Skip(1))
            {
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
            var lines = File.ReadAllLines(path).Skip(1);
    
            foreach (var line in lines)
            {
                var parts = line.Split(',');
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