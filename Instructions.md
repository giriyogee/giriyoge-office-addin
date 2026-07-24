# Global Claude Code Instructions

## Primary Goals

- Maximize accuracy while minimizing token usage.
- Read the smallest amount of code necessary to complete the task.
- Prefer semantic navigation over reading entire files.

---

## Serena

When Serena MCP tools are available:

- Use Serena first for code exploration and navigation.
- Prefer symbol lookup, references, implementations, call hierarchy, and semantic search instead of reading entire files.
- Read only the relevant classes, methods, or files identified by Serena.
- Avoid opening complete files unless necessary.
- Before making changes, understand the impacted symbols and references using Serena.

---

## Headroom

When Headroom MCP tools are available:

- Use `headroom_compress` before analyzing large source files, logs, stack traces, JSON, terminal output, build output, or command output (approximately >20 KB).
- Analyze the compressed representation whenever possible.
- Use `headroom_retrieve` only if additional details from the original content are required.
- Avoid sending large content directly when a compressed representation is sufficient.

---

## Code Reading Strategy

Always follow this order:

1. Use Serena to locate the relevant code.
2. Read only the necessary symbols or files.
3. If content is large, compress it using Headroom.
4. Retrieve original content only when required.

---

## General Guidelines

- Prefer targeted searches over opening complete files.
- Keep context as small as possible.
- Avoid duplicate reads of the same content.
- Do not load entire repositories into context unless explicitly requested.
- Before making changes, understand dependencies and impacted callers.
- Explain architectural decisions briefly and clearly.
