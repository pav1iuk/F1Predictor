using System.Collections.Generic;

namespace F1Predictor.Core
{
    public class TyreStrategyService
    {
        /// <summary>
        /// Симулює падіння темпу та знос гуми протягом гонки.
        /// tyreType: 1 - Soft, 2 - Medium, 3 - Hard
        /// trackTemp: температура асфальту (впливає на знос)
        /// </summary>
        public List<TyreLapData> CalculateDegradation(int tyreType, float trackTemp, int circuitType,
            int totalLaps = 50)
        {
            var result = new List<TyreLapData>();

            // Коефіцієнт абразивності траси
            double circuitFactor = circuitType switch
            {
                1 => 0.8, // Міська (напр. Монако) — знос нижчий на 20%
                2 => 1.0, // Стаціонарна (стандарт)
                3 => 1.3, // Абразивна (напр. Спа/Барселона) — знос вищий на 30%
                _ => 1.0
            };

            double maxLapsLifetime = tyreType switch
            {
                0 => 18.0, // Soft
                1 => 30.0, // Medium
                2 => 45.0, // Hard
                _ => 30.0
            };

            double tempFactor = trackTemp > 35.0f ? 1.25 : 1.0;
            // Підсумковий ресурс шин з урахуванням температури ТА траси
            double effectiveLifetime = maxLapsLifetime / (tempFactor * circuitFactor);

            double currentHealth = 100.0;
            double currentLapPenalty = 0.0;

            for (int lap = 1; lap <= totalLaps; lap++)
            {
                double lapWearPercent = 100.0 / effectiveLifetime;
                currentHealth = System.Math.Max(0, currentHealth - lapWearPercent);

                if (currentHealth > 50.0)
                    currentLapPenalty += 0.05 * tempFactor * circuitFactor;
                else if (currentHealth > 20.0)
                    currentLapPenalty += 0.15 * tempFactor * circuitFactor;
                else
                    currentLapPenalty += 0.50 * tempFactor * circuitFactor;

                result.Add(new TyreLapData
                {
                    LapNumber = lap,
                    LapTimePenalty = System.Math.Round(currentLapPenalty, 2),
                    TyreHealth = System.Math.Round(currentHealth, 1)
                });
            }

            return result;
        }
    }
}