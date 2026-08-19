# Phase 13 Audit — Findings C: Player / Camera / SaveSystem

**Scope:** `Assets/Player/**`, `Assets/Camera/**`, `Assets/SaveSystem/**`
**Files scanned:** 32
**Risk tier:** 혼합 — Phase 9~12 수정 파일 8종 = 고위험, 나머지 24종 = 일반
**Generated:** 2026-08-19

## D-04 — CP949 인코딩 위험 파일

> 이 파일들은 CP949 인코딩이다. 실제 수정 단계에서는 표준 Read/Edit 왕복이 비-ASCII 바이트를 U+FFFD로 훼손시키므로 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜이 필요하다 (CONTEXT.md D-04, STATE.md Phase 11 Plan 3 기록).

| # | File | 비고 |
|---|------|------|
| 1 | Assets/Player/Script/PlayerAnimator.cs | 일반 |
| 2 | Assets/Player/Script/PlayerSpeedDisplay.cs | 일반 |
| 3 | Assets/Player/Script/UI/PlayerUI.cs | 일반 |
| 4 | Assets/Player/Script/WaterController.cs | 일반 |

> `Assets/Camera/Script/CameraController.cs` 는 UTF-8이지만 비-ASCII(한글) 주석 라인이 5줄 존재하며, Phase 10/12에서 "비-ASCII 라인 수 5 유지"가 회귀 게이트였다 — 수정 시 동일 게이트를 재적용할 것.

## D-07 — 죽은 코드

### D-07 일반 항목

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| C-D07-01 | Assets/Player/Script/PlayerAnimator.cs | 45 | `PlayAttackAnimation` | XML 주석에 "외부에서 호출"이라 적혀 있으나 프로젝트 전체 참조 0건, 씬/프리팹/애니메이션 이벤트에서도 호출되지 않음 |
| C-D07-02 | Assets/Player/Script/PlayerAttack.cs | 50 | `defaultLayer` (private field) | 선언만 있고 대입/읽기 모두 없음, 프로젝트 전체 참조 0건 |
| C-D07-03 | Assets/Player/Script/PlayerAttack.cs | 67-82 | 주석 처리된 코드 블록 (`/* 버전 A: 마우스 방향 조준형 */`) | `OnBasicAttack()` 상단에 구버전 공격 로직 전체가 블록 주석으로 남아있음 (16줄) |
| C-D07-04 | Assets/Player/Script/SkillScript/WaveSlice.cs | 12 | `waveSlice` (public method) | 프로젝트 전체 참조 0건, 씬/프리팹 UnityEvent 바인딩 없음, 애니메이션 이벤트 호출 없음 |
| C-D07-05 | Assets/Player/Script/UI/PlayerUI.cs | 4 | `PlayerUI` 클래스 전체 | 클래스 자체가 씬/프리팹 어디에도 부착되지 않음 (참조 0건) — 아래 두 메서드도 이 클래스 소속이라 함께 죽음 |
| C-D07-06 | Assets/Player/Script/UI/PlayerUI.cs | 15 | `InitWaterIcon` | PlayerUI 클래스가 고아이므로 이 메서드도 호출부 없음 |
| C-D07-07 | Assets/Player/Script/UI/PlayerUI.cs | 30 | `updateWater` | 위와 동일 사유 |

### D-07 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| C-D07-08 | Assets/SaveSystem/Script/SaveLoadManager.cs | 118 | `IsBossDefeated(string bossId)` (public method) | 프로젝트 전체 참조 0건(선언부뿐) — `SaveOnBossDefeated` 는 쓰기만 하고, 이 조회 메서드를 읽는 호출부가 어디에도 없음 |
| C-D07-09 | Assets/SaveSystem/Script/SaveData.cs | 12 | `SaveVersion` (public field, 기본값 1) | 프로젝트 전체 참조 1건(선언부뿐), 직렬화/역직렬화 시 값은 채워지지만 아무도 읽지 않음 — 스키마 버저닝을 염두에 둔 선제적 필드로 보이나 CONTEXT.md에 명시된 의도 문서는 없음. 삭제보다는 "아직 안 쓰는 필드" 확인 차원 |
| C-D07-10 | Assets/SaveSystem/Script/SaveData.cs | 29 | `MapGimmickState` (public field) | 의도된 확장 스텁 (Phase 11 D-03/D-03b) — 삭제 권장 아님. `SaveLoadManager.EnsureCollections()`/`DebugLogState()` 에서만 다뤄지고 실제 기믹 상태를 채우는 코드는 아직 없음 |
| C-D07-11 | Assets/SaveSystem/Script/SaveData.cs | 32 | `Items` (public field) | 의도된 확장 스텁 (Phase 11 D-03/D-03b) — 삭제 권장 아님. 위와 동일 |

