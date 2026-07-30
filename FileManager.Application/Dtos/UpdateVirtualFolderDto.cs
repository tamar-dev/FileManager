namespace FileManager.Application.Dtos;

public class UpdateVirtualFolderDto
{
    public string? Name { get; set; }

    public Guid? ParentId { get; set; }

    public bool ParentIdSpecified { get; set; }
}
