---
status: complete
verification: compile-passed-runtime-pending
completed: 2026-09-13
---

# 세이브 포인트 / ESC 저장 슬롯 선택

Implementation commit: `53311c2`.

- Checkpoint interaction and ESC > Game > Save Progress open a shared runtime Canvas with three slot summaries and current-slot indication.
- Empty slots save immediately on selection; existing files (including unreadable data) require overwrite confirmation. Back returns from confirmation to selection, then closes the modal.
- Slot browsing/cancellation does not select a slot or invoke save/checkpoint recovery. Successful saving preserves the chosen CurrentSlot for subsequent save triggers. Write exceptions restore the prior CurrentSlot and display failure feedback.
- Checkpoint save still heals and records its spawn name; its respawn reference updates after success. ESC save preserves the existing SaveAnywhere scene/spawn policy and displays success feedback.
- Modal pauses gameplay, restores the preceding game state and cursor on close, and confines directional navigation to modal buttons. PauseMenu routes ESC to the dialog first.
- No per-scene Inspector setup is required. Existing TMP settings include NotoSansKR fallback.

## Verification

- PASS: runtime C# compilation using generated Assembly-CSharp references and installed Unity Roslyn; new SaveSlotDialog.cs explicitly included. Exit 0; existing project warnings remain.
- PASS: git diff --check and review of both entry points, confirmation/cancel paths, failure slot restoration, and explicit modal navigation.
- Unity MCP refresh did not respond; request was stopped. No successful Unity import, UI rendering, or Play-mode save/load test is claimed. No player save files were intentionally written during verification.

## Remaining Play checks

1. At a checkpoint, open selection then cancel: verify no heal, save, or respawn anchor change.
2. Save to an empty slot: verify checkpoint recovery, selected slot file, and matching load position/progress.
3. Select an occupied slot: cancel confirmation and verify its file is unchanged; then confirm overwrite and load it.
4. ESC > Game > Save Progress: save into another slot, confirm success feedback and continued pause; resume normally.
5. Verify Korean labels, mouse and keyboard navigation, and ESC back behavior at supported resolutions.
6. Check later boss/automatic save triggers use the selected slot. Confirm failed writes display failure and retain the previous CurrentSlot.

Pre-existing unrelated changes, including STATE phase progress, were preserved and excluded from this task's commits.
