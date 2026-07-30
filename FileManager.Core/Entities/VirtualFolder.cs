namespace FileManager.Core.Entities;

public class VirtualFolder
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public Guid? ParentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
