using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;


public partial class SpecialtyDto
{
    [JsonPropertyName("specialty_id")]
    public int SpecialtyId { get; set; }

    [JsonPropertyName("specialty_name")]
    public string SpecialtyName { get; set; } = null!;

}
