# Project Knowledge Base

This document stores project-specific knowledge, directory intent, module relationships, and migration context for `Packages/NB_FX`.

## Package Scope

`Packages/NB_FX` is a Unity package named `com.xuanxuan.nb.fx`.

Top-level modules:

- `NBShaders`: legacy production particle and FX shader package
- `NBShaders2`: next-generation shader package under active development
- `NBPostProcessing`: post-processing and fullscreen distortion pipeline
- `XuanXuanRenderUtility`: shared runtime and editor utility layer used by other modules

## Current Development Direction

Shader work currently has two valid tracks:

1. Maintain and extend `NBShaders` when production behavior must stay on the existing path.
2. Build and migrate features in `NBShaders2` when the work benefits from the newer modular editor architecture.

`NBShaders` is not obsolete. It remains both the current production baseline and an active maintenance target.

`NBShaders2` should be treated as the modular evolution path, not the default destination for every change.

## Directory Guide

### `NBShaders`

Current production implementation with tightly coupled shader, flags, and inspector logic.

- `Shader/ParticleBase.shader`: old main FX shader entry
- `Shader/HLSL/`: HLSL implementation details for the legacy shader
- `Editor/ParticleBaseGUI.cs`: large all-in-one custom inspector
- `Runtime/W9ParticleShaderFlags.cs`: packed flag and foldout bit definitions used by the old system

### `NBShaders2`

New shader path being developed from the legacy system.

- `Shader/NBShader.shader`: new shader entry
- `Editor/NBShaderGUI.cs`: thin shader GUI entry point
- `Editor/ShaderGUIItems/`: modular editor blocks for `NBShaders2`
- `Runtime/`: currently minimal; runtime surface is less mature than `NBShaders`

### `XuanXuanRenderUtility`

Shared foundation for editor GUI composition and common rendering helpers.

- `Editor/ShaderGUIItems/ShaderGUIRootItem.cs`: root orchestration for modular shader inspector UIs
- `Runtime/ShaderFlagsBase.cs`: base abstraction for packed shader flags
- `Editor/ShaderGUIHelper.cs`: common inspector helpers reused across shader editors
- `Shader/HLSL/`: shared shader utility code and bundled noise functions

### `NBPostProcessing`

Separate post-processing stack with its own runtime passes and shaders.

It is related to the broader FX package, but it is not the primary target when working on `NBShaders2`.

## Module Relationships

`NBShaders2` currently depends on the same conceptual flag model as `NBShaders`, while moving inspector implementation onto reusable classes from `XuanXuanRenderUtility`.

Key relationships:

- `NBShaders/Runtime/W9ParticleShaderFlags.cs` defines many packed material flags and foldout bits.
- `NBShaders2/Editor/NBShaderGUI.cs` creates a `NBShaderRootItem` instead of embedding all inspector logic directly.
- `NBShaders2/Editor/ShaderGUIItems/ModeBigBlockItem.cs` and `BaseOptionBigBlockItem.cs` are early examples of the modular GUI pattern.
- `XuanXuanRenderUtility/Editor/ShaderGUIItems/ShaderGUIRootItem.cs` provides the reusable root lifecycle for those modular blocks.

`NBShaders2` is therefore not a clean-room rewrite. It is a staged migration that still inherits assumptions and naming from the old `NBShaders` ecosystem.

## ShaderFlag System

One of the most important project-specific systems is the packed ShaderFlag protocol.

This is a shared contract across C#, material properties, shader defaults, inspector state, and HLSL decoding.

### Core Files

- `XuanXuanRenderUtility/Runtime/ShaderFlagsBase.cs`
- `NBShaders/Runtime/W9ParticleShaderFlags.cs`
- `NBShaders/Shader/HLSL/EffectFlags.hlsl`
- `NBShaders/Shader/HLSL/ParticlesUnlitInputNew.hlsl`
- `NBShaders/Editor/ParticleBaseGUI.cs`
- `XuanXuanRenderUtility/Editor/ShaderGUIHelper.cs`
- `NBShaders2/Shader/NBShader.shader`

