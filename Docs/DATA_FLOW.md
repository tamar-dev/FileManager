# FileManager — Data Flow

> **Constraint:** Unless explicitly stated otherwise, every flow described below is read-only with
> respect to the physical filesystem.
>
> FileManager may update its own index, metadata, and virtual organization in SQLite, but it must not
> automatically move, rename, delete, overwrite, or otherwise modify physical user files.

---

## Runtime Overview

The current application has two hosts:

```text
React Client
    |
    | HTTP / JSON
    v
FileManager.Api
    |
    v
FileManager.Application
    |
    v
FileManager.Core
    ^
    |
FileManager.Infrastructure
````

And:

```text
FileManager.Cli
    |
    v
FileManager.Application
    |
    v
FileManager.Core
    ^
    |
FileManager.Infrastructure
```

`FileManager.Api` and `FileManager.Cli` reuse the same Application and Infrastructure registrations.

---

## API Startup

```text
Start FileManager.Api
  -> Register AddFileManagerInfrastructure()
  -> Register AddFileManagerApplication()
  -> Run pending database migrations
  -> Expose HTTP endpoints
```

The API is the HTTP boundary for the React client.

It does not contain core business logic.

---

## Initial Indexing

Initial indexing begins from a user-selected physical root.

```text
Caller
  -> IIndexingAppService / indexing orchestration
  -> IIndexSource.EnumeratePaths(rootPath)
  -> InitialIndexingService
  -> FileEntryFactory.CreateAsync(path)
  -> IFileRepository.UpsertBatchAsync(...)
  -> SQLite
```

Filesystem interaction:

```text
IIndexSource
  -> reads physical directory structure

FileEntryFactory
  -> reads file metadata
  -> may read file content to calculate SHA-256
```

Database interaction:

```text
IFileRepository
  -> inserts or updates FileEntry metadata
```

No physical file is modified.

---

## Indexed Location Flow

A user may register a physical directory as an indexed location.

```text
React Settings
  -> FileManager.Api
  -> IIndexedLocationAppService
  -> Indexed-root repository
  -> SQLite
```

The path is normalized and validated before persistence.

An indexed root is application metadata pointing to a physical filesystem location.

Registering or removing an indexed location does not create or delete that physical directory.

---

## Start Indexing from React

```text
React Client
  -> POST indexing request to FileManager.Api
  -> IIndexingOrchestrationAppService
  -> indexing pipeline
  -> SQLite
```

The client may then navigate to or poll index status.

The backend owns indexing execution.

The React client does not perform filesystem enumeration itself.

---

## Index Status Flow

```text
React Client
  -> FileManager.Api
  -> IIndexStatusService
  -> current in-memory indexing state
  -> JSON response
```

Status information may include details such as:

* current state
* processed file count
* current file
* error information

Only status that the backend actually tracks should be exposed to the UI.

---

## File Created

When a physical file is created externally:

```text
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.AddAsync(path)
  -> FileEntryFactory.CreateAsync(path)
  -> IFileRepository.UpsertAsync(...)
  -> SQLite
```

The application reacts to the filesystem event.

It does not create the physical file.

---

## File Modified

```text
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.UpdateAsync(path)
  -> FileEntryFactory.CreateAsync(path)
  -> IFileRepository.UpsertAsync(...)
  -> SQLite
```

`FileEntryFactory` may reuse an existing hash when file metadata indicates that the file content
has not changed.

Otherwise, the source file is read to calculate a new hash.

The source file is never modified.

---

## File Deleted

```text
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService.RemoveAsync(path)
  -> IFileRepository.DeleteAsync(path)
  -> SQLite
```

The physical file was already removed outside FileManager.

FileManager only updates its own index to reflect reality.

It does not delete the physical file as part of this flow.

---

## File Renamed or Moved

```text
Physical filesystem change
  -> LocalFileWatcher
  -> FileChangeQueue
  -> FileChangeWorker
  -> IndexingService
  -> repository metadata update
  -> SQLite
```

The application observes a rename or move that has already occurred in the filesystem and synchronizes
its metadata accordingly.

It does not initiate the physical rename or move.

---

## Search Flow

```text
Search.jsx
  -> FileManager.Api
  -> ISearchAppService
  -> IFileRepository
  -> SQLite
  -> Search result DTOs
  -> React Client
