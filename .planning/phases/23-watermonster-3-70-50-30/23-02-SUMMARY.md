---
phase: 23-watermonster-3-70-50-30
plan: 02
requirements-completed: [D-07, D-08, D-09, D-10, D-11, D-12, D-13]
commits:
  task-1: "없음 — 오케스트레이터가 커밋"
  task-2: "없음 — 오케스트레이터가 커밋"
files:
  - Assets/Enemy/WaterMonster/Script/States/Attacks/GeyserEffect.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterGeyser.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterJumpLand.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterBurst.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterPrisonAttack.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterPrisonMapAoe.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterColorPrison.cs
  - Assets/Enemy/WaterMonster/Script/States/Attacks/WaterWavePush.cs
  - .planning/phases/23-watermonster-3-70-50-30/23-02-SUMMARY.md
deviation:
  - "사용자 지시에 따라 git add/commit을 실행하지 않음. 커밋 해시는 오케스트레이터가 기입해야 함."
  - "POSIX 셸용 automated 명령을 동일 조건의 PowerShell 명령으로 실행함."
  - "검수 3차 보완에 따라 색깔 감옥 판정·정리는 해당 시전의 존만 대상으로 함."
unverified:
  - "신규 WaterBurst.cs는 현재 Assembly-CSharp.csproj에 포함되지 않아 dotnet build로 컴파일되지 않음. Unity 컴파일은 오케스트레이터 확인 필요."
  - "Unity Play 모드의 타이밍, 보스 비활성화 후 존 소멸, 겹침 중 존 수와 피해 판정은 오케스트레이터 확인 필요."
---

# Phase 23 Plan 02 실행 요약

## Task 1

- `GeyserEffect.Configure`에서 전조 0.7초와 강화 피해·반경을 적용했다. `WaterGeyser`는 프리팹을 즉시 생성하고 타격 시점까지 `StartPattern`으로 실행 상태를 유지한다.
- 점프 착지에 강화 배율과 플레이어별 피해 1회 판정을 적용했다. 흡수 0.6초, 정지 0.4초 후 폭발하는 `WaterBurst`를 추가했다.
- 계획의 automated 조건을 PowerShell로 실행: `T1_OK`. `dotnet build Assembly-CSharp.csproj --nologo -v quiet`: 통과, 오류 0건, 기존 경고 6건.
- 커밋 해시: 없음(오케스트레이터 담당).

## Task 2

- 일반 감옥과 감옥→전체 공격의 트리거를 분리하고, 발사 전 0.6초 응축을 추가했다.
- 색깔 감옥에 4.5초 안전 수명과 `try/finally` 정리를 추가했다. 안전 판정과 정리는 이 시전이 만든 존만 대상으로 한다. 파도는 항상 좌측에서 먼저 출발한다.
- 계획의 automated 조건을 PowerShell로 실행: `T2_OK`. `dotnet build Assembly-CSharp.csproj --nologo -v quiet`: 통과, 오류 0건, 기존 경고 6건.
- 커밋 해시: 없음(오케스트레이터 담당).

## 범위·형식 확인

- 수정된 기존 C# 파일 7개는 BOM 없음·CRLF를 유지했다. `git diff --check` 통과.
- 계획 외 기존 미커밋 변경은 건드리지 않았다. `.csproj`, `.meta`, 씬, 프리팹, 컨트롤러는 수정하지 않았다.
