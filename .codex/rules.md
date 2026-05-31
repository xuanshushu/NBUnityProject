# Global Rules

These rules define strict behavior constraints when working on this project.
They must be followed at all times.

## Code Modification Rules

- Always modify existing code instead of creating new systems.
- Prefer the smallest possible change that fixes the issue.
- Do not refactor large systems unless explicitly requested.
- Do not introduce breaking changes.
- Do not rename public APIs without strong justification.
- Do not move files or change structure unless necessary.

## Scope Control

- Primary working directory is `Packages/NB_FX`.
- Do not modify files outside this directory unless explicitly required.
- Do not modify `Assets/`, `ProjectSettings/`, or other packages unless the task requires it.
- If work outside that scope is necessary, explain why first.

## Safety Rules

- Never guess implementation details.
- Always search the codebase before making changes.
- If unsure, gather more context instead of assuming.
- Do not invent Unity, URP, ShaderLab, or Shader APIs.
- Do not write code that merely "might work"; ensure correctness.

## Shader-Specific Rules

### Keywords

- Do not introduce new keywords unless absolutely necessary.
- Always consider shader variant explosion before adding keywords.
- Prefer `shader_feature` over `multi_compile`.
- Reuse existing keywords whenever possible.

### Pass and LightMode

- Do not modify or remove existing passes without understanding the full impact.
- Ensure all passes have correct `LightMode` tags.
- Do not add redundant passes.

### SRP and URP

- Do not break SRP Batcher compatibility.
- Do not introduce per-material data that disables batching.
- Ensure compatibility with the URP pipeline.

### HLSL Logic

- Do not duplicate logic across passes.
- Prefer modifying existing functions.
- Keep math consistent with the existing coordinate-space usage.

### ShaderFlag Protocol

- Do not bypass the existing packed ShaderFlag mechanism with ad-hoc bool state or scattered replacement logic.
- Do not change the meaning, position, or storage index of existing flag bits unless the migration impact is fully understood and intentionally handled.
- Treat C# flag definitions, shader property names, and HLSL flag macros as one synchronized protocol.
- When changing flag-driven behavior, verify the full chain:
  - runtime flag writes
  - material properties
  - inspector logic
  - HLSL reads
  - default shader values
- Do not break foldout-state flags, UV mode packing, wrap-mode packing, color-channel packing, custom-data packing, or p-noise blend packing with partial edits.
- Preserve compatibility for existing materials unless an explicit migration is part of the task.

## Performance Rules

- Do not introduce unnecessary texture sampling.
- Do not add dynamic branching in fragment shaders unless justified.
- Do not increase shader complexity without reason.
- Always consider runtime cost.

## Editor Rules

- Do not replace IMGUI with UI Toolkit.
- Keep inspector logic simple and readable.
- Ensure UI reflects actual shader state, including keywords and properties.
- Do not introduce unnecessary allocations.
- Editor-only inspector state should use dedicated material properties directly, typically `Float` properties for persisted foldout/toggle UI state.
- Do not store editor-only inspector state in packed ShaderFlag bits unless the task explicitly requires protocol-level compatibility.

## Compatibility Rules

- Must support Unity 2021.3 through current project targets.
- Do not use APIs unavailable in older Unity versions unless guarded.
- Use preprocessor directives when necessary.
- Ensure ShaderLab and HLSL compatibility across supported versions.

## Problem Solving Rules

When solving an issue:

1. Always search for relevant code first.
2. Identify the exact problem type.
3. Understand the existing implementation.
4. Identify the root cause, not just symptoms.
5. Apply the minimal fix.
6. Validate side effects.

## What Not To Do

- Do not rewrite systems unless explicitly asked.
- Do not introduce new architecture without justification.
- Do not over-engineer solutions.
- Do not increase shader variant count blindly.
- Do not ignore performance implications.
- Do not ignore backward compatibility.

## Preferred Working Style

- Think like a senior Unity graphics engineer.
- Be precise and conservative.
- Favor stability over cleverness.
- Read more code before acting.
- Make changes that are easy to review and revert.
