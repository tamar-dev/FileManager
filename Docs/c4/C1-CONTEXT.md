# C1 - System Context Diagram

## Purpose
Shows how FileManager fits into the user's environment and interacts with external systems.

## Diagram

```mermaid
graph TB
    User([User])
    FileManager[FileManager<br/>Local desktop file<br/>management application]
    FileSystem[(Windows File System<br/>Physical files)]

    User -->|Organizes files via CLI| FileManager
    FileManager -->|Reads file metadata| FileSystem
    FileManager -->|Monitors changes| FileSystem

    style FileManager fill:#1168bd,stroke:#0b4884,color:#ffffff
    style FileSystem fill:#999999,stroke:#6b6b6b,color:#ffffff
    style User fill:#08427b,stroke:#052e56,color:#ffffff
```

## Elements

### User
Desktop user who wants to organize their local files without changing physical file locations.

### FileManager
Local desktop file management application that provides:
- Virtual folder organization
- Collections and albums
- Tag and metadata management
- Timeline-based browsing
- Duplicate detection
- Thumbnail generation
- Future AI enrichment

### Windows File System
The native filesystem where physical files reside.
FileManager reads file metadata and monitors changes but never modifies, moves, renames, or
deletes any physical file.

## Key Principles
- The physical filesystem is the source of truth for physical files
- The Metadata Store is the source of truth for logical organization and application metadata
- FileManager never modifies physical files — it indexes and organizes them virtually
