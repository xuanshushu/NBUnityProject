---
name: nb-fx-release-doc
description: Create or update NBShader/NB/NB material/NB FX release documentation. Use when the user asks to make a release doc, release note, installation package Release, or feature manual update for NBShader, NB材质, NB特效, NB_FX, NB, or similar prompts such as "做一个NBShader发布文档", "做一个NB特效的发布文档", "更新NB材质Release", "写NB安装包ReleaseNote".
---

# NB FX Release Documentation

Use this skill to prepare and update NB_FX release docs from the current `Packages/NB_FX` Git state.

## Required Skills And Sources

- Use `lark-cli-local` for Feishu/Lark CLI behavior and PowerShell UTF-8 rules.
- Use `lark-doc` when reading or updating Feishu docs.
- If the change touches packed shader flags, also use `shader-flag-protocol`.
- Read `.codex/rules.md` and `.codex/project.md` before interpreting code changes.

## Fixed Project Context

- Work only from `D:\UnityProject\NBUnityProject\Packages\NB_FX`.
- Do not inspect unrelated Unity project files unless the user explicitly asks.
- If Git reports dubious ownership, use one-shot safe directory, not global config:
  ```powershell
  git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX ...
  ```

## Feishu Docs

- Main wiki: `BcmDwYEn6iaYl2kBDhhcLryTnRb`
- NB Shader art manual: `BHz8wHHSjiYJagk7WrmcAcconlb`
- NB post-processing art manual: `BpVLwAZ0liNMMpkcXMUcdUaUnVr`
- NB Shader art manual draft/copy for manual transfer: `Sw35w49cui3e69ksZ5ScHAdEngg`
- NB post-processing art manual draft/copy for manual transfer: `HQstwk9p0iqzeYk8aUocOXuonzJ`
- Installation package ReleaseNote: `YeRdwgVgOiqJG6krRXQcwaDTnot`

For wiki links, prefer `docs +fetch --doc=<token-or-url>` first. If a wiki token cannot be fetched directly, resolve it through the wiki node API per `lark-doc`.

## Art Manual Draft Policy

When writing NB update docs that include art-facing manual changes, do not update the official art manuals directly unless the user explicitly asks for that exact write.

- Write NB Shader art manual changes to the draft/copy doc `Sw35w49cui3e69ksZ5ScHAdEngg`.
- Write NB post-processing art manual changes to the draft/copy doc `HQstwk9p0iqzeYk8aUocOXuonzJ`.
- The user will manually copy the approved content from these draft docs into the official docs.
- For these draft docs, it is acceptable to overwrite the whole draft with a concise "今日新增/调整内容摘录" document when the user asks to extract today's changes.
- Keep a little placement context by using section-path headings such as `材质面板 / 特别功能 / 公共UV` or `后处理控制器面板 / 反闪`.
- Do not include old unchanged manual sections, videos, screenshots, or fragile media blocks in the extract unless the user explicitly asks.

## Workflow

1. Fetch current ReleaseNote and find the latest release block.
2. Extract the previous release commit from `NB_FX Git Commit: <sha>`.
3. Inspect only `Packages/NB_FX`:
   ```powershell
   git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX status --short
   git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX rev-parse HEAD
   git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX log --oneline --reverse <previous-sha>..HEAD
   git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX diff --stat <previous-sha>..HEAD
   git -c safe.directory=D:/UnityProject/NBUnityProject/Packages/NB_FX diff --name-status <previous-sha>..HEAD
   ```
4. Read changed source files enough to understand user-facing behavior. Do not rely only on commit names.
5. Classify changes into:
   - `新功能`
   - `兼容性/功能优化`
   - `Bug优化`
   - `版本兼容问题` only when user action or compatibility risk is real
6. Ask only for high-impact missing choices:
   - release version
   - whether to update ReleaseNote only or also manual pages
   - whether a package attachment exists, should be left as placeholder, or should be omitted
7. Draft the ReleaseNote and manual updates in the existing style.
8. Use `lark-cli docs +update --dry-run` before each Feishu write.
9. After writing, fetch the docs and verify key phrases are present.

## PowerShell UTF-8 Rule

Always set console encoding before piping Chinese Markdown into `lark-cli`; otherwise Chinese text may become question marks.

```powershell
$utf8 = New-Object System.Text.UTF8Encoding $false
$OutputEncoding = $utf8
[Console]::InputEncoding = $utf8
[Console]::OutputEncoding = $utf8
@'
Markdown here
'@ | lark-cli docs +update --as=user --doc="<doc>" --mode=<mode> --selection-by-title="<title>" --markdown=-
```

## ReleaseNote Style

- Insert newest version above the previous top version.
- Use exactly this heading style:
  ```markdown
  # Version：x.y.z
  ```
- Include commit when available:
  ```markdown
  <quote-container>
  NB_FX Git Commit: <HEAD sha>
  </quote-container>
  ```
- If no package is uploaded yet, write:
  ```markdown
  <quote-container>
  安装包待上传。
  </quote-container>
  ```
- Keep bullets short and result-oriented.
- Use user-facing wording: `新增`, `支持`, `修复`, `优化`, `调整`.
- Mention contributors with `@名字` only if the source material clearly provides them.

## Manual Style

- Write for artists first. Avoid backend terms such as "矩阵传递", "协议", "渲染状态同步", or implementation internals.
- Match existing feature sections:
  - heading
  - one or two short purpose sentences
  - parameter bullets
  - necessary warnings
- Prefer sentences like:
  - `用于...`
  - `控制...`
  - `默认值为...`
  - `需要注意...`
  - `可以通过粒子系统的CustomData...`
- Do not over-explain algorithms. Explain how the user should choose or adjust the feature.
- For technical restrictions, phrase them as practical art-facing limits:
  - Good: `部分VAT模式需要模型带有额外UV数据。`
  - Bad: `该模式依赖TEXCOORD语义和矩阵数据传递。`

## Update Targets

- Release-only work updates `安装包ReleaseNote`.
- Shader feature documentation updates `NB Shader美术功能说明`.
- Post-processing feature documentation updates `NB 后处理美术功能说明`.
- Installation entry changes may also require `NB特效功能说明书` main wiki page.

Use section-local updates instead of overwrite when possible:

```powershell
lark-cli docs +update --as=user --doc="<doc>" --mode=insert_before --selection-by-title="<heading>" --markdown=- --dry-run
lark-cli docs +update --as=user --doc="<doc>" --mode=insert_after --selection-with-ellipsis="start...end" --markdown=- --dry-run
```

If a heading is not found, fetch nearby content and switch to `selection-with-ellipsis`.

## Verification

After updates, run read-only checks:

- ReleaseNote contains the new `Version：x.y.z`.
- ReleaseNote contains the exact HEAD commit.
- Manual contains each new section or key phrase.
- Previous release blocks remain present.
- `git status --short` in `Packages/NB_FX` is unchanged.

Summarize what docs were updated and any placeholders left for the user, especially package upload placeholders.
