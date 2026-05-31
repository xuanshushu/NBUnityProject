---
name: urp-keyword-audit
description: Audit shader keywords and variant usage for the URP FX shader system. Use when reviewing shader_feature vs multi_compile choices, keyword consistency, stripping issues, or suspected variant explosion.
---

# URP Keyword Audit

Focus on `Packages/NB_FX`.

Audit workflow:

1. Find all relevant keyword declarations in shaders and includes.
2. Find all runtime and editor-side code that reads, sets, or synchronizes those keywords.
3. Group keywords by purpose:
   - feature toggles
   - lighting or pass behavior
   - debug or editor-only state
4. Identify problems:
   - unnecessary `multi_compile`
   - duplicate or overlapping keywords
   - keywords enabled without matching property or UI state
   - stripping gaps
   - combinations that inflate variants without shipping value
5. Recommend the smallest safe correction.

When reporting, include:

- where the keyword is declared
- where it is controlled
- whether it should remain `shader_feature` or `multi_compile`
- possible compatibility or content-authoring risks if changed

Do not add new keywords unless there is a strong reason and the variant cost is justified.

Load additional context from:

- [../../rules.md](../../rules.md)
- [../../project.md](../../project.md)
