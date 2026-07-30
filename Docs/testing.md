# FileManager — Testing Guide

## Purpose

This document describes the automated test strategy for FileManager.

The test suite exists to protect:

- filesystem safety
- indexing correctness
- persistence behavior
- application-service contracts
- virtual-organization rules
- integration between Core, Infrastructure, and SQLite

The exact number of tests is intentionally not documented here because it changes frequently.

The source of truth for current coverage is the test projects themselves.

---

## Test Projects

```text
tests/
├── FileManager.Core.Tests
├── FileManager.Application.Tests
├── FileManager.Infrastructure.Tests
└── FileManager.Integration.Tests
````

All test projects target the same .NET version as the application.

The suite uses:

* xUnit
* FluentAssertions
* Moq

---

## Test Layer Responsibilities

### `FileManager.Core.Tests`

Validates domain and core business behavior in isolation.

Typical coverage includes:

* hashing
* file metadata creation
* hash reuse
* duplicate detection
* initial indexing behavior
* indexing abstractions
* virtual-folder rules
* hierarchy validation
* cycle prevention
* filesystem-independent business rules

Core tests should avoid EF Core and SQLite.

External dependencies should be represented through mocked interfaces where appropriate.

---

### `FileManager.Application.Tests`

Validates application use cases and DTO mapping.

Typical coverage includes:

* indexing orchestration
* dashboard aggregation
* duplicate reporting
* search
* indexed locations
* path normalization behavior
* virtual-folder operations
* virtual-folder membership
* application-level validation
* result/DTO mapping
* conflict and not-found outcomes

Application tests should focus on use-case behavior.

They should not depend on:

* EF Core
* SQLite
* HTTP
* React
* real filesystem persistence unless a test explicitly requires it

---

### `FileManager.Infrastructure.Tests`

Validates persistence and infrastructure implementations.

Typical coverage includes:

* `FileRepository`
* indexed-root repository
* virtual-folder repository
* virtual-folder membership repository
* EF Core mappings
* SQLite persistence
* add/update/delete behavior
* unique constraints
* data round-tripping
* repository query semantics

Where practical, infrastructure tests should use a real SQLite database rather than mocking EF Core.

---

### `FileManager.Integration.Tests`

Validates real flows across multiple layers.

Typical integration scenarios include:

* initial indexing into SQLite
* real file metadata extraction
* hash persistence
* filesystem watcher flows
* create/update/delete synchronization
* rename/move synchronization
* virtual-folder persistence
* virtual-folder membership persistence
* end-to-end repository wiring
* dependency injection configuration

Integration tests should verify that components work together as configured in the application.

---

## Indexing Tests

### Initial Indexing

Initial indexing tests should verify that:

* candidate paths are supplied through `IIndexSource`
* discovered files are processed once
* metadata is created correctly
* files are persisted
* batch persistence behaves correctly
* cancellation is respected
* empty directories do not create invalid records

`InitialIndexingService` should not depend directly on `Directory` or a concrete filesystem-scanning implementation.

---

### Hash Reuse

Tests should verify that:

```text
unchanged size + unchanged last-modified
    -> existing hash may be reused
```

and:

```text
new or modified file
    -> SHA-256 is recalculated
