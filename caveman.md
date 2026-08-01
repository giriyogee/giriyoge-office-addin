I have the Caveman plugin installed and an existing custom Claude Code
statusLine in ~/.claude/settings.json.

Goal: preserve my current statusline (model, directory, context remaining)
and append:
- active Caveman mode
- estimated tokens saved from /caveman-stats

Inspect my existing settings and the installed Caveman plugin to determine
the actual state/files Caveman uses for mode and stats.

Requirements:
- Do NOT install --with-hooks or duplicate plugin hooks.
- Do NOT modify Caveman plugin files.
- Prefer a clean ~/.claude/statusline.ps1 rather than a large inline command.
- Do not assume Caveman paths/state files; verify them first.

Do not modify anything yet.

Show me:
1. What you found.
2. Proposed statusline.ps1.
3. Exact settings.json change.