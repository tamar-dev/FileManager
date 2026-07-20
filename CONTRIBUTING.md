# Contributing

## Project Vision (Mandatory)

This project is a **desktop file management engine**, not a search engine.

- The engine organizes existing files using a logical metadata layer.
- Physical files remain in their native disk locations.
- Search is only one consumer of metadata, not the primary purpose.

## Source of Truth

- **Filesystem**: source of truth for physical files.
- **Application database**: source of truth for logical organization and application metadata.

The engine never owns physical files. It only indexes and organizes them.

## Architecture Rules

- Keep modules focused and single-responsibility.
- Keep `Core` independent from `Infrastructure`.
- `Infrastructure` provides storage, indexing, and OS integration implementations.
- Preserve existing functionality and accepted architectural decisions.

## Documentation Standards

When updating `PRD`, `ARCHITECTURE`, `ROADMAP`, and `ADR` documents:

- Preserve the project vision above.
- Keep documents concise and clear.
- Remove contradictions.
- Eliminate duplicated information.
- Avoid unnecessary complexity.
- Do not reposition the project as a search engine.
- Suggest new ADRs only for significant architectural decisions.

## Performance Direction

- Current implementation may use directory enumeration and `FileSystemWatcher`.
- Architecture must allow replacing indexing implementations in the future without changing business logic.