---
name: unity-shader-debug
description: Debug Unity URP shader issues in Packages/NB_FX. Use when investigating rendering bugs, broken passes, incorrect lighting, HLSL logic faults, material-state mismatches, or URP compatibility regressions.
---

# Unity Shader Debug

Work inside `Packages/NB_FX` unless the task clearly requires another location.

Follow this order:

1. Search for the relevant shader, include, editor script, and material tooling code.
2. Classify the issue before changing code:
   - ShaderLab or pass layout
   - HLSL logic
   - keyword or variant state
   - material inspector sync
   - URP renderer or pipeline integration
3. Read the existing implementation end to end before proposing a fix.
4. Identify the root cause, not just the visible symptom.
5. Apply the smallest safe fix in existing files whenever possible.
6. Validate likely side effects on other passes, keywords, batching, and version compatibility.

Checks to perform when relevant:

- Pass count, pass order, and `LightMode` tags
- Keyword declarations, enable paths, and stripping behavior
- Constant buffer layout and SRP Batcher safety
- Coordinate-space assumptions in HLSL
- Redundant texture sampling or dynamic branching
- Inspector UI mapping to material properties and keywords

Response expectations:

- State the root cause clearly
- State exactly what was changed
- Call out risks, especially variant count, batching, and Unity-version compatibility

Load additional context from:

- [../../project.md](../../project.md) for project background
- [../../rules.md](../../rules.md) for hard constraints
