# Phase 16: 일정 간격 자동저장 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-09-14
**Phase:** 16-interval-autosave
**Areas discussed:** 저장 대상 슬롯/파일, 저장 시점 조건 + 간격, 자동저장이 기록하는 내용, 사용자 피드백 / 설정 노출

---

## 영역 선택

| Option | Description | Selected |
|--------|-------------|----------|
| 저장 대상 슬롯/파일 | 현재 슬롯 덮어쓰기 vs 별도 자동저장 파일 | ✓ |
| 저장 시점 조건 + 간격 | 순수 고정 간격 vs 안전 조건 게이트, 간격 길이 | ✓ |
| 자동저장이 기록하는 내용 | 체력·스폰지점 처리 | ✓ |
| 사용자 피드백 / 설정 노출 | 화면 표시 유무, setting.json 노출 | ✓ |

**사용자의 선택:** 4개 영역 전부.

---

## 저장 대상 슬롯/파일

### Q1. 자동저장은 어느 파일에 기록하나요?

| Option | Description | Selected |
|--------|-------------|----------|
| 현재 슬롯 덮어쓰기 (추천) | `CurrentSlot`의 `save_N.json`을 그대로 덮어씀. 사망 로드/이어하기 경로 무수정, 슬롯 카드 진행도 자동 최신화. 나쁜 순간의 자동저장이 수동 세이브를 덮는 위험은 영역 2/3으로 방어 | ✓ |
| 슬롯별 별도 자동저장 파일 | `save_N_auto.json`을 따로 둠. 수동 세이브 보호되지만 사망/이어하기가 둘 중 어느 쪽을 읽을지 규칙이 새로 필요하고 슬롯 카드 UI도 수정 필요 | |
| 전용 자동저장 슬롯 1개 | 슬롯 3개와 별도 칸 하나. 슬롯 독립성(P14 D-06)이 깨짐 | |

**User's choice:** 현재 슬롯 덮어쓰기
**Notes:** → D-01

### Q2. 덮어쓸 때 직전 파일을 백업으로 남길까요?

| Option | Description | Selected |
|--------|-------------|----------|
| 남기지 않음 (추천) | 백업 되돌리는 UI가 없으면 파일만 늘고 쓸모 없음 | ✓ |
| 직전 1회분 `.bak` 유지 | 손상·오저장 시 수동 복구 여지 (P15 D-05와 같은 방향) | |

**User's choice:** 남기지 않음
**Notes:** → D-02

---

## 저장 시점 조건 + 간격

### Q3. 자동저장을 억제할 조건은 어디까지 두나요?

사전 조사 결과 프로젝트에 전역 '전투 중' 플래그가 없음을 확인하고 선택지에 명시함.

| Option | Description | Selected |
|--------|-------------|----------|
| Playing 상태일 때만 (추천) | `GameStateManager.CurrentState == Playing`. 기존 상태머신 재사용, 새 인프라 0 | ✓ |
| Playing + 전투 중 억제 | 보스전/전투 중에도 스킵. '전투 중' 판정 플래그를 새로 만들어야 함 | |
| 조건 없음 — 순수 고정 간격 | N분마다 무조건. 씬 로딩 중·메뉴 안에서도 동작 | |

**User's choice:** Playing 상태일 때만
**Notes:** → D-03

### Q4. 자동저장 간격은?

| Option | Description | Selected |
|--------|-------------|----------|
| 3분 (추천) | 최악의 경우 3분치 손실. JSON이 작아 쓰기 부담 없음 | ✓ |
| 5분 | 쓰기 빈도를 낮추고 체크포인트를 더 주역으로 유지 | |
| 1분 | 미저장 구간 거의 제거. 나쁜 순간이 박힐 확률도 상승 | |

**User's choice:** 3분
**Notes:** → D-04

### Q5. 수동 저장이 일어나면 타이머를 어떻게 하나요?

| Option | Description | Selected |
|--------|-------------|----------|
| 리셋한다 (추천) | 방금 저장했으니 간격을 처음부터 다시 셈 | ✓ |
| 무관하게 계속 돌린다 | 구현은 단순하지만 저장 직후 또 쓰는 경우 발생 | |

**User's choice:** 리셋한다
**Notes:** → D-05

---

## 자동저장이 기록하는 내용

### 1차 질문 (사용자가 명확화 요청 — 재구성됨)

