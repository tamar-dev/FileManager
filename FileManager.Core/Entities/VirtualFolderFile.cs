namespace FileManager.Core.Entities;

public class VirtualFolderFile
{
    public Guid Id { get; set; }

    public Guid VirtualFolderId { get; set; }

    public Guid FileEntryId { get; set; }

    public DateTime CreatedAt { get; set; }
}
