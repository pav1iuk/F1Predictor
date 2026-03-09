using System.Collections.Generic;

namespace F1Predictor.UI
{
    public static class ImageHelper
    {
        // Офіційні заглушки з сервера Formula 1
        private const string DefaultDriver = "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/driver_fallback_image.png";
        private const string DefaultTeam = "https://media.formula1.com/content/dam/fom-website/teams/2025/team-logo-fallback.png"; // Або лого F1
        private const string DefaultCircuit = "https://media.formula1.com/image/upload/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Monaco_Circuit.png";

        // 1. СЛОВНИК ПІЛОТІВ
        private static readonly Dictionary<string, string> Drivers = new Dictionary<string, string>
        {
            // --- СЕЗОН 2024/2025 (Актуальні фото) ---
            { "verstappen", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/M/MAXVER01_Max_Verstappen/maxver01.png.transform/2col/image.png" },
            { "perez", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/S/SERPER01_Sergio_Perez/serper01.png.transform/2col/image.png" },
            { "hamilton", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/LEWHAM01_Lewis_Hamilton/lewham01.png.transform/2col/image.png" },
            { "russell", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/G/GEORUS01_George_Russell/georus01.png.transform/2col/image.png" },
            { "leclerc", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/C/CHALEC01_Charles_Leclerc/chalec01.png.transform/2col/image.png" },
            { "sainz", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/C/CARSAI01_Carlos_Sainz/carsai01.png.transform/2col/image.png" },
            { "norris", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/LANNOR01_Lando_Norris/lannor01.png.transform/2col/image.png" },
            { "piastri", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/O/OSCPIA01_Oscar_Piastri/oscpia01.png.transform/2col/image.png" },
            { "alonso", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/F/FERALO01_Fernando_Alonso/feralo01.png.transform/2col/image.png" },
            { "stroll", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/LANSTR01_Lance_Stroll/lanstr01.png.transform/2col/image.png" },
            { "gasly", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/P/PIEGAS01_Pierre_Gasly/piegas01.png.transform/2col/image.png" },
            { "ocon", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/E/ESTOCO01_Esteban_Ocon/estoco01.png.transform/2col/image.png" },
            { "albon", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/A/ALEALB01_Alexander_Albon/alealb01.png.transform/2col/image.png" },
            { "tsunoda", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/Y/YUKTSU01_Yuki_Tsunoda/yuktsu01.png.transform/2col/image.png" },
            { "bottas", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/V/VALBOT01_Valtteri_Bottas/valbot01.png.transform/2col/image.png" },
            { "zhou", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/G/GUAZHO01_Guanyu_Zhou/guazho01.png.transform/2col/image.png" },
            { "hulkenberg", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/N/NICHUL01_Nico_Hulkenberg/nichul01.png.transform/2col/image.png" },
            { "magnussen", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/K/KEVMAG01_Kevin_Magnussen/kevmag01.png.transform/2col/image.png" },
            { "ricciardo", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/D/DANRIC01_Daniel_Ricciardo/danric01.png.transform/2col/image.png" },
            
            // --- НОВАЧКИ / ЗАМІНИ (Rookies) ---
            { "bearman", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/O/OLLBEA01_Oliver_Bearman/ollbea01.png.transform/2col/image.png" },
            { "colapinto", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/F/FRACOL01_Franco_Colapinto/fracol01.png.transform/2col/image.png" },
            { "lawson", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/L/LIALAW01_Liam_Lawson/lialaw01.png.transform/2col/image.png" },
            { "antonelli", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/K/KIMANT01_Kimi_Antonelli/kimant01.png.transform/2col/image.png" },
            { "doohan", "https://media.formula1.com/d_driver_fallback_image.png/content/dam/fom-website/drivers/J/JACDOO01_Jack_Doohan/jacdoo01.png.transform/2col/image.png" },
            
            // --- ЛЕГЕНДИ (Використовуємо офіційну заглушку F1, бо архівних фото немає на цьому сервері в такому форматі) ---
            // Це гарантує, що картинка завантажиться і буде виглядати акуратно (силует шолома)
            { "schumacher", DefaultDriver },
            { "vettel", DefaultDriver },
            { "raikkonen", DefaultDriver },
            { "rosberg", DefaultDriver },
            { "button", DefaultDriver },
            { "massa", DefaultDriver },
            { "webber", DefaultDriver },
            { "senna", DefaultDriver },
            { "prost", DefaultDriver },
            { "mansell", DefaultDriver },
            { "lauda", DefaultDriver },
        };

        // 2. СЛОВНИК КОМАНД
        private static readonly Dictionary<string, string> Teams = new Dictionary<string, string>
        {
            // --- АКТУАЛЬНІ 2024/2025 ---
            { "red bull", "https://media.formula1.com/content/dam/fom-website/teams/2024/red-bull-racing-logo.png.transform/2col/image.png" },
            { "mercedes", "https://media.formula1.com/content/dam/fom-website/teams/2024/mercedes-logo.png.transform/2col/image.png" },
            { "ferrari", "https://media.formula1.com/content/dam/fom-website/teams/2024/ferrari-logo.png.transform/2col/image.png" },
            { "mclaren", "https://media.formula1.com/content/dam/fom-website/teams/2024/mclaren-logo.png.transform/2col/image.png" },
            { "aston martin", "https://media.formula1.com/content/dam/fom-website/teams/2024/aston-martin-logo.png.transform/2col/image.png" },
            { "alpine", "https://media.formula1.com/content/dam/fom-website/teams/2024/alpine-logo.png.transform/2col/image.png" },
            { "williams", "https://media.formula1.com/content/dam/fom-website/teams/2024/williams-logo.png.transform/2col/image.png" },
            { "haas", "https://media.formula1.com/content/dam/fom-website/teams/2024/haas-f1-team-logo.png.transform/2col/image.png" },
            { "sauber", "https://media.formula1.com/content/dam/fom-website/teams/2024/kick-sauber-logo.png.transform/2col/image.png" },
            { "rb", "https://media.formula1.com/content/dam/fom-website/teams/2024/rb-logo.png.transform/2col/image.png" },
            
            // --- ІСТОРИЧНІ / ІНШІ НАЗВИ (Мапимо на актуальні або заглушку) ---
            { "alfa romeo", "https://media.formula1.com/content/dam/fom-website/teams/2023/alfa-romeo-logo.png.transform/2col/image.png" }, 
            { "alphatauri", "https://media.formula1.com/content/dam/fom-website/teams/2023/alphatauri-logo.png.transform/2col/image.png" },
            { "toro rosso", "https://media.formula1.com/content/dam/fom-website/teams/2018/toro-rosso-logo.png.transform/2col/image.png" },
            { "racing point", "https://media.formula1.com/content/dam/fom-website/teams/2020/racing-point-logo.png.transform/2col/image.png" },
            { "renault", "https://media.formula1.com/content/dam/fom-website/teams/2019/renault-logo.png.transform/2col/image.png" },
            { "force india", DefaultTeam },
            { "lotus", DefaultTeam },
            { "manor", DefaultTeam },
        };

        // 3. СЛОВНИК ТРАС
        private static readonly Dictionary<string, string> Circuits = new Dictionary<string, string>
        {
            { "monaco", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244984/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Monaco_Circuit.png" },
            { "monza", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244987/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Italy_Circuit.png" },
            { "silverstone", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Great_Britain_Circuit.png" },
            { "spa", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244982/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Belgium_Circuit.png" },
            { "suzuka", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Japan_Circuit.png" },
            { "interlagos", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244981/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Brazil_Circuit.png" },
            { "bahrain", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Bahrain_Circuit.png" },
            { "jeddah", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Saudi_Arabia_Circuit.png" },
            { "albert park", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Australia_Circuit.png" },
            { "baku", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244987/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Azerbaijan_Circuit.png" },
            { "miami", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Miami_Circuit.png" },
            { "imola", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244986/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Emilia_Romagna_Circuit.png" },
            { "gilles villeneuve", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Canada_Circuit.png" },
            { "barcelona", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244986/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Spain_Circuit.png" },
            { "red bull ring", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244987/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Austria_Circuit.png" },
            { "hungaroring", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244987/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Hungary_Circuit.png" },
            { "zandvoort", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Netherlands_Circuit.png" },
            { "marina bay", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244986/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Singapore_Circuit.png" },
            { "austin", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/USA_Circuit.png" },
            { "mexico", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Mexico_Circuit.png" },
            { "las vegas", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Las_Vegas_Circuit.png" },
            { "yas marina", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Abu_Dhabi_Circuit.png" },
            { "shanghai", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/China_Circuit.png" },
            { "lusail", "https://media.formula1.com/image/upload/f_auto/q_auto/v1677244985/content/dam/fom-website/2018-redesign-assets/Circuit%20maps%2016x9/Qatar_Circuit.png" },
        };

        // --- МЕТОДИ ПОШУКУ ---

        public static string GetDriverUrl(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return DefaultDriver;
            var nameLower = fullName.ToLower();
            
            foreach (var kvp in Drivers)
            {
                if (nameLower.Contains(kvp.Key)) return kvp.Value;
            }
            return DefaultDriver;
        }

        public static string GetTeamUrl(string teamName)
        {
            if (string.IsNullOrEmpty(teamName)) return DefaultTeam;
            var nameLower = teamName.ToLower();

            foreach (var kvp in Teams)
            {
                if (nameLower.Contains(kvp.Key)) return kvp.Value;
            }
            return DefaultTeam;
        }

        public static string GetCircuitUrl(string circuitName)
        {
            if (string.IsNullOrEmpty(circuitName)) return DefaultCircuit;
            var nameLower = circuitName.ToLower();

            foreach (var kvp in Circuits)
            {
                if (nameLower.Contains(kvp.Key)) return kvp.Value;
            }
            return DefaultCircuit;
        }
    }
}