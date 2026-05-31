---
name: material-inspector-sync
description: Check and fix synchronization between Unity material inspector UI, shader properties, and keyword state. Use when toggles do not match runtime behavior, inspector changes do not affect rendering, or editor tooling drifts from shader logic.
---

# Material Inspector Sync

Target the custom inspector and shader code in `Packages/NB_FX`.

Use this workflow:

1. Find the relevant shader properties.
2. Find the inspector code that draws and updates those properties.
3. Find the keyword or pass logic driven by those properties.
4. Verify the mapping across all three layers:
   - shader property declaration
   - inspector control
   - runtime or shader keyword behavior
5. Fix mismatches with the smallest change possible.

Review checklist:

- property names and IDs match exactly
- inspector defaults match shader expectations
- UI state reflects real keyword state
- multi-edit and material refresh behavior are not broken
- no unnecessary editor allocations are introduced

When making changes:

- prefer updating existing inspector or shader code
- keep IMGUI patterns already used by the project
- explain any risks to existing materials or serialized values

Load additional context from:

- `D:\UnityProject\NBUnityProject\.codex\rules.md`
- `D:\UnityProject\NBUnityProject\.codex\project.md`
