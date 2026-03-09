using System.Globalization;

namespace F1Predictor.ML
{
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

            var rawRaces = File.ReadAllLines(racesPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new RawRace
                {
                    RaceId = float.Parse(parts[0], CultureInfo.InvariantCulture),
                    CircuitId = float.Parse(parts[3], CultureInfo.InvariantCulture)
                })
                .ToList();

            var joinedData = from result in rawResults
                             join race in rawRaces on result.RaceId equals race.RaceId
                             select new RaceData
                             {
                                 DriverId = result.DriverId,
                                 ConstructorId = result.ConstructorId,
                                 Grid = result.Grid,
                                 PositionOrder = result.PositionOrder,
                                 CircuitId = race.CircuitId
                             };

            return joinedData.ToList();
        }
        public static List<float> GetDriverRecentResults(float driverId, string resultsPath, int count = 10)
        {
            var history = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => float.Parse(parts[2], CultureInfo.InvariantCulture) == driverId)
                .Select(parts => new 
                {
                    RaceId = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    Position = float.Parse(parts[8], CultureInfo.InvariantCulture) 
                })
                .OrderByDescending(x => x.RaceId) 
                .Take(count) 
                .Select(x => x.Position)
                .Reverse() 
                .ToList();

            return history;
        }
        public static List<float> GetDriverResultsAtCircuit(float driverId, float circuitId, string resultsPath, string racesPath)
        {
            var racesAtCircuit = File.ReadAllLines(racesPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Select(parts => new 
                {
                    RaceId = float.Parse(parts[0], CultureInfo.InvariantCulture),
                    CircuitId = float.Parse(parts[3], CultureInfo.InvariantCulture),
                    Year = int.Parse(parts[1])
                })
                .Where(r => r.CircuitId == circuitId)
                .ToList();

            var raceIds = new HashSet<float>(racesAtCircuit.Select(r => r.RaceId));

            var history = File.ReadAllLines(resultsPath)
                .Skip(1)
                .Select(line => line.Split(','))
                .Where(parts => 
                {
                    float rId = float.Parse(parts[1], CultureInfo.InvariantCulture);
                    float dId = float.Parse(parts[2], CultureInfo.InvariantCulture);
                
                    return dId == driverId && raceIds.Contains(rId);
                })
                .Select(parts => new
                {
                    RaceId = float.Parse(parts[1], CultureInfo.InvariantCulture),
                    Position = float.Parse(parts[8], CultureInfo.InvariantCulture)
                })
                .OrderBy(x => x.RaceId)
                .Select(x => x.Position)
                .ToList();

            return history;
        }
    }
    
}