## D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

> TODO/FIXME/HACK 주석: 이 범위에서 0건 (프로젝트 전체도 0건).

> `Assets/Camera/**` 는 Debug.Log 0건 (Phase 9~12 내내 로그를 넣지 않은 상태 유지).

### D-08 일반 항목

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| C-D08-01 | Assets/Player/Script/Menu/PauseMenu.cs | 12, 16, 29 | Debug.LogError/Debug.Log | InputHandler 구독 실패/상태 추적 로그 — 개발용 상태 추적, 12/29는 제거 권장, 16(LogError)은 오류 진단용 유지 권장 |
| C-D08-02 | Assets/Player/Script/PlayerAttack.cs | 120, 188, 189, 190, 296, 372 | Debug.Log/Debug.LogError | 120("막타, 쿨다운")·296("Q 준비완료!")·372(스플래시 적용 로그)는 개발용 상태 추적 — 제거 권장. 188-190(레이어 없음 LogError 3건)은 오류 진단용 — 유지 권장 |
| C-D08-03 | Assets/Player/Script/PlayerAttackDamager.cs | 15, 21, 38, 43 | Debug.Log 계열 | 데미지 타입 오버라이드/적용 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-04 | Assets/Player/Script/PlayerController.cs | 72 | Debug.LogError | "InputHandler가 씬에 없습니다!" — 오류 진단용, 유지 권장 |
| C-D08-05 | Assets/Player/Script/PlayerInputHandler.cs | 11 | Debug.Log 계열 | 구독 관련 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-06 | Assets/Player/Script/PlayerStats.cs | 62 | Debug.Log 계열 | TakeDamage 경로 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-07 | Assets/Player/Script/SkillScript/FlashSlice.cs | 32 | Debug.Log 계열 | 스킬 사용 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-08 | Assets/Player/Script/SkillScript/WaveSlice.cs | 22, 36, 45, 59 | Debug.Log 계열 | 죽은 메서드 `waveSlice()`(C-D07-04)와 같은 파일의 다른 메서드들에 있는 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-09 | Assets/Player/Script/UI/PlayerHealthUI.cs | 26 | Debug.Log 계열 | 하트 초기화 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-10 | Assets/Player/Script/UI/PlayerWaterUI.cs | 59 | Debug.Log 계열 | 물병 초기화 추적 로그 — 개발용 상태 추적, 제거 권장 |
| C-D08-11 | Assets/Player/Script/WaterController.cs | 37, 51 | Debug.Log 계열 | 물 회복 관련 추적 로그 (CP949 파일, 줄번호는 정확함) — 개발용 상태 추적, 제거 권장 |

### D-08 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| C-D08-12 | Assets/Player/Script/InputHandler.cs | 80, 87, 173 | Debug.LogError | Input Action Asset 미할당/Player 액션맵 없음 오류 — 오류 진단용, 유지 권장 |
| C-D08-13 | Assets/Player/Script/InputHandler.cs | 124, 183, 191 | Debug.Log | ESC 키 입력 등 상태 추적 로그 — 개발용 상태 추적, 제거 권장 (단 InputHandler는 고위험 파일이므로 제거 시 회귀 검증 필요) |
| C-D08-14 | Assets/SaveSystem/Script/SaveLoadManager.cs | 84, 93, 129, 144, 156, 162, 171, 200, 218, 223, 253 | Debug.Log/LogWarning/LogError | 저장/로드 성공·실패 진단 로그 11건 — 대부분 `LogError`/`LogWarning`(파일 없음, 역직렬화 null, 씬 로드 실패 등)이라 오류 진단용 유지 권장. 129/223 등 일부는 상태 확인용(`HasSaveFile` 결과, `DebugLogState` 출력)으로 개발 편의 로그에 가까움 |
| C-D08-15 | Assets/Player/Script/PlayerAttack.cs | 67-82 | 주석처리 코드 | C-D07-03 과 동일 항목 (구버전 마우스 조준 공격 로직) — PlayerAttack은 일반 티어 파일이지만 이 블록은 고위험 티어인 InputHandler 구독 체인(`PlayerInputHandler` 상속)과 얽혀 있어 신중 검토 목록에도 교차 기재 |