```

Search operates on indexed metadata.

It does not scan or modify physical files during the normal query flow.

---

## Dashboard Flow

```text
Dashboard.jsx
  -> FileManager.Api
  -> IDashboardAppService
  -> indexed metadata / duplicate analysis
  -> Dashboard DTO
  -> React Client
```

Dashboard information is derived from application metadata.

---

## Duplicate Detection Flow

```text
React Client / CLI
  -> IDuplicateAppService
  -> IFileRepository.GetAllAsync()
  -> DuplicateDetector
  -> group FileEntry records by content hash
  -> DuplicateGroupDto[]
```

Conceptually:

```text
Hash A
  -> File 1
  -> File 2

Hash B
  -> File 3
  -> File 4
  -> File 5
```

Duplicate detection is report-only.

No physical file is deleted, moved, or modified.

The user decides whether any physical action should be taken.

---

## Virtual Folder Tree

Virtual folders are stored entirely in the application database.

Loading the tree follows:

```text
React Virtual Explorer
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> IVirtualFolderRepository
  -> SQLite
  -> VirtualFolder DTOs
  -> React
```

No physical directory is read or created as part of virtual-folder tree operations.

---

## Create Virtual Folder

```text
React Client
  -> POST FileManager.Api
  -> IVirtualFolderAppService.CreateAsync(...)
  -> validate name
  -> validate optional parent
  -> validate duplicate sibling name
  -> IVirtualFolderRepository.AddAsync(...)
  -> SQLite
```

The operation creates application metadata only.

No physical directory is created.

---

## Rename Virtual Folder

```text
React Client
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> validate operation
  -> IVirtualFolderRepository
  -> SQLite
```

Only the virtual folder name changes.

The physical filesystem remains unchanged.

---

## Move Virtual Folder

```text
React Client
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> validate target parent
  -> prevent hierarchy cycles
  -> update ParentId
  -> SQLite
```

This changes only the logical virtual hierarchy.

No physical directory is moved.

---

## Delete Virtual Folder

```text
React Client
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> validate delete rules
  -> remove virtual-folder metadata
  -> SQLite
```

Deleting a virtual folder must not delete physical files.

Any file membership removed as a consequence is logical membership only.

---

## Add File to Virtual Folder

```text
React Virtual Explorer
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> validate VirtualFolder
  -> validate FileEntry
  -> IVirtualFolderFileRepository
  -> insert membership
  -> SQLite
```

Conceptually:

```text
FileEntry
    |
    +---- VirtualFolderFile ----> VirtualFolder
```

The operation adds a logical relationship only.

The physical file remains in its existing location.

---

## Remove File from Virtual Folder

```text
React Virtual Explorer
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> IVirtualFolderFileRepository
  -> remove membership
  -> SQLite
```

Only the relationship is removed.

The indexed `FileEntry` and physical file remain intact.

---

## File in Multiple Virtual Folders

A single `FileEntry` may have multiple memberships:

```text
                +--> Virtual Folder A
                |
FileEntry ------+--> Virtual Folder B
                |
                +--> Virtual Folder C
```

All memberships reference the same indexed file.

No physical copies are created.

---

## File Virtual-Folder Membership Lookup

```text
React properties panel
  -> FileManager.Api
  -> IVirtualFolderAppService
  -> virtual-folder membership repository
  -> SQLite
  -> list of VirtualFolders containing the FileEntry
```

This flow supports showing all logical locations associated with a file.

---

## Persistence Boundary

SQLite stores application-owned state such as:

```text
FileEntry
IndexedRoot
VirtualFolder
VirtualFolderFile
future tags
future collections
derived metadata
```

SQLite is the source of truth for virtual organization.

The physical filesystem is the source of truth for physical files.

---

## Read/Write Boundary

### Physical Filesystem

Normal engine operations may:

```text
READ:
- paths
- metadata
- file contents for hashing
- change notifications
```

They must not automatically:

```text
WRITE:
- delete files
- move files
- rename files
- overwrite files
- modify contents
```

---

### FileManager Database

The application may write:

```text
- index records
- hashes and metadata
- indexed locations
- virtual folders
- virtual-folder membership
- status/application metadata
- future tags and collections
- future derived metadata
```

This separation is a core architectural invariant.

---

## Core Principle

Every data flow should preserve:

```text
Filesystem
    |
    | read / observe
    v
FileManager Index
    |
    +---- Search
    +---- Dashboard
    +---- Duplicate Detection
    +---- Virtual Folders
    +---- Future Organization Features
```

FileManager enriches and organizes information around physical files without silently taking ownership
of the files themselves.

