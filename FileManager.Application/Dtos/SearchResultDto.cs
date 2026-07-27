namespace FileManager.Application.Dtos;

/// <summary>
/// Search result shaped for direct consumption by the UI. Distinct from
/// <see cref="FileResultDto"/> which is a generic file representation used
/// by other features (e.g. duplicates) and should not be coupled to the
/// search UI's specific contract.
/// </summary>
public class SearchResultDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string Path { get; set; } = "";

    public long Size { get; set; }

    public string Extension { get; set; } = "";

    public DateTime Modified { get; set; }

    public string Type { get; set; } = "";

    public string Hash { get; set; } = "";
}
