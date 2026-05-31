# NBUnityProject Codex Instructions

Primary work happens in `Packages/NB_FX`.

This repository contains the FX rendering stack. Shader work may target either ongoing maintenance in `NBShaders` or new development in `NBShaders2`.

## Operating Rules

- Always search the relevant code before answering or editing.
- Prefer modifying existing files instead of creating parallel systems.
- Keep fixes minimal, safe, and easy to review.
- Do not hallucinate Unity, URP, ShaderLab, HLSL, or editor APIs.
- Be conservative with shader keywords, pass layout, render queue logic, and SRP Batcher compatibility.
- Do not modify files outside `Packages/NB_FX` unless the task clearly requires it.
- Avoid changes under `Assets`, `ProjectSettings`, or unrelated packages unless explicitly needed.

## Working Context

- `NBShaders` is the active production shader implementation and still receives maintenance.
- `NBShaders2` is the newer modularized shader and editor path under active development.
- `XuanXuanRenderUtility` provides shared editor and shader GUI infrastructure used by `NBShaders2`.
- `NBPostProcessing` is related, but it is not the default target for shader UI and packed-flag work.

## Shader Safety

- Do not introduce new keywords unless necessary, and prefer `shader_feature` over `multi_compile`.
- Do not modify or remove passes without understanding full impact, including `LightMode` usage.
- Preserve SRP Batcher compatibility and avoid per-material changes that silently disable batching.
- Do not duplicate HLSL logic across passes; prefer changing shared functions.
- Treat packed shader flags as a protocol spanning C#, shader properties, HLSL decoding, inspector writes, and serialized materials.

## Required Investigation Flow

1. Search for the relevant shader, include, editor script, runtime flag type, and material tooling code.
2. Classify the issue before editing:
   - ShaderLab or pass layout
   - HLSL logic
   - keyword or variant state
   - material inspector sync
   - packed ShaderFlag protocol
   - URP pipeline compatibility
3. Read the current implementation end to end.
4. Identify the root cause, not only the visible symptom.
5. Apply the smallest safe fix in existing files when possible.
6. Validate likely side effects on batching, variants, serialized materials, and Unity-version compatibility.

## Project Docs

Read these intentionally when relevant:

- `.codex/rules.md`: hard constraints
- `.codex/project.md`: project knowledge base and architecture notes
- `.codex/skills/`: reusable workflows for shader debugging, keyword audits, ShaderFlag protocol work, and inspector sync

## Fast Entry Points

- legacy shader baseline: `Packages/NB_FX/NBShaders/Shader/ParticleBase.shader`
- legacy inspector baseline: `Packages/NB_FX/NBShaders/Editor/ParticleBaseGUI.cs`
- legacy flag definitions: `Packages/NB_FX/NBShaders/Runtime/W9ParticleShaderFlags.cs`
- new shader entry: `Packages/NB_FX/NBShaders2/Shader/NBShader.shader`
- new inspector entry: `Packages/NB_FX/NBShaders2/Editor/NBShaderGUI.cs`
- modular GUI root: `Packages/NB_FX/XuanXuanRenderUtility/Editor/ShaderGUIItems/ShaderGUIRootItem.cs`
