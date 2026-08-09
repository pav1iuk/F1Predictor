using System.Text.Json.Serialization;

namespace F1Predictor.Data;

public class OpenF1Meeting
{
    [JsonPropertyName("meeting_key")]
    public int MeetingKey { get; set; }

    [JsonPropertyName("meeting_name")]
    public string MeetingName { get; set; } = string.Empty;

    [JsonPropertyName("country_name")]
    public string CountryName { get; set; } = string.Empty;

    [JsonPropertyName("year")]
    public int Year { get; set; }
}

public class OpenF1Driver
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("name_acronym")]
    public string NameAcronym { get; set; } = string.Empty;

    [JsonPropertyName("team_name")]
    public string TeamName { get; set; } = string.Empty;

    [JsonPropertyName("team_colour")]
    public string TeamColour { get; set; } = string.Empty;

    [JsonPropertyName("headshot_url")]
    public string HeadshotUrl { get; set; } = string.Empty;

    [JsonPropertyName("session_key")]
    public int SessionKey { get; set; }
}

public class OpenF1Position
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("position")]
    public int Position { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}
public class OpenF1CarData
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("speed")]
    public int Speed { get; set; }

    [JsonPropertyName("rpm")]
    public int Rpm { get; set; }

    [JsonPropertyName("n_gear")]
    public int NGear { get; set; }

    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
}
public class OpenF1Stint
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("stint_number")]
    public int StintNumber { get; set; }

    [JsonPropertyName("compound")]
    public string Compound { get; set; } = string.Empty;

    [JsonPropertyName("tyre_age_at_start")]
    public int TyreAgeAtStart { get; set; }

    [JsonPropertyName("laps_stint")]
    public int LapsStint { get; set; }
}
public class OpenF1Lap
{
    [JsonPropertyName("driver_number")]
    public int DriverNumber { get; set; }

    [JsonPropertyName("lap_number")]
    public int LapNumber { get; set; }

    [JsonPropertyName("lap_duration")]
    public double? LapDuration { get; set; }

    [JsonPropertyName("is_pit_out_lap")]
    public bool IsPitOutLap { get; set; }
}
public class OpenF1Weather
{
    [JsonPropertyName("air_temperature")]
    public double AirTemperature { get; set; }

    [JsonPropertyName("track_temperature")]
    public double TrackTemperature { get; set; }

    [JsonPropertyName("humidity")]
    public double Humidity { get; set; }

    [JsonPropertyName("rainfall")]
    public int Rainfall { get; set; } // 0 = Сухо, 1 = Дощ
}