## D-10 — 과도하게 긴/복잡한 함수 (권장)

> D-10은 권장 수준 관찰이다. 실제 수정은 사용자 승인 필수 (CONTEXT.md D-10).

### D-10 일반 항목

| ID | File | Line(s) | Symbol | Metric | Reason |
|----|------|---------|--------|--------|--------|
| C-D10-01 | Assets/Player/Script/PlayerAttack.cs | 62-140 | `OnBasicAttack` | 77줄 / 분기 8 | 콤보 카운트 판정 + 방향/스폰 위치 계산 + 3타 특수 배율 + Water Boost 버프 적용 + 코루틴 시작이 한 메서드에 혼재. 상단에 죽은 구버전 블록(C-D07-03)까지 포함되어 실질 가독성은 더 나쁨 |

### D-10 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Metric | Reason |
|----|------|---------|--------|--------|--------|
| C-D10-02 | Assets/Player/Script/InputHandler.cs | 61-104 | `Awake` | 42줄 / 분기 4 | 싱글톤 초기화 + InputActionAsset 로드 + "Player" 액션맵 조회 + 액션 10개 바인딩이 한 메서드에 혼재. InputHandler는 Phase 10에서 "0줄 변경"이 성공 기준이었던 고위험 파일이므로 분해 시 프로젝트 전체 입력 이벤트 구독자 전원(PlayerController/PlayerAttackBase/PrisonDebuff/CameraController/PlaceWaterMonster 등)에 대한 회귀 검증 필요 |

> 참고 — `CameraController.LateUpdate`(444-470줄, 실측 26줄 / 분기 3)는 D-10 수치 기준을 충족하지 않는다(40줄 미만, 분기 15 미만). 다만 Phase 9~12가 누적된 파이프라인 순서 계약이 있다: LateUpdate 실행 순서(위치추종/데드존 → 줌 Lerp → 경계 클램프 → 데드존 재앵커 → Hit Shake)가 계약이므로 분해 시 순서 보존 필수. 수치 기준이 아닌 설계 취약성 판단이므로 다른 D-10 항목과 구분해 표기함.

## D-09 후보 관찰 (raw — Plan 05에서 교차 검증)

- `Assets/Player/Script/InputHandler.cs` 와 `Assets/Player/Script/PlayerInputHandler.cs`: 이름이 매우 비슷하지만 실제로는 다른 역할이다. `InputHandler.cs:9`(`InputHandler.Instance`, DontDestroyOnLoad 싱글톤, Unity Input System 이벤트 버스, 78회 참조)는 원시 입력 이벤트 발행자이고, `PlayerInputHandler.cs:7`(`abstract class PlayerInputHandler : MonoBehaviour`, `PlayerAttack.cs:4` 가 상속)는 공격 관련 추상 콜백(`OnBasicAttack`/`OnSkillE`/`OnSkillR`/`OnHeal`/`OnSkillQ`) 계약일 뿐 InputHandler 이벤트를 직접 구독하지 않는다(구독은 `PlayerAttackBase.cs:27-41`가 담당). **진짜 중복이 아니라 이름만 비슷한 별개 클래스** — 다만 이름이 헷갈리므로 리네이밍은 검토해볼 만하다.
- `Assets/Player/Script/UI/PlayerHealthUI.cs:44`(`InitHearts`)/`:72`(`UpdateHealthUI`) 와 `Assets/Player/Script/UI/PlayerWaterUI.cs:43`(`InitBottles`)/`:64`(`UpdateWaterUI`): 하트 슬롯과 물병 슬롯을 각각 `List<GameObject>` 로 만들고 `Instantiate` 후 `Update*UI()` 에서 채움 상태를 다시 그리는 거의 동일한 골격.
- `Assets/Player/Script/Menu/GameSettingsPanel.cs`, `Assets/Player/Script/Menu/GraphicsSettingsPanel.cs`, `Assets/Player/Script/Menu/SoundSettingsPanel.cs` 3종 모두 `PlayerPrefs` 기반 값 로드→적용→저장 골격을 각자 구현 (`ControlsSettingsPanel.cs`/`PauseMenu.cs`/`PauseMenuTabController.cs`는 PlayerPrefs를 쓰지 않아 이 패턴에서 제외).
- `Assets/Camera/Script/CameraBoundsTrigger.cs:39,57`(`OnTriggerEnter2D`/`OnTriggerExit2D`) 와 `Assets/Camera/Script/CameraZoomTrigger.cs:17,24`(동일 메서드명): 트리거 진입/이탈 골격 자체는 비슷하나, `CameraZoomTrigger`는 필드 0개로 설계된 게 의도(E-08 배제 대상)이므로 골격 유사성만 관찰로 남김 — 통합 권장 아님.

