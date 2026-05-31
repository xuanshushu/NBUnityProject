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

- pass count, pass order, and `LightMode` tags
- keyword declarations, enable paths, and stripping behavior
- constant buffer layout and SRP Batcher safety
- coordinate-space assumptions in HLSL
- redundant texture sampling or dynamic branching
- inspector UI mapping to material properties and keywords

Response expectations:

- state the root cause clearly
- state exactly what was changed
- call out risks, especially variant count, batching, and Unity-version compatibility

Load additional context from:

- `D:\UnityProject\NBUnityProject\.codex\project.md`
- `D:\UnityProject\NBUnityProject\.codex\rules.md`