### What `ShaderFlagsBase` Provides

- `SetFlagBits`
- `ClearFlagBits`
- `CheckFlagBits`
- support for both `Material` and `MaterialPropertyBlock`
- indexed storage across multiple packed integers

This base class is reused outside `NBShaders`, so the pattern is a package-level convention.

### What `W9ParticleShaderFlags` Adds

`W9ParticleShaderFlags` maps packed storage slots by index:

- `0`: `_W9ParticleShaderFlags`
- `1`: `_W9ParticleShaderFlags1`
- `2`: `_W9ParticleShaderWrapFlags`
- `3`: `_W9ParticleShaderGUIFoldToggle`
- `4`: `_W9ParticleShaderGUIFoldToggle1`
- `5`: `_W9ParticleShaderGUIFoldToggle2`
- `6`: `_W9ParticleShaderColorChannelFlag`
- `7`: `_W9ParticleShaderPNoiseBlendFlag`

The system stores more than feature toggles. It also persists foldout state, wrap-mode encoding, UV mode selection, color-channel selection, custom-data source mapping, and procedural-noise blend mode selection.

### Important Consequence

Treat this system as a protocol, not a local implementation detail.

When changing any part of it, think in terms of compatibility across:

- C# constants
- HLSL macros
- shader property declarations
- default values in `.shader`
- inspector write logic
- runtime `MaterialPropertyBlock` logic
- existing serialized materials

## Legacy vs New Architecture

### `NBShaders`

Characteristics:

- giant shader property surface
- large inspector class with many responsibilities
- behavior coordinated through packed bit flags and direct material mutations
- production behavior currently lives here

Typical reference files:

- `NBShaders/Shader/ParticleBase.shader`
- `NBShaders/Editor/ParticleBaseGUI.cs`
- `NBShaders/Runtime/W9ParticleShaderFlags.cs`

### `NBShaders2`

Characteristics:

- feature-heavy shader with decomposing editor architecture
- GUI composed from item classes and larger blocks
- shared editor infrastructure pulled from `XuanXuanRenderUtility`
- migration happens feature by feature

Typical reference files:

- `NBShaders2/Shader/NBShader.shader`
- `NBShaders2/Editor/NBShaderGUI.cs`
- `NBShaders2/Editor/ShaderGUIItems/ModeBigBlockItem.cs`
- `NBShaders2/Editor/ShaderGUIItems/BaseOptionBigBlockItem.cs`

## Suggested Working Strategy

First decide which track the task belongs to:

- `NBShaders` maintenance or production bugfix
- `NBShaders2` feature development or migration

### For `NBShaders` Maintenance

1. Fix the production path directly in `NBShaders`.
2. Trace the full chain:
   - shader properties
   - HLSL or ShaderLab behavior
   - editor-side mutations
   - packed flag usage
3. Keep the fix minimal and preserve existing serialized behavior unless change is intentional.

### For `NBShaders2` Development

1. Find the behavior in `NBShaders` first.
2. Locate the corresponding shader property, packed flag, keyword, and inspector logic.
3. Decide whether `NBShaders2` should preserve the same serialized property contract or intentionally diverge.
4. Port features in small vertical slices:
   - shader properties
   - HLSL or ShaderLab behavior
   - inspector block
   - material keyword or render-state synchronization
5. Verify that the modular GUI still drives the real material state correctly.

## Common Pitfalls

- Porting a property without porting related inspector state logic.
- Porting inspector UI without updating render queue, blend, z-write, or keyword side effects.
- Treating `NBShaders2` as fully independent when it still relies on legacy flags and naming.
- Changing packed flag meaning without checking old materials or helper code.
- Editing only shader code when behavior is also controlled from editor-side material mutations.
