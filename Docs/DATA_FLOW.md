# Data Flow

## Startup

Start Engine
 -> Load existing index
 -> Run initial indexing if needed
 -> Start change monitoring


## File Created

Filesystem
 -> ChangeMonitor
 -> Queue
 -> Worker
 -> IndexingService
 -> Repository
 -> Database


## File Modified

Filesystem
 -> ChangeMonitor
 -> Queue
 -> Update metadata


## File Deleted

Filesystem
 -> ChangeMonitor
 -> Remove from index
