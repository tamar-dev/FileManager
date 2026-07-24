# FileManager — Product Requirements Document

## Product Vision

FileManager is a **local desktop file management application**.

Its purpose is to help users **rediscover, organize, and manage their files** — especially photos —
without changing the physical folder structure on disk.

Users accumulate thousands of files spread across many physical directories. FileManager overlays a
**virtual organization layer** on top of the existing filesystem, letting users organize the same
physical file into multiple logical locations — virtual folders, collections, albums, and future
organizational structures — without duplicating or moving it.

The application owns only **metadata and virtual organization**. It never owns the physical files.

### What FileManager Is

- A local desktop file management and organization application.
- A personal file library that grows smarter over time.
- A virtual organization layer that sits above the native filesystem.
- An application for browsing, organizing, and rediscovering files without altering their physical location.

### What FileManager Is Not

- A search engine. Search is an enabling capability, not the product goal.
- A file synchronization tool.
- A cloud storage service.
- An application that moves, renames, or restructures the user's files automatically.

---

## Non-Negotiable Principles

These principles are **fundamental architectural and product rules**. They are not implementation
details and must never be violated by any feature, background service, or automated process.

1. **Never delete user files automatically.**
   No feature, background process, indexing pass, synchronization step, duplicate detection run,
   or AI enrichment operation may delete a physical file without explicit user initiation and
   confirmation.

2. **Never modify physical user files automatically.**
   The application must never move, rename, overwrite, or otherwise alter a physical file
   automatically. Indexing, change monitoring, thumbnail generation, duplicate detection, and AI
   analysis are all read-only operations on the filesystem.

3. **The user's filesystem is always the source of truth for physical files.**
   The application index reflects the filesystem; the filesystem does not reflect the index.
   If the index and the filesystem disagree, the filesystem wins.

4. **The application owns only metadata and virtual organization.**
   Virtual folders, collections, albums, tags, AI-generated metadata, and every future
   organizational feature exist entirely within the application's own database.
   None of them create, move, or alter any file or folder on disk.

5. **Background services may read files but must never write to them.**
   Indexing workers, hash calculators, thumbnail generators, change monitors, and AI enrichment
   pipelines are permitted to read file content. They must never write to, move, rename, or delete
   any file.

6. **Any operation that affects physical files must be explicit and user-initiated.**
   If a future feature allows a user to rename, move, or delete a physical file, it must be an
   action the user consciously triggers, with a clear confirmation step. It must never happen
   automatically during indexing, synchronization, duplicate detection, AI enrichment, or any
   background processing.

---

## Primary Capabilities

### Virtual Organization Layer
- **Virtual folders** — logical containers that reference physical files without moving them.
- **Collections and albums** — curated groupings independent of the physical folder structure.
- **Tags and metadata** — user-applied and application-extracted labels.
- One physical file can belong to many virtual folders, collections, and albums simultaneously.

### File Library
- Reliable indexing of local files with rich metadata extraction.
- Timeline-based browsing of files by date.
- Thumbnail previews generated from file content.
- Duplicate detection and management (the user decides what to do with duplicates).

### Search as Enabling Capability
Fast, accurate search is critical infrastructure for delivering the product experience.
It is not the product itself. Search enables users to find files; the product helps users
organize and rediscover them.

---

## Development Priorities

The following priorities govern implementation order:

1. **Reliable indexing engine** — the foundation everything else depends on.
2. **Rich metadata extraction** — file properties, EXIF data, media attributes.
3. **Timeline-based browsing** — browsing files by date captured or date modified.
4. **Virtual folders** — associating one physical file with multiple logical locations.
5. **Collections and albums** — curated, named groupings of files.
6. **Duplicate detection and management** — identifying duplicates; user decides what to do.
7. **Thumbnail generation** — visual previews generated in the background without modifying files.
8. **AI enrichment** — automatic categorization, object recognition, face recognition, OCR, and
   semantic search as an optional metadata layer (see Future Vision).

---

## Core Architecture Principle

| Layer | Owns |
|---|---|
| Physical filesystem | Physical files and folder structure |
| Metadata Store | File metadata, hashes, paths |
| Virtual organization layer | Virtual folders, collections, albums, tags |
| AI enrichment layer (future) | AI-generated metadata and smart collections |

No layer may modify the layer above it in this table. The application reads upward (from the
filesystem) and writes only to the layers it owns (index, virtual organization, AI-generated metadata).

---

## Future Vision — Intelligent Personal File Library

The long-term vision is an **intelligent personal file library** that understands the user's content
and surfaces it in meaningful ways.

Future AI capabilities will enrich the user's library through:

- **Automatic categorization** — grouping files by inferred type, subject, or event.
- **Object recognition** — tagging photos by the objects or scenes they contain.
- **Face recognition** — identifying people across photos with user confirmation.
- **OCR** — extracting searchable text from scanned documents and images.
- **Semantic search** — finding files by meaning rather than filename alone.
- **Smart collections** — automatically maintained groupings based on content and metadata rules.

**AI enriches metadata and organization. It does not replace user control.**

All AI-generated metadata is stored in the application's database as suggestions or annotations.
The user can review, correct, or remove any AI-generated metadata. AI analysis is a read-only
operation on the physical filesystem and must never trigger any file modification.

---

## Out of Scope

- Cloud sync or remote storage.
- Automatic file organization (moving or renaming files on disk).
- File editing or content modification.
- Any feature that modifies physical files without explicit user action.
