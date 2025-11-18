using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

public class PaginationDto
{
    [FromQuery(Name = "page")]
    // [Range(1, int.MaxValue, ErrorMessage = CommonMessages.PAGE_NUMBER_MUST_BE_GREATER_THAN_0)]
    public string? Page { get; set; }

    [FromQuery(Name = "page_size")]
    // [Range(1, int.MaxValue, ErrorMessage = CommonMessages.PAGE_SIZE_MUST_BE_GREATER_THAN_0)]
    public string? PageSize { get; set; }
}
