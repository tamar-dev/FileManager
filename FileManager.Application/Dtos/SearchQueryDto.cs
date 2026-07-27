namespace FileManager.Application.Dtos;

public class SearchQueryDto
{
    public string? Name { get; set; }

    public string? Extension { get; set; }

    public string? Path { get; set; }

    public DateTime? ModifiedAfter { get; set; }

    public DateTime? ModifiedBefore { get; set; }
}
