namespace FileManager.Application.Dtos;

public class VirtualFolderDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid? ParentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public List<VirtualFolderDto> Children { get; set; } = [];
}
