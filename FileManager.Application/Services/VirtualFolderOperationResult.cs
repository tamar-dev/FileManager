using FileManager.Application.Dtos;

namespace FileManager.Application.Services;

public class VirtualFolderOperationResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsConflict { get; set; }

    public bool IsNotFound { get; set; }

    public VirtualFolderDto? Folder { get; set; }
}
