---
phase: 16-interval-autosave
plan: 01
subsystem: save-system
tags: [autosave, coroutine, unity]
requires: [phase-15-save-load]
provides: [interval-autosave-timer, save-countdown-reset]
affects: [16-02, 16-03]
key-files:
  created: [Assets/SaveSystem/Script/AutoSaveTimer.cs, Assets/SaveSystem/Script/AutoSaveTimer.cs.meta]
  modified: [Assets/SaveSystem/Script/SaveLoadManager.cs]
metrics:
  tasks_completed: 2
  completed: 2026-09-14
requirements-completed: [D-01, D-03, D-03b, D-03c, D-04, D-05, D-07, D-07b, D-07c]
---

# Phase 16 Plan 01 — 자동저장 타이머

별도 영속 AutoSave 오브젝트가 1초 틱으로 플레이 가능 시간을 누적하고, 180초마다 현재 슬롯에 SaveAnywhere()를 호출한다. 상태 게이트는 타이머에만 있으며 Save()의 디스크 쓰기 성공 후 카운트를 초기화한다.

## Task commits

| Task | Commit | 변경 |
|---|---|---|
| 1 | b05668b | AutoSaveTimer 및 고유 GUID 생성 |
| 2 | a388529 | Save() 말미에 +8/-0줄: 리셋 알림 1문장과 주석 |

## 검증

- 신규 타이머가 실제 Assembly-CSharp.csproj Compile 항목에 포함됨. csproj는 Unity 생성·Git 제외 파일로 유지.
- 각 태스크 dotnet build: 오류 0, 기존 경고 6. 작업 전 --no-incremental 재빌드에서도 같은 경고 6개 확인.
- 26개(Task 1) / 29개(Task 2) 정적 검사 PASS: 계획 코드와 일치, 180초/1초 상수, Playing/PlayerStats 가드, 직접 저장 호출, 중복 방지·영속성·정적 리셋, GUID 유일성, 인코딩, 기존 Assets SHA-256 보존.
- SaveAnywhere/체크포인트/보스/로드/설정 메서드는 기존 코드와 동일. SaveLoadManager 변경은 지정된 +8/-0줄뿐.
- Unity Play 모드 실측은 16-03에서 대기. 런타임 동작 통과를 주장하지 않음.

## Deviations

- [BUG-010](bugs/BUG-010-plan-acceptance-false-positives.md): 계획의 grep는 주석까지 세므로 제시 코드 자체와 충돌한다. 원문 코드는 그대로 구현하고 금지 호출/상태 게이트 검증은 주석을 제외한 코드로 실행했다. 전체 원문 명령 결과는 Check.md에 기록 예정.
- 기존 미커밋 Assets 변경은 작업 전 SHA-256과 비교했다. 사용자의 기존 변경을 커밋하지 않았다.
- Python WindowsApps 별칭을 실행할 수 없어 Node.js로 파일 생성·바이트 검증을 수행했다.

## Self-Check: PASSED

구현 두 태스크 및 컴파일/정적 검증 완료. 두 코드 커밋 존재. Phase 16 전체 완료 여부는 Play 모드 체크포인트 이후 결정한다.
