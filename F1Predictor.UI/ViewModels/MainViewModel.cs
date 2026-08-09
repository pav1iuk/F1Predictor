using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using F1Predictor.Core;
using F1Predictor.Data;
using F1Predictor.ML;
using LiveCharts;
using LiveCharts.Wpf;

namespace F1Predictor.UI.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty] private List<Driver> _drivers;
        [ObservableProperty] private List<Team> _teams;
        [ObservableProperty] private List<Circuit> _circuits;

        private Driver _selectedDriver;
        public Driver SelectedDriver
        {
            get => _selectedDriver;
            set
            {
                if (SetProperty(ref _selectedDriver, value))
                {
                    UpdateDriverMedia();
                }
            }
        }

        private Team _selectedTeam;
        public Team SelectedTeam
        {
            get => _selectedTeam;
            set
            {
                if (SetProperty(ref _selectedTeam, value))
                {
                    UpdateTeamMedia();
                }
            }
        }

        private Circuit _selectedCircuit;
        public Circuit SelectedCircuit
        {
            get => _selectedCircuit;
            set
            {
                if (SetProperty(ref _selectedCircuit, value))
                {
                    UpdateCircuitMedia();
                }
            }
        }

        [ObservableProperty] private int _gridPosition = 1;
        [ObservableProperty] private bool _isRainMode;
        [ObservableProperty] private bool _isLoading;
        [ObservableProperty] private string _predictionResultText;

        // Параметри для ШІ та шин
        [ObservableProperty] private int _selectedTyreType = 1; // 0 - Soft, 1 - Medium, 2 - Hard
        [ObservableProperty] private float _trackTemperature = 30.0f;
        [ObservableProperty] private int _selectedCircuitType = 1;

        [ObservableProperty] private List<TyreLapData> _tyreDegradationData;
        [ObservableProperty] private string _pitWindowRecommendation;
        [ObservableProperty] private SeriesCollection _tyreSeriesCollection;
        [ObservableProperty] private string[] _lapLabels;

        // Медіа-шляхи для зображень в UI
        [ObservableProperty] private string _driverPhotoPath;
        [ObservableProperty] private string _teamLogoPath;
        [ObservableProperty] private string _circuitImagePath;

        private ModelPredictor _predictor;

        public MainViewModel()
        {
            _ = LoadDataAsync();
        }

        [RelayCommand]
        private async Task LoadDataAsync()
        {
            IsLoading = true;

            try
            {
                await Task.Run(() =>
                {
                    Drivers = CsvDataLoader.LoadDrivers("drivers.csv");
                    Teams = CsvDataLoader.LoadTeams("teams.csv");
                    Circuits = CsvDataLoader.LoadCircuits("circuits.csv");

                    _predictor = new ModelPredictor();
                });

                if (Drivers?.Count > 0) SelectedDriver = Drivers[0];
                if (Teams?.Count > 0) SelectedTeam = Teams[0];
                if (Circuits?.Count > 0) SelectedCircuit = Circuits[0];

                PredictionResultText = "Готово до прогнозу";
            }
            catch (Exception ex)
            {
                PredictionResultText = $"Помилка: {ex.Message}";
            }

            IsLoading = false;
        }

        private void UpdateDriverMedia()
        {
            if (SelectedDriver == null) return;
            DriverPhotoPath = $"pack://application:,,,/Images/Drivers/{SelectedDriver.DriverId}.png";
        }

        private void UpdateTeamMedia()
        {
            if (SelectedTeam == null) return;
            TeamLogoPath = $"pack://application:,,,/Images/Teams/{SelectedTeam.ConstructorId}.png";
        }

        private void UpdateCircuitMedia()
        {
            if (SelectedCircuit == null) return;
            CircuitImagePath = $"pack://application:,,,/Images/Circuits/{SelectedCircuit.CircuitId}.png";
        }

        [RelayCommand]
        private void Predict()
        {
            if (SelectedDriver == null || SelectedTeam == null || SelectedCircuit == null)
            {
                PredictionResultText = "Оберіть усі параметри!";
                return;
            }

            if (_predictor == null)
            {
                PredictionResultText = "ШІ завантажується...";
                return;
            }

            float mlTyreType = SelectedTyreType + 1;
            float rawPrediction = _predictor.Predict(
                (float)SelectedDriver.DriverId, 
                (float)SelectedTeam.ConstructorId, 
                (float)GridPosition, 
                (float)SelectedCircuit.CircuitId,
                mlTyreType,
                TrackTemperature,
                (float)SelectedCircuitType
            );

            double strategyPenalty = 0.0;

            if (IsRainMode)
            {
                strategyPenalty += 2.0;
            }

            if (SelectedTyreType == 0 && TrackTemperature > 38.0f) 
            {
                strategyPenalty += 2.5; 
            }
            else if (SelectedTyreType == 0) 
            {
                strategyPenalty -= 0.8; 
            }
            else if (SelectedTyreType == 2) 
            {
                strategyPenalty += 0.5; 
            }

            double finalPosition = Math.Clamp(rawPrediction + strategyPenalty, 1.0, 20.0);

            PredictionResultText = $"P{finalPosition:F1}";

            CalculateTyreStrategy();
        }

        [RelayCommand]
        private void CalculateTyreStrategy()
        {
            var strategyService = new TyreStrategyService();

            TyreDegradationData = strategyService.CalculateDegradation(SelectedTyreType, TrackTemperature, SelectedCircuitType, 50);

            var penaltyValues = new ChartValues<double>();
            var labels = new List<string>();

            foreach (var item in TyreDegradationData)
            {
                penaltyValues.Add(item.LapTimePenalty);
                labels.Add(item.LapNumber.ToString());
            }

            string tyreName = SelectedTyreType switch { 0 => "Soft", 1 => "Medium", 2 => "Hard", _ => "" };

            TyreSeriesCollection = new SeriesCollection
            {
                new LineSeries
                {
                    Title = $"Темп {tyreName} (сек/коло)",
                    Values = penaltyValues,
                    PointGeometry = DefaultGeometries.Circle,
                    PointGeometrySize = 6
                }
            };

            LapLabels = labels.ToArray();

            var pitLap = TyreDegradationData.Find(d => d.TyreHealth <= 25.0)?.LapNumber ?? 20;
            PitWindowRecommendation = $"Рекомендоване вікно піт-стопу для {tyreName}: {pitLap - 1} – {pitLap + 2} коло (Залишок ресурсу ~25%)";
        }
    }
}