## 기타 — 코드 외 정리 항목

(없음 — Scope C에는 씬/빌드 설정 문제가 없다. `Assets/Scenes/InGame.unity` 스테일 엔트리는 Scope D 담당.)

## 스캔 커버리지

| # | File | Lines | Risk | Scanned |
|---|------|-------|------|---------|
| 1 | Assets/Camera/Script/CameraBoundsTrigger.cs | 113 | 고위험 | yes |
| 2 | Assets/Camera/Script/CameraController.cs | 470 | 고위험 | yes |
| 3 | Assets/Camera/Script/CameraZoomTrigger.cs | 46 | 고위험 | yes |
| 4 | Assets/Player/Animation/PlayerIdle.cs | 51 | 일반 | yes |
| 5 | Assets/Player/Script/AttackBox.cs | 32 | 일반 | yes |
| 6 | Assets/Player/Script/GameStateManager.cs | 68 | 일반 | yes |
| 7 | Assets/Player/Script/InputHandler.cs | 197 | 고위험 | yes |
| 8 | Assets/Player/Script/LadderMovement.cs | 67 | 일반 | yes |
| 9 | Assets/Player/Script/Menu/ControlsSettingsPanel.cs | 124 | 일반 | yes |
| 10 | Assets/Player/Script/Menu/GameSettingsPanel.cs | 79 | 일반 | yes |
| 11 | Assets/Player/Script/Menu/GraphicsSettingsPanel.cs | 63 | 일반 | yes |
| 12 | Assets/Player/Script/Menu/PauseMenu.cs | 59 | 일반 | yes |
| 13 | Assets/Player/Script/Menu/PauseMenuTabController.cs | 59 | 일반 | yes |
| 14 | Assets/Player/Script/Menu/SoundSettingsPanel.cs | 67 | 일반 | yes |
| 15 | Assets/Player/Script/PlayerAnimator.cs | 48 | 일반 | yes |
| 16 | Assets/Player/Script/PlayerAttack.cs | 375 | 일반 | yes |
| 17 | Assets/Player/Script/PlayerAttackBase.cs | 49 | 일반 | yes |
| 18 | Assets/Player/Script/PlayerAttackDamager.cs | 50 | 일반 | yes |
| 19 | Assets/Player/Script/PlayerController.cs | 298 | 고위험 | yes |
| 20 | Assets/Player/Script/PlayerInputHandler.cs | 35 | 일반 | yes |
| 21 | Assets/Player/Script/PlayerSpeedDisplay.cs | 63 | 일반 | yes |
| 22 | Assets/Player/Script/PlayerStats.cs | 77 | 고위험 | yes |
| 23 | Assets/Player/Script/PrisonDebuff.cs | 79 | 일반 | yes |
| 24 | Assets/Player/Script/SkillScript/FlashSlice.cs | 35 | 일반 | yes |
| 25 | Assets/Player/Script/SkillScript/WaveSlice.cs | 61 | 일반 | yes |
| 26 | Assets/Player/Script/UI/InGameHUDController.cs | 37 | 일반 | yes |
| 27 | Assets/Player/Script/UI/PlayerHealthUI.cs | 111 | 일반 | yes |
| 28 | Assets/Player/Script/UI/PlayerUI.cs | 46 | 일반 | yes |
| 29 | Assets/Player/Script/UI/PlayerWaterUI.cs | 99 | 일반 | yes |
| 30 | Assets/Player/Script/WaterController.cs | 98 | 일반 | yes |
| 31 | Assets/SaveSystem/Script/SaveData.cs | 40 | 고위험 | yes |
| 32 | Assets/SaveSystem/Script/SaveLoadManager.cs | 263 | 고위험 | yes |

## 요약

| 카테고리 | 항목 수 | 고위험 항목 수 |
|----------|---------|----------------|
| D-07 | 11 | 4 |
| D-08 | 15 | 4 |
| D-10 | 2 | 1 |