처음 제시한 체력 질문은 "저체력 자동저장 → 부활 즉사 루프"를 전제로 3개 선택지(저장값 하한
적용 / 현재 체력 그대로 / 체력 갱신 안 함)를 제시했다. 사용자가 답변 전에 **"지금 저장 후 로드
방식이 어떻게 돼? 보스나 몬스터 체력은 유지돼 리셋돼?"** 라고 물었고, 코드를 실측한 결과
전제 자체가 틀렸음이 드러나 이 질문은 폐기됐다.

**실측 결과:**
- `SaveData.PlayerStatsSaveData`에는 `MaxHealth` / `MaxTotalHealth`만 있고 **현재 체력 필드가 없다.**
  `SaveData.cs:21` 주석: "현재 체력은 휘발성, 로드는 항상 풀피 부활" (Phase 15 정책)
- `LoadGame()` → `LoadSceneAsync(SceneName)`으로 씬을 통째 재생성 → 보스·몬스터 HP 전부 초기화.
  격파 기록이 있는 보스만 `IsBossDefeated()` 조회로 스스로 등장하지 않음
  (`TutorialBossController.cs:139`, `WaterMonsterStats.cs:28`, `SpiritStats.cs:15`)

→ 체력 관련 방어 장치(하한 보정, 저체력 시 스킵)를 전부 제거. → D-06

### Q6. 자동저장의 씬/스폰지점 처리는? (1차 질문에서 답변됨)

| Option | Description | Selected |
|--------|-------------|----------|
| SaveAnywhere 로직 재사용 (추천) | 현재 씬 기록, 체크포인트 씬과 다르면 스폰지점 비워 씬 기본 시작 위치로 부활. 검증된 경로, P11 D-05 준수 | ✓ |
| 씬·스폰지점 둘 다 안 건드림 | 부활 위치가 마지막 체크포인트에 고정되지만 '미저장 구간 제거' 목표와 어긋남 | |

**User's choice:** SaveAnywhere 로직 재사용
**Notes:** → D-07

### Q7. 그 결과 부활 지점이 씬 기본 시작 위치로 바뀌는 걸 받아들일까요?

| Option | Description | Selected |
|--------|-------------|----------|
| 받아들인다 (추천) | 2스테이지 체크포인트 후 3스테이지에서 자동저장되면 3스테이지 시작점 부활. 씬을 되돌리지 않으므로 진행은 앞으로만 감 | ✓ |
| 체크포인트 씬에서만 자동저장 | 부활 지점은 항상 체크포인트 유지. 새 씬에서 체크포인트를 못 밟으면 그 구간 내내 자동저장 안 걸림 | |

**User's choice:** 받아들인다
**Notes:** → D-08

### Q8. 보스전 도중 자동저장은?

| Option | Description | Selected |
|--------|-------------|----------|
| 그대로 걸린다 (추천) | 체력이 저장되지 않으니 무해. 씬 이름 + 격파 보스 목록만 기록되고, 죽으면 어차피 풀피 재대결 | ✓ |
| 보스전 중에는 억제 | 실익 없이 '보스전 중' 판정 플래그를 새로 만들어야 함 | |

**User's choice:** 그대로 걸린다
**Notes:** → D-09, D-05b

---

## 사용자 피드백 / 설정 노출

사전 조사: 범용 토스트/알림 UI 없음 (`HealPopup`은 전투용 월드 스페이스). 설정 토글은
`screenShakeToggle` 선례 존재 (`GameSettingsPanel.cs:15-16`).

### Q9. 자동저장 발생을 화면에 알려줄까요?

| Option | Description | Selected |
|--------|-------------|----------|
| 화면 구석에 짧은 텍스트 (추천) | 1~2초 표시 후 페이드. 범용 알림 UI가 없어 신규 제작 필요, 영속 캔버스에 올리는 작업 포함 | ✓ |
| 표시 없이 조용히 | `Debug.Log`만. 범위 최소, 새 UI 에셋 불필요 | |

**User's choice:** 화면 구석에 짧은 텍스트
**Notes:** → D-10

### Q10. setting.json에 자동저장 설정을 노출할까요?

| Option | Description | Selected |
|--------|-------------|----------|
| on/off 토글 1개 (추천) | `SettingsData`에 bool + Toggle 하나. 선례와 동일 패턴 | |
| on/off + 간격 선택 | 1/3/5분 선택까지. 설정 UI와 검증 경우의 수 증가 | |
| 설정 없음 — 항상 켜짐 | 고정 동작. 범위 최소 | ✓ |

**User's choice:** 설정 없음 — 항상 켜짐
**Notes:** → D-11. 추천안을 기각한 유일한 항목.

---

---

## 후속 논의 — 호출 형태 (CONTEXT.md 작성 후)

