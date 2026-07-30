# FileManager — Domain Model

## Overview

The FileManager domain is intentionally separated into two categories:

| Category | Description | Source of truth |
|---|---|---|
| **Physical** | Files and directories that exist on disk | User filesystem |
| **Application-owned** | Index data, virtual organization, and derived metadata | FileManager database |

The filesystem remains the source of truth for physical files.

FileManager may read physical files and store metadata about them, but application-owned structures must not implicitly modify the physical filesystem.

---

## Physical File Index

### `FileEntry`

`FileEntry` represents the indexed metadata for a physical file.

It is not the physical file itself.

| Field | Description |
|---|---|
| `Id` | Internal stable identifier |
| `FullPath` | Absolute physical path |
| `Name` | File name |
| `Extension` | File extension |
| `Size` | Size in bytes |
| `LastModified` | Last-modified timestamp |
| `Hash` | SHA-256 content hash |

A `FileEntry` is created and maintained by the indexing engine.

The filesystem remains authoritative for whether the physical file actually exists.

---

## Indexed Locations

### `IndexedRoot`

`IndexedRoot` represents a physical location selected by the user for indexing.

It allows FileManager to remember which filesystem roots belong to the user's indexed library.

Typical responsibilities include:

- identifying a root path
- tracking indexing-related metadata
- supporting re-indexing and status presentation

An `IndexedRoot` refers to a physical filesystem location.

It does not own or modify that directory.

---

## Virtual Organization

Virtual organization is stored entirely in the FileManager database.

It exists independently of physical folder structure.

### `VirtualFolder`

`VirtualFolder` represents a logical folder.

It does not represent a physical directory.

| Field | Description |
|---|---|
| `Id` | Internal identifier |
| `Name` | Display name |
| `ParentId` | Nullable parent virtual-folder identifier |
| `CreatedAt` | Creation timestamp |
| `UpdatedAt` | Last update timestamp |

`ParentId` allows nested virtual folders.

A root virtual folder has no parent.

Conceptually:

```text
Virtual Folder A
├── Virtual Folder B
│   └── Virtual Folder C
└── Virtual Folder D
````

The hierarchy belongs only to the application database.

Creating or rearranging this hierarchy does not create, rename, move, or delete physical directories.

---

### `VirtualFolderFile`

`VirtualFolderFile` represents membership between a `VirtualFolder` and a `FileEntry`.

It is the association entity for the many-to-many relationship between indexed files and virtual folders.

| Field             | Description                                          |
| ----------------- | ---------------------------------------------------- |
| `VirtualFolderId` | Reference to `VirtualFolder`                         |
| `FileEntryId`     | Reference to `FileEntry`                             |
| `CreatedAt`       | Optional membership creation timestamp, when present |

Conceptually:

```text
FileEntry
   |
   +---- VirtualFolderFile ----> VirtualFolder A
   |
   +---- VirtualFolderFile ----> VirtualFolder B
```

A single indexed file may therefore appear in multiple virtual folders.

This does not duplicate the physical file.

Removing a `VirtualFolderFile` record removes only the logical membership.

The physical file remains unchanged.

---

## Relationship Model

The primary virtual-organization relationship is:

```text
VirtualFolder 1 ---- N VirtualFolderFile N ---- 1 FileEntry
```

Which yields:

```text
VirtualFolder N <----> N FileEntry
```

This relationship is central to FileManager's virtual organization model.

It allows one physical file to be presented in multiple logical locations without copying or moving it.

---

## Virtual Folder Rules

The domain should preserve the following invariants:

* A virtual folder name must not be empty.
* A referenced parent virtual folder must exist.
* A virtual folder cannot be its own parent.
* Moving a virtual folder must not create a hierarchy cycle.
* A file membership must reference an existing indexed file.
* A file must not have duplicate membership in the same virtual folder.
* Removing virtual-folder metadata must not remove the physical file.

Where sibling-name uniqueness is enforced, that rule belongs to the virtual-folder use case and persistence contract.

---

## Duplicate Detection

### `DuplicateGroup`

A duplicate group represents a set of indexed files whose content hashes indicate identical content.

Conceptually:

```text
SHA-256 Hash
    |
    +---- FileEntry
    +---- FileEntry
    +---- FileEntry
```

Duplicate groups are derived from indexed file metadata.

They should not be treated as physical ownership structures.

The application reports duplicates.

It does not automatically delete, move, or alter any member file.

A duplicate group may be exposed to presentation layers through an application DTO rather than persisted as a dedicated entity.

---

## Future Organization Concepts

The following concepts belong to the application-owned metadata layer and may be introduced later.

### `Tag`

A label associated with one or more indexed files.

Potential sources:

* user
* future AI enrichment

A tag must never be written into or modify the source file unless a future explicit user action is designed for that purpose.

---

### `FileTag`

Association between a `FileEntry` and a `Tag`.

Conceptually:

```text
Tag N <----> N FileEntry
```

---

### Collection / Album

A named logical grouping of indexed files.

Collections are application metadata only.

They may differ from virtual folders by being flat, curated, or rule-driven.

---

### Smart Collection

A dynamically maintained logical grouping based on metadata rules.

Examples may include:

* file type
* date
* physical path
* tags
* future AI-generated metadata

Smart collections do not own physical files.

---

## Future Derived Metadata

Future enrichment features may add derived metadata such as:

* OCR text
* object/category labels
* image metadata
* semantic metadata
* other analysis results

Derived metadata remains separate from the physical file.

Removing derived metadata must not affect the source file.

---

## Ownership Summary

| Concept                   | Owned by filesystem | Owned by FileManager |
| ------------------------- | ------------------: | -------------------: |
| Physical file             |                 Yes |                   No |
| Physical directory        |                 Yes |                   No |
| File contents             |                 Yes |                   No |
| `FileEntry` index record  |                  No |                  Yes |
| `IndexedRoot`             |                  No |                  Yes |
| `VirtualFolder`           |                  No |                  Yes |
| `VirtualFolderFile`       |                  No |                  Yes |
| Duplicate grouping result |                  No |              Derived |
| Tags                      |                  No |                  Yes |
| Collections               |                  No |                  Yes |
| AI/derived metadata       |                  No |                  Yes |

---

## Core Principle

The domain model must preserve this distinction:

```text
Physical file
    |
    | indexed by
    v
FileEntry
    |
    +---- Virtual folders
    +---- Duplicate analysis
    +---- Future tags
    +---- Future collections
    +---- Future derived metadata
```

The physical file remains owned by the filesystem throughout.

FileManager organizes references and metadata around it.


