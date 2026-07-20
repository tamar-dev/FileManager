# ADR-002 Index Engine

Status: Accepted

Decision:
The system should be built around an Index Engine abstraction.

The engine should not depend directly on one indexing mechanism.

Future implementations may replace filesystem monitoring without changing business logic.