```

The purpose is both correctness and performance.

---

### Incremental Monitoring

Watcher/incremental tests should verify:

* file creation adds indexed metadata
* file modification updates metadata
* file deletion removes stale metadata
* file rename/move updates the index correctly
* application behavior reflects external filesystem changes

The application must never initiate destructive filesystem behavior during these tests.

---

## Search Tests

Search tests should validate combinations of supported metadata filters such as:

* name
* extension
* physical path
* modified-after
* modified-before

They should also verify DTO mapping, including file classification/type when that is part of the public contract.

Search should operate against indexed metadata rather than scanning the filesystem at query time.

---

## Duplicate Detection Tests

Tests should verify:

* files with identical valid hashes are grouped
* files with different hashes are not grouped
* empty/invalid hashes are excluded where appropriate
* file count is correct
* total size is correct
* wasted/potential savings calculations are correct
* DTO mapping preserves file identity and metadata

Duplicate detection remains report-only.

No duplicate test should delete physical files as part of the feature behavior.

---

## Indexed Location Tests

Tests should verify:

* valid paths are normalized before persistence
* surrounding quotes are removed when appropriate
* whitespace is trimmed
* duplicate paths are detected case-insensitively on Windows
* invalid or missing paths are rejected
* removing an indexed location removes only application metadata
* physical directories are not deleted

Where applicable, repository tests should verify uniqueness behavior at the persistence boundary.

---

## Virtual Folder Tests

Virtual folders are a core product capability and require explicit coverage.

### Creation

Verify:

* root folders can be created
* nested folders can be created
* blank names are rejected
* missing parents are rejected
* duplicate sibling names are handled according to the application rule

### Rename / Update

Verify:

* a folder can be renamed
* a folder can be moved under another parent
* self-parenting is rejected
* hierarchy cycles are rejected
* missing target parents are rejected

### Deletion

Verify:

* deleting a virtual folder affects metadata only
* physical files remain unchanged
* non-empty-folder behavior follows the defined application rule
* memberships are handled safely

### Membership

Verify:

* an indexed file can be added to a virtual folder
* the same file can belong to multiple virtual folders
* duplicate membership does not create duplicate association rows
* membership can be removed
* removing membership does not remove the `FileEntry`
* removing membership does not modify the physical file

---

## Dependency Injection Tests

DI tests should verify that the main application registrations can be resolved successfully.

Important registrations include:

```text
IIndexingAppService
IDuplicateAppService
IDashboardAppService
ISearchAppService
IIndexedLocationAppService
IVirtualFolderAppService
IIndexStatusService
IIndexingOrchestrationAppService
```

Infrastructure repository implementations should also be resolvable through their interfaces.

The purpose of these tests is to catch composition-root regressions early.

---

## Database Tests

SQLite tests should verify:

* migrations create the expected schema
* repository mappings persist correctly
* IDs and foreign keys round-trip correctly
* many-to-many membership behaves correctly
* unique constraints behave as intended
* database access does not accidentally target the process working directory

The application uses its configured local database location rather than relying on implicit `DbContext`
fallback configuration.

---

## Filesystem Safety

Tests involving real files must preserve the project's non-negotiable safety rules.

Production feature behavior under test must not:

* delete user files
* move user files
* rename user files
* overwrite user files
* modify file contents

Temporary files created by the tests themselves may be cleaned up as test fixtures.

That cleanup is test infrastructure, not FileManager product behavior.

---

## Test Isolation

Each test should own its temporary state.

For filesystem/integration tests:

* use temporary directories
* isolate test data from the real user filesystem
* use dedicated SQLite databases
* clean up test-owned resources
* avoid sharing mutable database state across tests

When testing `FileSystemWatcher`, use polling with a timeout rather than fixed sleeps where possible.

This reduces flakiness while still failing quickly when the watcher pipeline is broken.

---

## Running the Tests

From the repository root:

```powershell
dotnet test
```

Run a single project:

```powershell
dotnet test tests/FileManager.Core.Tests
dotnet test tests/FileManager.Application.Tests
dotnet test tests/FileManager.Infrastructure.Tests
dotnet test tests/FileManager.Integration.Tests
```

Run a filtered group:

```powershell
dotnet test --filter "FullyQualifiedName~VirtualFolder"
```

or:

```powershell
dotnet test --filter "FullyQualifiedName~Search"
```

---

## Build Validation

Before merging a feature:

```powershell
dotnet build
dotnet test
```

For changes involving the React client, also run from:

```text
client/file-manager-app
```

```powershell
npm run build
```

A feature should not be considered complete if:

* the .NET solution does not build
* relevant automated tests fail
* the React production build fails for client changes

---

## Testing Principle

The test suite should protect behavior, not implementation details.

Prefer tests that answer:

```text
Does the system behave correctly?
```

over tests that answer:

```text
Did this private method get called exactly this way?
```

The highest-priority behaviors to protect are:

1. physical file safety
2. indexing correctness
3. persistence integrity
4. virtual-folder integrity
5. stable application contracts
6. predictable integration between layers