원래 "SaveAnywhere 직접 호출 vs 전용 래퍼 신설"은 Claude 재량 항목이었으나, 사용자가 두 번
질문하며 확정으로 승격됐다.

### F1. "그냥 SaveAnywhere 쓰면 안돼?"

**답:** 된다. `SaveSlotDialog`는 호출자(`GameSettingsPanel.cs:75`)에 있지 `SaveAnywhere()` 안에
없고, 힐도 없다. 래퍼는 불필요한 코드다. → **D-07b 확정**

**다만 확인 과정에서 별개 함정 발견:** `GameStateManager.HandleSceneLoaded()`
(`GameStateManager.cs:79-87`)가 씬 종류 무관하게 모든 씬 로드마다 `CurrentState = Playing`으로
되돌린다 → 메인메뉴에서도 `Playing`이라 D-03 게이트를 통과 → `SceneName = "MainMenu"`가 슬롯에
기록 → 그 슬롯 로드 시 `PlayerStats.Instance == null` → `AbortLoadToMainMenu()` → **슬롯 파손.**
→ **D-03c 신설** (`PlayerStats.Instance != null` 가드 필수화). 재량 항목이었던 것이 필수가 됨.

### F2. "아니면 그냥 Save함수도 있네"

**답: 배제.** `Save()`를 자동저장에 쓰면 완전한 no-op이다.

| 필드 | `Save()`가 3분마다 갱신하나? |
|---|---|
| `SceneName` / `SpawnPointName` | ❌ `Save()`는 건드리지 않음 |
| `MaxHealth` / `MaxTotalHealth` | ❌ 유일한 변경 경로 `PlayerStats.AddHealth()`(`PlayerStats.cs:52`)의 **호출자 0건** |
| `BossProgress` | ❌ `SaveOnBossDefeated()`가 격파 즉시 이미 저장 |
| `MapGimmickState` / `Items` | ❌ 쓰기 훅 없는 스텁 |

전부 ❌ — 직전과 동일한 JSON을 다시 쓰는 것에 불과하고, 죽으면 여전히 마지막 체크포인트로
돌아간다. 자동저장이 의미를 가지려면 `SceneName` 갱신이 필수이며 그걸 하는 기존 메서드는
`SaveAnywhere()` 하나뿐이다. → **D-07c 기각 기록**

### F3. "호출 단위에서 Scene이나 게임 상태를 확인해서 호출하면 SaveAnywhere 써도 되잖아"

**답: 맞고, 그렇게 할 수밖에 없다.** 가드를 호출자(타이머)에 두는 것이 유일하게 가능한 배치다.

**증거:** 일시정지 메뉴 저장은 `CurrentState == Paused` 상태에서 실행된다.
`PauseMenu.cs:62`와 `SaveSlotDialog.cs:38`이 각각 `SetState(GameState.Paused)`를 호출하고,
그 상태에서 `GameSettingsPanel.cs:75`가 `SaveAnywhere()`를 부른다. 따라서 D-03의
`Playing` 게이트를 `SaveAnywhere()` **안에** 넣으면 기존 수동 저장이 통째로 죽는다.

P11 D-01(매니저는 로직만 소유, 호출자가 직접 호출)과도 일치하며, 결과적으로
`SaveLoadManager.cs`는 한 줄도 수정하지 않는다. → **D-03b 신설**

이로써 이번 페이즈의 신규 코드는 **타이머 컴포넌트 + 알림 UI 둘뿐**으로 확정됐다.

---

## Claude's Discretion

- 자동저장 타이머를 소유할 컴포넌트 위치
- ~~`SaveAnywhere()` 직접 호출 vs 전용 진입점 신설~~ → F1에서 D-07b로 확정
- ~~`PlayerStats.Instance` 없는 씬 방어~~ → F1에서 D-03c로 필수화
- 씬 전환 직후 타이머 처리 (이어서 카운트 vs 새로 셈)
- D-05 타이머 리셋을 `Save()` 내부에 걸지 각 호출부에 걸지
- 알림 텍스트 문구·위치·페이드 시간, 영속 캔버스 배치 방식
- 타이머 구현 방식 및 `Time.timeScale = 0` 구간 취급

## Deferred Ideas

- 자동저장 on/off·간격 설정 노출 (D-11 기각)
- 자동저장 별도 파일 / 전용 슬롯 (D-01 기각)
- '전투 중' 전역 판정 플래그 (D-05b 불필요)
- 범용 토스트/알림 시스템으로의 일반화
- 맵 기믹 상태 저장 훅 (P15 D-10 유지)
