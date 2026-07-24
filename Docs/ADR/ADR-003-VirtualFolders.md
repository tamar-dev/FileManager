# ADR-003: Virtual Folders

Status: Proposed

## Context

Users need to organize files into logical groupings without changing the physical folder structure
on disk. Moving or copying files to reflect logical organization is destructive, creates duplicates,
and violates the non-negotiable principle that the application never modifies physical user files
automatically.

## Decision

Implement a **virtual organization layer** that references physical files by identity (`FileEntry`)
rather than by physical location.

- A `VirtualFolder` is a logical container stored in the application database.
- A `VirtualFolderItem` is the association between a `FileEntry` and a `VirtualFolder`.
- One physical file (`FileEntry`) can belong to any number of virtual folders simultaneously.
- Creating, renaming, or deleting a virtual folder or its associations has **no effect on the
  physical filesystem**. No file is moved, renamed, copied, or deleted as a result of any virtual
  folder operation.

This principle extends to every future organizational feature: collections, albums, tags, and
AI-generated groupings all reference `FileEntry` records by identity — they never modify files.

## Consequences

- The user's physical folder structure is never altered by the application.
- Virtual organization is resilient to physical file moves: when the index detects a physical rename
  or move, virtual folder memberships can be preserved by file identity (hash).
- The virtual organization layer can be discarded or rebuilt entirely without affecting any physical
  file.
- Future organizational features (collections, albums, smart collections) follow the same
  architectural pattern: **reference by identity, never modify the filesystem**.
