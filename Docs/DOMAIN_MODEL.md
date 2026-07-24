# FileManager — Domain Model

## Overview

The domain is split into two categories that must always remain separate:

| Category | Description | Lives in |
|---|---|---|
| **Physical** | Files and folders that exist on disk | User's filesystem (read-only) |
| **Virtual** | Logical organization and metadata | Application database |

The application never modifies the physical category. It reads physical files to build and maintain
the virtual category.

---

## Physical (Read-Only Reference)

### FileEntry
Represents an indexed snapshot of a physical file. The application reads the file; the entry is
what it stores.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `FullPath` | Absolute path on disk |
| `Name` | Filename including extension |
| `Extension` | File extension |
| `Size` | Size in bytes at time of indexing |
| `LastModified` | Last-modified timestamp at time of indexing |
| `Hash` | SHA-256 content hash (used for identity and duplicate detection) |

`FileEntry` is the foundation of the index. It is kept in sync with the filesystem by the indexing
engine and change monitor. The physical file is never altered.

---

## Virtual Organization Layer

These entities exist entirely within the application database. They reference `FileEntry` records
but do not create, move, or alter any file on disk.

### VirtualFolder
A logical container that groups files independently of their physical location.
One physical file (`FileEntry`) can belong to any number of virtual folders.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `Name` | Display name |
| `ParentId` | Parent folder (nullable, for nested hierarchies) |

### VirtualFolderItem
The association between a `FileEntry` and a `VirtualFolder`.
Deleting a `VirtualFolderItem` removes the file from the logical view only.
The physical file is never affected.

| Field | Description |
|---|---|
| `VirtualFolderId` | Reference to `VirtualFolder` |
| `FileEntryId` | Reference to `FileEntry` |

### Collection / Album
A named, curated grouping of files. Collections differ from virtual folders in that they are
flat (no nesting) and may be manually curated or automatically maintained by rules.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `Name` | Display name |
| `Type` | Manual (album) or rule-based (smart collection) |

### Tag
A user-applied or AI-generated label attached to one or more files.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `Name` | Label text |
| `Source` | `User` or `AI` |

### FileTag
Association between a `FileEntry` and a `Tag`.

---

## Metadata Layer

### DuplicateGroup
Represents a set of files that share the same content hash.
The application identifies duplicates; the **user decides** what to do.
No automatic file deletion or modification is performed.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `Hash` | Shared SHA-256 content hash |
| `Members` | Collection of `FileEntry` references |

---

## Future Entities (AI Enrichment Layer)

These will be stored as application metadata only — never written to the physical files.

- `AiLabel` — AI-generated category or object tag
- `FaceCluster` — Grouping of faces identified across photos (with user confirmation)
- `OcrContent` — Extracted text from a scanned document or image
- `SmartCollection` — Rule-based collection driven by AI-generated metadata

All AI entities are additive and optional. Removing them has no effect on the physical filesystem
or on the core virtual organization layer.
