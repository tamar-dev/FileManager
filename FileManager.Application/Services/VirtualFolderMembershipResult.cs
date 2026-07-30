namespace FileManager.Application.Services;

public class VirtualFolderMembershipResult
{
    public bool Success { get; set; }

    public string? ErrorMessage { get; set; }

    public bool IsConflict { get; set; }

    public bool IsNotFound { get; set; }
}
