using Microsoft.AspNetCore.Mvc;

public class AppointmentFilterDto
{
    [FromQuery(Name = "start_day")]
    public string? StartDay { get; set; }

    [FromQuery(Name = "end_day")]
    public string? EndDay { get; set; }
}