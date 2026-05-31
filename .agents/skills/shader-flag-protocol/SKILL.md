---
name: shader-flag-protocol
description: Work with the packed ShaderFlag protocol used by NBShaders, NBShaders2, and related FX systems. Use when adding, debugging, migrating, or reviewing bit-packed shader flags, wrap flags, UV mode packing, custom data packing, color channel packing, foldout flags, or MaterialPropertyBlock flag writes.
---

# Shader Flag Protocol

Use this skill whenever a task touches the packed ShaderFlag mechanism.

This project treats ShaderFlags as a cross-layer protocol, not a local implementation detail.

## Read These Files First

- `Packages/NB_FX/XuanXuanRenderUtility/Runtime/ShaderFlagsBase.cs`
- `Packages/NB_FX/NBShaders/Runtime/W9ParticleShaderFlags.cs`
- `Packages/NB_FX/NBShaders/Shader/HLSL/EffectFlags.hlsl`

Read these too when relevant:

- `Packages/NB_FX/NBShaders/Shader/HLSL/ParticlesUnlitInputNew.hlsl`
- `Packages/NB_FX/NBShaders/Editor/ParticleBaseGUI.cs`
- `Packages/NB_FX/XuanXuanRenderUtility/Editor/ShaderGUIHelper.cs`
- `Packages/NB_FX/NBShaders2/Shader/NBShader.shader`
- `Packages/NB_FX/NBShaders2/Editor/NBShaderGUI.cs`

## Core Rule

Do not treat a ShaderFlag change as a single-file edit.

Any meaningful change may require synchronized updates across:

1. C# flag definitions
2. shader property declarations and defaults
3. HLSL macro definitions and decoding logic
4. editor GUI read/write logic
5. runtime `Material` or `MaterialPropertyBlock` writes
6. existing serialized materials and compatibility assumptions

## What Counts As Protocol State

In this project, packed state includes more than feature toggles.

It also includes:

- main feature bits
- secondary feature bits
- wrap-mode encoding
- foldout persistence bits
- color channel selection
- UV mode selection
- custom data source mapping
- procedural-noise blend mode selection

## Required Investigation Flow

When changing or debugging a flag-related issue:

1. Identify which packed storage slot is affected.
2. Find the C# constant or constants involved.
3. Find the matching HLSL macro or decode path.
4. Find the shader property name and default value.
5. Find all GUI or runtime writers of that state.
6. Check whether old materials depend on the current bit layout.
7. Apply the smallest safe change.

## Slot Mapping to Keep in Mind

For `W9ParticleShaderFlags`, the packed indices currently map to:

- `0`: `_W9ParticleShaderFlags`
- `1`: `_W9ParticleShaderFlags1`
- `2`: `_W9ParticleShaderWrapFlags`
- `3`: `_W9ParticleShaderGUIFoldToggle`
- `4`: `_W9ParticleShaderGUIFoldToggle1`
- `5`: `_W9ParticleShaderGUIFoldToggle2`
- `6`: `_W9ParticleShaderColorChannelFlag`
- `7`: `_W9ParticleShaderPNoiseBlendFlag`

Do not change slot meaning casually.

## Special Cases

### Foldout Bits

Foldout bits are persisted material state and are also used by editor animation logic.

If you touch foldout bits, verify:

- `W9ParticleShaderFlags` bit constants
- `ShaderGUIHelper` foldout readers/writers
- any assumptions tied to anim-bool indexing

### Wrap Flags

Wrap mode is packed into `_W9ParticleShaderWrapFlags` and decoded in HLSL through bit pairs.

If you touch wrap behavior, verify both:

- C# write path
- `CheckLocalWrapFlags` behavior in HLSL

### Custom Data Packing

Custom data routing is nibble-packed and decoded in HLSL.

If you touch custom data mapping, verify both:

- C# flag position constants
- `GetCustomData` in HLSL

### UV Mode Packing

UV mode is packed and then decoded by `GetUVByUVMode`.

If you touch UV mode behavior, verify:

- editor write path
- packed bit positions
- HLSL decoder behavior

### PNoise Blend Packing

Blend mode is packed and decoded by `BlendPNoise`.

If you change available blend modes or bit layout, update both sides together.

## Do Not Do These

- do not replace the protocol with scattered booleans
- do not move existing bits without checking compatibility impact
- do not update only C# or only HLSL
- do not assume a flag is editor-only without checking shader reads
- do not add a new packed meaning if there is already an existing compatible slot or pattern

## Output Expectations

When reporting or implementing a change, explicitly state:

- which packed slot was involved
- which bit or encoded field was involved
- which files were updated or verified
- whether compatibility with existing materials was preserved
