# FileManager Engine - Product Requirements

## Vision
FileManager is a local desktop file management engine.

It provides a logical organization layer above the native filesystem and is **not** primarily a search engine.
Search is one consumer of metadata, not the product goal.

## Core Principle
- Physical filesystem: source of truth for physical files.
- Application database/index: source of truth for logical organization and application metadata.
- The engine never owns physical files; it indexes and organizes them.

## Primary Capabilities
- Virtual folders
- Associate one file with multiple logical folders
- Tags and metadata
- Duplicate detection
- Smart collections
- Large photo library organization
- Persistent metadata store synchronized with filesystem changes
