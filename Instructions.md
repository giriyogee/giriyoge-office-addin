# Global Claude Code Instructions

## Token Optimization

When Headroom MCP tools are available:

- Prefer using `headroom_compress` before processing large source files, logs, stack traces, JSON, terminal output, build output, command output, or other large text (approximately >20 KB).
- Analyze the compressed representation whenever possible.
- Use `headroom_retrieve` only if additional details from the original content are required.
- Avoid sending or reading large content directly when compressed content is sufficient.
- If Headroom tools are unavailable or unsuitable, continue normally without failing.
