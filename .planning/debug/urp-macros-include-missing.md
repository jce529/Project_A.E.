---
status: awaiting-verification
trigger: "Shader error in Universal Render Pipeline/2D/Sprite-Lit-Default: Couldn't open include file Packages/com.unity.render-pipelines.core/ShaderLibrary/Macros.hlsl"
created: 2026-09-11
updated: 2026-09-11
---

# Symptoms

- Expected: Unity script compilation completes without a URP shader include error.
- Actual: Immediately after compiling, the Console reports that `Macros.hlsl` cannot be opened from `Common.hlsl`.
- Timeline: First observed just now.
- Reproduction: Compile the project; the error appears in the Console immediately afterward.

# Current Focus

- hypothesis: Unity's shader compiler transiently failed to resolve package include paths during its initial startup/import pass.
- test: Clear the Console and trigger one more script/shader compilation without changing package files.
- expecting: The error does not recur because all referenced include files are now present and subsequent asset refreshes produced no shader errors.
- next_action: User verifies whether a clean recompilation reproduces the error.

# Evidence

- timestamp: 2026-09-11
  finding: `Packages/manifest.json` requests URP 17.3.0; `packages-lock.json` resolves both URP and render-pipelines.core to 17.3.0.
- timestamp: 2026-09-11
  finding: `Macros.hlsl`, `CommonDeprecated.hlsl`, and `ForwardPlusKeyword.deprecated.hlsl` all exist and are readable in the resolved PackageCache directories.
- timestamp: 2026-09-11
  finding: Editor.log lines 654, 659, and 664 contain the only shader include errors. They occur immediately after the first four shader compiler processes launch; later asset refreshes contain no recurrence.

# Eliminated

- hypothesis: URP and render-pipelines.core are locked to incompatible versions.
  reason: Both resolve to 17.3.0.
- hypothesis: The package cache permanently lacks the referenced include files.
  reason: All three files currently exist and can be read.
- hypothesis: A project-authored shader has an invalid include.
  reason: Every reported include originates inside Unity's built-in URP/Core packages.

# Resolution

- root_cause: Most likely a transient package-include resolution race during initial Unity shader compiler startup; exact internal Unity cause is not observable from repository files.
- fix: No repository/package edit indicated. Clear the Console and trigger a clean recompilation first.
- verification: Pending one clean recompilation with no repeated shader error.
- files_changed: `.planning/debug/urp-macros-include-missing.md` only (diagnostic record).
