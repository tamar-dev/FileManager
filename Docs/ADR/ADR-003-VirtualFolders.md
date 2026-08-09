# ADR-003: Virtual Folders

Status: Accepted

## Context

Users need to organize files into logical groupings without changing the physical folder structure
on disk.

Using physical directories for every organizational view would require moving or copying files,
which would create unwanted duplication, make organization rigid, and violate FileManager's
non-negotiable architectural rule that virtual organization must not automatically modify physical files.

The system therefore needs a logical organization layer that is independent of physical location.

## Decision

Implement virtual folders as application-owned metadata stored in the FileManager database.

A `VirtualFolder` represents a logical folder.

It may optionally reference another `VirtualFolder` through `ParentId`, allowing nested hierarchies.

A `VirtualFolderFile` represents the association between a `VirtualFolder` and a `FileEntry`.

This creates a many-to-many relationship:

```text
VirtualFolder N <----> N FileEntry
````

A single indexed file may therefore appear in multiple virtual folders without being copied,
moved, renamed, or otherwise changed on disk.

Virtual-folder operations include:

* creating a virtual folder
* renaming a virtual folder
* moving a virtual folder within the virtual hierarchy
* deleting virtual-folder metadata
* adding a file to a virtual folder
* removing a file from a virtual folder

All of these operations affect application metadata only.

They must not create, rename, move, copy, or delete physical files or directories.

## Hierarchy Rules

The virtual-folder hierarchy must remain valid.

The application must prevent:

* self-parenting
* missing parent references
* hierarchy cycles
* invalid folder names

Where sibling-name uniqueness is enforced, it should be handled consistently by the application and persistence layers.

## Membership Rules

A `VirtualFolderFile` must reference:

* an existing `VirtualFolder`
* an existing indexed `FileEntry`

Duplicate membership of the same file in the same virtual folder must not create duplicate association records.

Removing a membership affects only the logical relationship.

The indexed file and physical file remain unchanged.

## Consequences

### Positive

* Users can organize files independently of physical folder structure.
* One physical file can appear in multiple logical locations.
* Virtual-folder organization survives independently of UI presentation.
* The model supports nested Explorer-like navigation.
* Virtual organization can evolve without requiring physical file duplication.
* Future organization concepts such as tags and collections can follow the same reference-based pattern.

### Trade-offs

* The application must maintain referential integrity between indexed files and virtual-folder memberships.
* The UI must clearly distinguish virtual location from physical location.
* Physical file moves or deletions detected by indexing may require reconciliation of existing metadata references.
* Hierarchy validation adds application-level rules such as cycle prevention.

## Architectural Constraint

Virtual folders belong exclusively to the application-owned metadata layer.

They must never be treated as physical directories.

The filesystem remains the source of truth for physical files.

The FileManager database remains the source of truth for virtual-folder hierarchy and membership.

## Resulting Model

```text
Physical File
    |
    v
FileEntry
    |
    +---- VirtualFolderFile ----> VirtualFolder A
    |
    +---- VirtualFolderFile ----> VirtualFolder B
```

The physical file exists once.

FileManager may expose it through multiple virtual folders by storing references only.
