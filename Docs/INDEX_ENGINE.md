# Index Engine Design

## Goal
Maintain a complete and continuously updated representation of files.

## Architecture

                IndexEngine
                     |
        +------------+-------------+
        |                          |
        v                          v
 InitialIndexer              ChangeMonitor
        |                          |
        v                          v
 Metadata Index             Change Events


## Initial Indexing

The initial build creates a snapshot of filesystem metadata.

Current implementation:
- Directory enumeration

Future implementation:
- Windows NTFS optimized indexing mechanisms


## Incremental Updates

After initial indexing:

Created -> Add metadata
Changed -> Update metadata
Deleted -> Remove metadata
Renamed -> Update identity/path


## Important

The engine must not depend on FileSystemWatcher.

FileSystemWatcher is only one possible implementation of ChangeMonitor.
