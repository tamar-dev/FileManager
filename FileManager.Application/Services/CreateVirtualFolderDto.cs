namespace FileManager.Application.Dtos;

public class CreateVirtualFolderDto
{
    public string Name { get; set; } = string.Empty;

    public Guid? ParentId { get; set; }
}