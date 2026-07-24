# FileManager — Roadmap

## Principle

FileManager is a local desktop file management application. Its goal is to help users rediscover,
organize, and manage their files without changing their physical folder structure.

The roadmap reflects the development priorities defined in the [PRD](PRD.md):

1. Reliable indexing engine
2. Rich metadata extraction
3. Timeline-based browsing
4. Virtual folders
5. Collections and albums
6. Duplicate detection and management
7. Thumbnail generation
8. AI enrichment

**Search is an enabling capability, not the product goal.** Fast, accurate search is part of the
infrastructure that powers browsing, organization, and discovery — not the end product itself.

**No roadmap item may introduce automatic modification of physical user files.** Every operation
that affects files on disk must remain an explicit, user-initiated action with confirmation.

---

## Phase 1 — Foundation

Goal: a stable, reliable metadata index that tracks the local filesystem.

- Reliable initial indexing and change monitoring separation
- Stable metadata schema and persistence (SQLite)
- Filesystem synchronization (created, modified, deleted, renamed events)
- Basic virtual folders and tags
- Hash-based file identity for duplicate detection groundwork

---

## Phase 2 — Organization Features

Goal: the virtual organization layer that lets users organize files without moving them.

- Virtual folders — one physical file in multiple logical locations
- Multi-folder logical association for a single file
- Collections and albums — named, curated groupings
- Smart collections — rule-based groupings driven by metadata
- Duplicate detection pipeline — identification only; user decides on action
- Photo-library oriented metadata improvements (EXIF, capture date, camera model)

---

## Phase 3 — Browsing and Discovery

Goal: timeline-based browsing and rich metadata-driven navigation.

- Timeline-based browsing — browse files by capture date or last-modified date
- Thumbnail generation — background preview generation (read-only on source files)
- Metadata-driven filtering and browsing
- Performance: replaceable indexing strategies behind Core contracts
- Performance: background workers for hashing and thumbnail generation
- Performance: incremental update optimizations
- Memory/cache access layer for frequent queries

---

## Phase 4 — Advanced Engine Capabilities

Goal: operational maturity and extensibility for the long-term vision.

- Collection and query engine enhancements
- Thumbnail cache lifecycle management
- Operational diagnostics and resilience improvements
- Search as a first-class navigation surface (powered by the metadata index)

---

## Phase 5 — AI Enrichment (Future Vision)

Goal: transform the library into an intelligent personal file archive.

All AI features are an **optional, additive, read-only metadata layer**. AI analysis reads file
content and writes only to the application's metadata store. No AI feature may modify, move,
rename, or delete physical files.

- Automatic categorization of files by inferred type, subject, or event
- Object recognition — tagging photos by content
- Face recognition — identifying people across photos (with user confirmation)
- OCR — extracting searchable text from scanned documents and images
- Semantic search — finding files by meaning rather than filename
- Smart collections powered by AI-generated metadata

See [PRD — Future Vision](PRD.md#future-vision--intelligent-personal-file-library) for the full
description of AI principles and constraints.
