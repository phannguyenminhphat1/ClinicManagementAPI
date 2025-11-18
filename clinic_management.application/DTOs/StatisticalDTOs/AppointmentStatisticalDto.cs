using System.Text.Json.Serialization;

public class AppointmentStatisticalDto
{
    [JsonPropertyName("summary")]
    public SummaryDto? Summary { get; set; }

    [JsonPropertyName("chart")]
    public List<ChartDto>? Chart { get; set; }
}

public class ChartDto
{
    [JsonPropertyName("labels")]
    public List<string>? Labels { get; set; }

    [JsonPropertyName("datasets")]
    public List<DatasetsDto>? Datasets { get; set; }

    [JsonPropertyName("details")]
    public List<Details>? Details { get; set; }
}

public class DatasetsDto
{
    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("data")]
    public List<int>? Data { get; set; }
}

public class SummaryDto
{
    [JsonPropertyName("total_appointments")]
    public int TotalAppointments { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("pending")]
    public int Pending { get; set; }

    [JsonPropertyName("cancelled")]
    public int Cancelled { get; set; }

}


public class Details
{

    [JsonPropertyName("date")]
    public string? Date { get; set; }

    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("completed")]
    public int Completed { get; set; }

    [JsonPropertyName("pending")]
    public int Pending { get; set; }

    [JsonPropertyName("examining")]
    public int Examining { get; set; }

    [JsonPropertyName("cancelled")]
    public int Cancelled { get; set; }

}