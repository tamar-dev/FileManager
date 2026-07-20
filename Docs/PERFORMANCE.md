# Performance Strategy

The target is an indexing architecture inspired by high performance desktop indexers.

Important concepts:

## Separate storage from access speed

Persistent storage:
- SQLite
- Reliable metadata storage

Fast access layer:
- Memory index
- Optimized structures for frequent operations


## Background Processing

Heavy operations should not block indexing:

File discovery
    |
    v
Metadata saved
    |
    v
Background workers:
- Hash calculation
- Thumbnail generation
- Content analysis


## Incremental Updates

Never rebuild the whole index after every change.
Only process affected files.
