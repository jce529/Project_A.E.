# Phase 13 — 코드베이스 정리 감사 보고서

**Phase:** 13-codebase-cleanup-audit
**Generated:** 2026-08-19 (2026-08-20 갱신 — D-08 일괄 처리 결과 반영, 이후 D-07/D-08 잔여 항목 개별 처리로 마무리)
**Scope:** `Assets/` 아래 전체 C# 스크립트 168개 파일 / 12372줄 (CONTEXT.md D-03)
**Status:** **정리 작업 마무리 (2026-08-20).** D-07(죽은 코드)·D-08(TODO/디버그 잔재)은 고위험 항목까지 포함해 전부 개별 판단·실행 완료. D-09(중복 로직)·D-10(긴 함수 리팩토링)는 성격상 "권장" 등급 리팩토링이라 이번 정리 범위에서 의도적으로 제외 — 승인/미승인 상태만 표기된 채 백로그로 남음 (미체크 15건, 아래 "남은 백로그" 참고). 실행 상세는 `## 2026-08-20 처리 완료 로그` 참고 (CONTEXT.md D-01/D-02)

## 이 보고서를 읽는 법

- 항목마다 체크박스가 있다. **승인하려는 항목만 `[x]` 로 바꿔서 알려주면** 그 항목만 수정 작업으로 넘어간다.
- `[x]`는 카테고리에 따라 의미가 다르다: D-07/D-09는 "삭제/리팩토링 승인됨(미실행 가능)", D-08은 "2026-08-20 일괄 처리 완료(제거 또는 확인-유지)". 실행 여부는 `## 2026-08-20 처리 완료 로그` 및 각 행의 부기 참고.
- `## 회귀 위험 높음` 섹션 항목은 이미 Play 모드로 검증된 코드에 있는 것이다 — 더 신중하게 판단해야 한다 (D-05/D-06).
- `삭제 권장 아님` 이 붙은 항목은 "발견은 됐지만 의도적으로 남긴 것"이다. 재확인용으로만 나열되어 있다.
- D-10은 **권장** 등급이다 (주관 판단이 개입, CONTEXT.md D-10).
- ⚠ 표시가 붙은 D-08 행은 2026-08-20 실행 결과가 원래 권장(유지/제거)과 어긋난 것으로 확인된 사례다 — `## 2026-08-20 처리 완료 로그`의 "정책 이탈 사례" 참고.

## 전체 통계

| 지표 | 값 | 비고 |
|------|-----|------|
| 스캔한 .cs 파일 | 168 | 계획서 값과 일치 (재측정: `find Assets -name "*.cs" \| wc -l`) |
| 총 라인 수 | 12372 | 계획서 값과 일치 |
| CP949(비-UTF-8) 파일 | 46 | 계획서 값과 일치 (분포는 fragment별 실측으로 일부 정정 — 아래 D-04 참고) |
| `Debug.Log*` 발생 건수 | 226 | 계획서 값과 일치 |
| `Debug.Log*` 포함 파일 수 | 92 | 계획서 값과 일치 |
| TODO/FIXME/HACK/XXX | 0 | 계획서 값과 일치 |

| 카테고리 | 총 항목 | 일반 | 회귀 위험 높음 |
|----------|---------|------|----------------|
| D-07 죽은 코드 | 45 | 29 | 16 |
| D-08 TODO/디버그 잔재 | 96 | 46 | 50 |
| D-09 중복 로직 | 9 | 4 | 5 |
| D-10 긴/복잡한 함수 (권장) | 12 | 5 | 7 |

> Scope B fragment 자체 요약표는 D-08을 33건(일반 7)으로 기재했으나, 그중 1건(`B-D08-07`)은 실제 발견이 아니라 "TODO/FIXME 0건" 사실을 표 형식으로 적어둔 placeholder 행이라 이 보고서에서는 실제 항목 표에 옮기지 않고 산문 각주("TODO/FIXME/HACK 주석: 프로젝트 전체 0건")로 대체했다. 따라서 이 보고서의 D-08 합계(96)는 fragment 원본 합계(97)보다 1 적은 것이 정확하다.

## 일반 항목

### D-07 — 죽은 코드

| 승인 | ID | File | Line(s) | Symbol | Reason |
|------|----|------|---------|--------|--------|
| [x] | B-D07-01 | Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs | 12-162 | `EnemyBrain` (클래스 전체) | 프로젝트 전체 코드 참조 0건(선언부만), `.cs.meta` GUID로 전체 `.unity`/`.prefab` 검색해도 어디에도 컴포넌트로 부착되지 않음 — 씬/코드 양쪽에서 완전히 고립된 클래스 |
| [x] | B-D07-02 | Assets/Enemy/Monster_Alpha/Script/patorl.cs | 3-63 | `PatrolMovement` (클래스 전체) | 프로젝트 전체 코드 참조 0건, GUID 검색으로도 씬/프리팹 부착 0건 |
| [x] | C-D07-01 | Assets/Player/Script/PlayerAnimator.cs | 45 | `PlayAttackAnimation` | XML 주석에 "외부에서 호출"이라 적혀 있으나 프로젝트 전체 참조 0건, 씬/프리팹/애니메이션 이벤트에서도 호출되지 않음 |
| [x] | C-D07-02 | Assets/Player/Script/PlayerAttack.cs | 50 | `defaultLayer` (private field) | 선언만 있고 대입/읽기 모두 없음, 프로젝트 전체 참조 0건 |
| [x] | C-D07-03 | Assets/Player/Script/PlayerAttack.cs | 67-82 | 주석 처리된 코드 블록 (`/* 버전 A: 마우스 방향 조준형 */`) | `OnBasicAttack()` 상단에 구버전 공격 로직 전체가 블록 주석으로 남아있음 (16줄) → 2026-08-20 사용자 승인, 블록 주석 + 라벨 헤더(`[버전 A]` 주석 3줄) 함께 제거 완료 |
| [x] | C-D07-04 | Assets/Player/Script/SkillScript/WaveSlice.cs | 12 | `waveSlice` (public method) | 프로젝트 전체 참조 0건, 씬/프리팹 UnityEvent 바인딩 없음, 애니메이션 이벤트 호출 없음 |
| [x] | C-D07-05 | Assets/Player/Script/UI/PlayerUI.cs | 4 | `PlayerUI` 클래스 전체 | 클래스 자체가 씬/프리팹 어디에도 부착되지 않음 (참조 0건) — 아래 두 메서드도 이 클래스 소속이라 함께 죽음 |
| [x] | C-D07-06 | Assets/Player/Script/UI/PlayerUI.cs | 15 | `InitWaterIcon` | PlayerUI 클래스가 고아이므로 이 메서드도 호출부 없음 |
| [x] | C-D07-07 | Assets/Player/Script/UI/PlayerUI.cs | 30 | `updateWater` | 위와 동일 사유 |
| [x] | D-D07-01 | Assets/map/script/portal.cs | 4 | `Portal` (class) | 기존 식별 고아 코드 재확인 (Assets/SaveSystem/Check.md:108) — 프로젝트 전체 코드 참조 0건(선언부 제외), 스크립트 GUID 기준 씬/프리팹 부착 0건. `grep -rl Portal Assets --include=*.unity`가 히트를 반환하지만 전부 `SignpostPortal`(별개의 살아있는 클래스) 매치이며 실제 `Portal` 컴포넌트 부착은 아님(GUID 검증으로 확인) |
| [x] | D-D07-02 | Assets/Script/CorruptedWater.cs | 3 | `CorruptedWater` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `PureWater.cs`와 로직·로그 문구까지 거의 동일 (D-09-D-D09-01 참고) |
| [x] | D-D07-03 | Assets/Script/PureWater.cs | 3 | `PureWater` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `CorruptedWater.cs`와 로직·로그 문구까지 거의 동일 (D-09-D-D09-01 참고) |
| [x] | D-D07-04 | Assets/Script/Txt/DOUBLESPACE.cs | 4 | `DOUBLESPACE` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `Assets/Script/Txt/` 5개 파일 전부 동일 패턴(D-D07-04~08) — 계획서는 D-09(중복) 후보로 지목했으나 실측 결과 전부 죽은 코드로 재분류 |
| [x] | D-D07-05 | Assets/Script/Txt/SHIFT.cs | 4 | `SHITF` (class, 오탈자 그대로) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-06 | Assets/Script/Txt/SPACE.cs | 4 | `SPACE` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-07 | Assets/Script/Txt/WASD.cs | 4 | `WASD` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-08 | Assets/Script/Txt/TxtController.cs | 4 | `TutorialTrigger` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 파일명(TxtController)과 실제 클래스명(TutorialTrigger)이 다름 |
| [x] | D-D07-09 | Assets/Script/TakeDmg.cs | 3 | `GiveDmg` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `EnemyDamager.cs`와 패턴이 겹치지만 이 파일 자체가 죽어있어 실질 DRY 위반은 아님 |
| [x] | D-D07-10 | Assets/Script/InteractivePrompt.cs | 3 | `InteractionPrompt` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 파일명과 클래스명 불일치 |
| [x] | D-D07-11 | Assets/Script/ObstacleInteraction.cs | 3 | `ObstacleInteraction` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-12 | Assets/Script/PlatformController.cs | 4 | `PlatformController` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-13 | Assets/Script/ProtoEnemy.cs | 4 | `ProtoEnemy` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 이름 그대로 초기 프로토타입 계열 |
| [x] | D-D07-14 | Assets/map/script/3 stage/PipeSwitch.cs | 3 | `PipeSwitch` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `3 stage pump.unity` 등 관련 씬은 존재하나 이 컴포넌트는 어디에도 부착돼 있지 않음 |
| [x] | D-D07-15 | Assets/map/script/PumpManager1.cs | 3 | `PumpManager` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| [x] | D-D07-16 | Assets/map/script/3 stage/ShrineManager.cs | 3 | `ShrineManager` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `3 stage shrine.unity` 씬은 존재하나 이 컴포넌트는 부착돼 있지 않음 |
| [x] | D-D07-17 | Assets/Editor/BuildPhase2Assets.cs | 7 | `BuildPhase2Assets` (class) | `[MenuItem]` 진입점 2개 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| [x] | D-D07-18 | Assets/Editor/BuildWaterMonsterAssets.cs | 7 | `BuildWaterMonsterAssets` (class) | `[MenuItem]` 진입점 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| [x] | D-D07-19 | Assets/Editor/Phase1CLI.cs | 4 | `Phase1CLI.ExecuteAll` | `[MenuItem]`/`[InitializeOnLoad]` 없음, 코드 참조 0건 — Unity `-executeMethod` CLI 배치 호출용으로 추정. Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| [x] | D-D07-20 | Assets/Editor/PlaceWaterMonsterInScene.cs | 6 | `PlaceWaterMonsterInScene` (class) | `[MenuItem]` 진입점 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요. 존재하지 않는 `Assets/Scenes/InGame.unity` 를 대상으로 함 (## 기타 참고) |

### D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

> `Assets/Camera/**` 는 Debug.Log 0건 (Phase 9~12 내내 로그를 넣지 않은 상태 유지). `Assets/ImportedAsset/**` 도 0건.
> TODO/FIXME/HACK 주석: 프로젝트 전체 0건.

| 승인 | ID | File | Line(s) | Kind | Reason |
|------|----|------|---------|------|--------|
| [x] | B-D08-01 | Assets/Enemy/Boss/Script/FloorHandAttack.cs | 29 | Debug.Log | 개발용 상태 추적("바닥 손 공격 시작!") — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | B-D08-02 | Assets/Enemy/Boss/Script/hand.cs | 31 | Debug.LogWarning | "Player not found" — 오류 진단용 — 유지 권장 → 유지 확인(미변경) |
| [x] | B-D08-03 | Assets/Enemy/Monster_Alpha/Script/bullet.cs | 17, 33, 50 | Debug.Log/LogError | 17/33=Rigidbody2D 누락 오류 진단용(유지 권장), 50=피격 로그(제거 권장) → 50만 제거, 17/33 유지 확인 |
| [x] | B-D08-04 | Assets/Enemy/Monster_Alpha/Script/enemy.cs | 25, 73 | Debug.LogError/Log | 25=Player 태그 미설정 오류 진단용(유지 권장), 73=사망 로그(제거 권장) → 73만 제거, 25 유지 확인 |
| [x] | B-D08-05 | Assets/Enemy/Monster_Alpha/Script/EnemyBrain.cs | 140, 146 | Debug.LogWarning/Log | 클래스 자체가 B-D07-01로 죽은 코드 — 정리 시 클래스와 함께 제거됨 → 2026-08-20 로그 2건 제거(클래스 자체 삭제는 B-D07-01 별도 미실행) |
| [x] | B-D08-06 | Assets/Enemy/Script/EnemyHitBox.cs | 18 | Debug.LogWarning | BossStatsSystem 탐색 실패 — 오류 진단용 — 유지 권장 → 유지 확인(미변경) |
| [x] | C-D08-01 | Assets/Player/Script/Menu/PauseMenu.cs | 12, 16, 29 | Debug.LogError/Log | 12/29=상태 추적(제거 권장), 16(LogError)=오류 진단용(유지 권장) → 12/29 제거, 16 유지 확인 |
| [x] | C-D08-02 | Assets/Player/Script/PlayerAttack.cs | 120, 188, 189, 190, 296, 372 | Debug.Log/LogError | 120/296/372=상태 추적(제거 권장), 188-190(레이어 없음 LogError 3건)=오류 진단용(유지 권장) → 120/296/372 제거, 188-190 유지 확인 |
| [x] | C-D08-03 | Assets/Player/Script/PlayerAttackDamager.cs | 15, 21, 38, 43 | Debug.Log | 데미지 타입 오버라이드/적용 추적 로그 — 제거 권장 → 2026-08-20 4건 전량 제거 |
| [x] | C-D08-04 | Assets/Player/Script/PlayerController.cs | 72 | Debug.LogError | "InputHandler가 씬에 없습니다!" — 오류 진단용, 유지 권장 → 유지 확인(미변경) |
| [x] | C-D08-05 | Assets/Player/Script/PlayerInputHandler.cs | 11 | Debug.LogError (정정 — 원 태깅 Debug.Log는 오류) | `InputHandler.Instance == null` 널 가드 오류 로그 — 오류 진단용, 유지 권장 (다른 파일들의 동일 패턴 널 가드 로그와 일관). **재태깅 완료 (2026-08-20): "추적 로그/제거 권장"은 오판정, 실제로는 유지 권장 카테고리 — 코드 미변경(원래도 D-08 배치에서 제외되어 남아있었음)** |
| [x] | C-D08-06 | Assets/Player/Script/PlayerStats.cs | 62 | Debug.Log | TakeDamage 경로 추적 로그 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | C-D08-07 | Assets/Player/Script/SkillScript/FlashSlice.cs | 32 | Debug.Log | 스킬 사용 추적 로그 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | C-D08-08 | Assets/Player/Script/SkillScript/WaveSlice.cs | 22, 36, 45, 59 | Debug.Log | 죽은 메서드 `waveSlice()`(C-D07-04)와 같은 파일의 다른 메서드 추적 로그 — 제거 권장 → 2026-08-20 4건 전량 제거 |
| [x] | C-D08-09 | Assets/Player/Script/UI/PlayerHealthUI.cs | 26 | Debug.Log | 하트 초기화 추적 로그 — 제거 권장 → 2026-08-20 제거 완료. ⚠ 실제 제거된 줄은 `Debug.LogWarning("PlayerStats Instance를 찾을 수 없습니다...")` (null 가드) — Kind 표기가 실제와 다름, 처리 완료 로그 참고 |
| [x] | C-D08-10 | Assets/Player/Script/UI/PlayerWaterUI.cs | 59 | Debug.Log | 물병 초기화 추적 로그 — 제거 권장 → 2026-08-20 제거 완료. ⚠ 실제 제거된 줄은 `Debug.LogError(...)`(Image 컴포넌트 누락 가드) — Kind 표기가 실제와 다름, 처리 완료 로그 참고 |
| [x] | C-D08-11 | Assets/Player/Script/WaterController.cs | 37, 51 | Debug.Log | 물 회복 관련 추적 로그 (CP949 파일) — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | D-D08-01 | Assets/Script/CorruptedWater.cs | 13, 22, 35, 39 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-02 죽은 코드) → 2026-08-20 4건 전량 제거 (클래스 자체 삭제는 D-D07-02 별도 미실행) |
| [x] | D-D08-02 | Assets/Script/Chase.cs | 23 | Debug.LogError | 오류 진단용 — 유지 권장 (Player 오브젝트 미발견 가드) → 유지 확인(미변경) |
| [x] | D-D08-03 | Assets/Script/InteractivePrompt.cs | 66 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | D-D08-04 | Assets/Script/ObstacleInteraction.cs | 14, 24, 33 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-11 죽은 코드) → 2026-08-20 3건 전량 제거 (클래스 자체 삭제는 D-D07-11 별도 미실행) |
| [x] | D-D08-05 | Assets/Script/MainMenuUI.cs | 29, 38 | Debug.Log/LogWarning | 29=상태 추적(제거 권장), 38(세이브 파일 없음)=오류 진단용(유지 권장) → 29 제거, 38 유지 확인 |
| [x] | D-D08-06 | Assets/Script/EnvironmentManager.cs | 56, 107 | Debug.Log | 환경 상태 전환/BGM 컷오프 변경 로그 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | D-D08-07 | Assets/Script/ProtoEnemy.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-13 죽은 코드) → 2026-08-20 제거 완료 (클래스 자체 삭제는 D-D07-13 별도 미실행) |
| [x] | D-D08-08 | Assets/Script/PureWater.cs | 13, 22, 35, 39 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-03 죽은 코드) → 2026-08-20 4건 전량 제거 (클래스 자체 삭제는 D-D07-03 별도 미실행) |
| [x] | D-D08-09 | Assets/Script/Combat/CombatSpawner.cs | 21, 51 | Debug.LogWarning | 오류 진단용(프리팹/컴포넌트 미발견) — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-10 | Assets/Script/Combat/HealPopupSpawner.cs | 22 | Debug.LogWarning | 오류 진단용(프리팹 미발견) — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-11 | Assets/Script/TakeDmg.cs | 15 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-09 죽은 코드) → 2026-08-20 제거 완료 (클래스 자체 삭제는 D-D07-09 별도 미실행) |
| [x] | D-D08-12 | Assets/map/script/PlayerRespawn.cs | 47 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | D-D08-13 | Assets/map/script/FallZone.cs | 17, 25 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | D-D08-14 | Assets/map/script/InteractableWall.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | D-D08-15 | Assets/map/script/3 stage/FloorPuzzleManager.cs | 19, 25 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | D-D08-16 | Assets/map/script/3 stage/OpengameManger.cs | 34 | Debug.Log | 개발용 상태 추적 — 제거 권장 (클래스는 참조 22건으로 활성) → 2026-08-20 제거 완료 |
| [x] | D-D08-17 | Assets/map/script/PumpManager1.cs | 11, 21 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-15 죽은 코드) → 2026-08-20 2건 전량 제거 (클래스 자체 삭제는 D-D07-15 별도 미실행) |
| [x] | D-D08-18 | Assets/map/script/RoomSwitch.cs | 26 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | D-D08-19 | Assets/map/script/3 stage/ShrineManager.cs | 16, 26 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-16 죽은 코드) → 2026-08-20 2건 전량 제거 (클래스 자체 삭제는 D-D07-16 별도 미실행) |
| [x] | D-D08-20 | Assets/map/script/3 stage/SlidingPuzzleManager.cs | 107 | Debug.Log | 개발용 상태 추적(퍼즐 클리어) — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | D-D08-21 | Assets/map/script/3 stage/SlidingPuzzleTrigger.cs | 52, 60, 65, 71, 85 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 5건 전량 제거 |
| [x] | D-D08-22 | Assets/Editor/AnimationEventCleaner.cs | 40 | Debug.Log | 에디터 도구 출력 — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-23 | Assets/Editor/BuildPhase2Assets.cs | 45, 123, 142, 147, 151 | Debug.Log/LogWarning | 에디터 도구 출력 — 유지 권장 → 파일 자체가 D-D07-17 승인으로 2026-08-20 삭제되어 해소됨 |
| [x] | D-D08-24 | Assets/Editor/BuildWaterMonsterAssets.cs | 15, 61, 96 | Debug.Log | 에디터 도구 출력 — 유지 권장 → 파일 자체가 D-D07-18 승인으로 2026-08-20 삭제되어 해소됨 |
| [x] | D-D08-25 | Assets/Editor/PlaceWaterMonsterInScene.cs | 57 | Debug.Log | 에디터 도구 출력 — 유지 권장 → 파일 자체가 D-D07-20 승인으로 2026-08-20 삭제되어 해소됨 |
| [x] | D-D08-26 | Assets/Editor/Tools/FluidMaterialCreator.cs | 29, 119, 124 | Debug.LogWarning/Log | 에디터 도구 출력 — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-27 | Assets/Editor/Tools/CombatPrefabGenerator.cs | 150 | Debug.Log | 에디터 도구 출력 — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-28 | Assets/Editor/Tools/FluidNoiseTextureGenerator.cs | 75, 117 | Debug.Log | 에디터 도구 출력 — 유지 권장 → 유지 확인(미변경) |
| [x] | D-D08-29 | Assets/Editor/Tools/WaterSpiritGenerator.cs | 42, 46, 94 | Debug.Log/LogWarning | 에디터 도구 출력 — 유지 권장 → 유지 확인(미변경) |

### D-09 — 중복 로직 (DRY 위반)

| 승인 | ID | File | Line(s) | Symbol | Reason |
|------|----|------|---------|--------|--------|
| [x] | C-D09-01 | Assets/Player/Script/UI/PlayerHealthUI.cs + Assets/Player/Script/UI/PlayerWaterUI.cs | PlayerHealthUI.cs:44,72, PlayerWaterUI.cs:43,64 | `InitHearts`/`UpdateHealthUI` vs `InitBottles`/`UpdateWaterUI` | `List<GameObject>` 슬롯 생성→`Instantiate`→`Update*UI()`에서 채움 상태 재도시 골격이 거의 동일 |
| [x] | C-D09-02 | Assets/Player/Script/Menu/GameSettingsPanel.cs + Assets/Player/Script/Menu/GraphicsSettingsPanel.cs + Assets/Player/Script/Menu/SoundSettingsPanel.cs | GameSettingsPanel.cs 전체, GraphicsSettingsPanel.cs 전체, SoundSettingsPanel.cs 전체 | 설정 패널 3종 | `PlayerPrefs` 로드→적용→저장 골격을 3개 패널이 각자 독립 구현 (`ControlsSettingsPanel`/`PauseMenu`/`PauseMenuTabController`는 PlayerPrefs 미사용이라 제외) |
| [x] | D-D09-01 | Assets/Script/CorruptedWater.cs + Assets/Script/PureWater.cs | CorruptedWater.cs:1-43, PureWater.cs:1-42 | `CorruptedWater` vs `PureWater` | 클래스명만 다르고 Trigger 처리/로그 문구까지 동일한 스크립트 2벌 — 단 둘 다 D-07 죽은 코드(D-D07-02/03)로도 기재되어 있어, 해당 항목 승인 시 이 중복도 함께 해소됨 |
| [x] | D-D09-02 | Assets/Editor/Tools/CombatPrefabGenerator.cs + Assets/Editor/BuildPhase2Assets.cs | CombatPrefabGenerator.cs:147, BuildPhase2Assets.cs:41 | 프리팹 저장 보일러플레이트 | "임시 GameObject 생성 → 컴포넌트 부착 → `PrefabUtility.SaveAsPrefabAsset` → `AssetDatabase.SaveAssets` → `DestroyImmediate`" 골격이 두 Editor 도구에 반복 |

### D-10 — 과도하게 긴/복잡한 함수 (권장)

> D-10은 권장 수준 관찰이다. 실제 수정은 사용자 승인 필수 (CONTEXT.md D-10).

| 승인 | ID | File | Line(s) | Symbol | Metric | Reason |
|------|----|------|---------|--------|--------|--------|
| [v] | C-D10-01 | Assets/Player/Script/PlayerAttack.cs | 62-140 | `OnBasicAttack` | 77줄 / 분기 8 | 콤보 카운트 판정 + 방향/스폰 위치 계산 + 3타 특수 배율 + Water Boost 버프 적용 + 코루틴 시작 혼재. 상단에 죽은 구버전 블록(C-D07-03)까지 있어 실질 가독성은 더 나쁨 |
| [v] | D-D10-01 | Assets/Editor/BuildPhase2Assets.cs | 49-152 | `PlacePhase2Objects` | 104줄 / 분기 6 | 씬 오브젝트 9종 생성 + AddComponent + SerializedObject 필드 와이어링 4종이 단일 메서드에 혼재 |
| [ ] | D-D10-02 | Assets/Editor/Tools/FluidNoiseTextureGenerator.cs | 11-85 | `GenerateNoiseTexture` | 75줄 / 분기 3 | 텍스처 생성 + 픽셀 채우기 이중 for 루프 + 저장 + import 설정 호출 혼재 |
| [ ] | D-D10-03 | Assets/Editor/BuildWaterMonsterAssets.cs | 18-63 | `BuildAnimator` | 46줄 / 분기 2 | 애니메이터 컨트롤러 생성 + 스테이트 여러 개 등록 + 트랜지션 설정 혼재 |
| [ ] | D-D10-04 | Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/HeroKnight.cs | 43-178 | `Update` | 136줄 / 분기 36 | 입력 처리 + 상태머신 전환 + 애니메이터 파라미터 갱신 혼재 [벤더 — 외부 도입 에셋, 정리 권장 아님] |

## 회귀 위험 높음 — 신중 검토 필요

> 아래 항목은 이미 Play 모드로 검증된 코드에 있다 (CONTEXT.md D-05/D-06). 수정 시 회귀 위험이 높으므로 개별적으로 더 신중하게 판단할 것.

### D-07 — 죽은 코드 (고위험)

| 승인 | ID | File | Line(s) | Symbol | Reason |
|------|----|------|---------|--------|--------|
| [x] | A-D07-01 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 8 | `ChargeRange` | 프로젝트 전체 참조 1건(선언부뿐) — STATE.md Key Decision은 "SpiritFarProjectile의 minDistance로 재활용 금지"였을 뿐, 필드 자체 삭제를 금지한 것은 아님. 실제 구현(SpiritCombatState.cs)이 거리 조건 없이 확정(D-02a)되어 필드가 완전 미사용임을 재확인 → 2026-08-20 사용자 승인으로 필드 삭제 완료 (Spirit Clone.prefab:220의 직렬화된 값은 Unity가 무시하므로 별도 정리 불필요) |
| [x] | A-D07-02 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 263 | `EnterWaterCombat()` | 프로젝트 전체 참조 1건(선언부뿐), 씬/프리팹/anim/controller 참조 0건. 주석("Used by test harnesses")과 달리 실제 호출부가 코드베이스 어디에도 없음. 실제 WaterMonsterCombatState 진입 경로는 Update()의 CombatState→WaterMonsterCombatState 바꿔치기 로직(구 191행)이며 이 메서드는 그 경로와 무관 → 2026-08-20 사용자 승인으로 메서드 삭제 완료 |
| [x] | A-D07-03 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 257 | `CoreColor` (getter) | `SetCoreColor()`로 쓰기만 되고 프로젝트 전체에서 읽는 곳이 0건 — **삭제 권장 아님**: 2026-08-20 사용자 판단으로 유지 결정, 코드 미변경 |
| [x] | A-D07-04 | Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs | 7 | `PuddleExplosionController` (클래스 전체) | 클래스명 프로젝트 전체 참조 1건(선언부뿐), 씬/프리팹/asset 참조 0건 — **삭제 권장 아님**: 완성된 코드이며 `PuddleStackManager`/`PuddlePool`과 함께 씬에 배치만 하면 그대로 동작. 2026-08-20 설명서 작성으로 처리 완료, 사용법은 `Assets/Enemy/WaterMonster/Docs/PuddleExplosionController.md` 참고 |
| [x] | A-D07-05 | Assets/Enemy/WaterMonster/Script/Phase3/WaterTeleportState.cs | 2 | `using System.Collections.Generic;` | 사용되지 않는 using → 2026-08-20 제거 완료 |
| [x] | A-D07-06 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 2 | `using System.Collections;` | 사용되지 않는 using (Generic 쪽은 별도 using으로 커버됨) → 2026-08-20 제거 완료 |
| [x] | B-D07-03 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 5-45 | `SeedProjectile` (클래스 전체) | 프로젝트 전체 코드 참조 0건, GUID로 씬/프리팹 검색해도 부착 위치 없음 → 2026-08-20 사용자 승인으로 파일 삭제 완료 (.cs + .cs.meta) |
| [x] | B-D07-04 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 15 | `Launch(Vector2 dir)` | 호출부 0건 (클래스 자체가 B-D07-03) → 클래스와 함께 삭제 완료 |
| [x] | B-D07-05 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossChaseState.cs | 5-56 | `WoodBossChaseState` (클래스 전체) | `IBossState` 구현체지만 `new WoodBossChaseState()` 호출부 0건 — 상태머신에 편입되지 못함 → 2026-08-20 사용자 승인, `WoodBoss/` 폴더 전체 삭제로 함께 해소 |
| [x] | B-D07-06 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossAttackState.cs | 19 | `CloseRange` (private const float) | 선언 후 참조 0건 → 2026-08-20 사용자 승인, `WoodBoss/` 폴더 전체 삭제로 함께 해소 |
| [x] | B-D07-07 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 6 | `WoodBossStatsSystem` (재확인 — 정정, 이후 삭제) | **정정: 당초 삭제 대상 아니었음.** 기존 `Assets/SaveSystem/Check.md:108`의 고아 판정은 문자열 `"WoodBossStatSystem"`(중간 `s` 없음)로 검색해 참조 0건이라 결론지었으나, 실제 클래스명은 `WoodBossStatsSystem`(중간에 `s` 있음)이며 `WoodBossController.cs:19,25`에서 `GetComponent<WoodBossStatsSystem>()`으로 실제 사용 중임을 확인했었다. **다만 2026-08-20 사용자가 WoodBoss 시스템 자체를 더 이상 쓰지 않기로 결정 — `WoodBossController.cs`를 포함한 `WoodBoss/` 폴더 전체가 씬/프리팹 참조 0건임을 GUID로 재확인 후 폴더째 삭제, 이 파일도 함께 삭제됨.** |
| [x] | D-D07-21 | Assets/map/script/GameManager.cs | 8 | `NextSpawnPointName` | 기존 식별 고아 코드 재확인 — 유일 writer가 고아 `Portal.cs:18`, reader 0건. Phase 11에서 `PlayerSpawner.targetSpawnPointName`이 실제 스폰 경로로 대체 채택됨 → 2026-08-20 사용자 승인, `NextSpawnPointName` 프로퍼티 제거 + `Portal.cs`(씬/프리팹 GUID 참조 0건 재확인) 파일 삭제 완료 (.cs + .cs.meta) |
| [x] | C-D07-08 | Assets/SaveSystem/Script/SaveLoadManager.cs | 118 | `IsBossDefeated(string bossId)` (public method) | 프로젝트 전체 참조 0건(선언부뿐) — `SaveOnBossDefeated`는 쓰기만 하고 읽는 호출부가 없음. **삭제 권장 아님**: 2026-08-20 사용자 판단으로 유지 결정, 코드 미변경 |
| [x] | C-D07-09 | Assets/SaveSystem/Script/SaveData.cs | 12 | `SaveVersion` (public field, 기본값 1) | 프로젝트 전체 참조 1건(선언부뿐) — 스키마 버저닝용 선제적 필드로 추정, 명시된 의도 문서는 없음. **삭제 권장 아님**: 2026-08-20 사용자 판단으로 유지 결정, 코드 미변경 |
| [x] | C-D07-10 | Assets/SaveSystem/Script/SaveData.cs | 29 | `MapGimmickState` (public field) | 의도된 확장 스텁 (Phase 11 D-03/D-03b) — 삭제 권장 아님 |
| [x] | C-D07-11 | Assets/SaveSystem/Script/SaveData.cs | 32 | `Items` (public field) | 의도된 확장 스텁 (Phase 11 D-03/D-03b) — 삭제 권장 아님 |

> **주의(B-D07-07 재확인 결과):** 기존 STATE.md/Check.md의 "`WoodBossStatSystem.cs` 는 고아 코드" 기재는 철자 오류에 의한 오판정이었다. 이 항목은 삭제 후보가 아니라 정정 기록이다.

### D-08 — TODO/FIXME 잔재 및 임시 디버그 코드 (고위험)

> `Assets/Camera/Script/CameraController.cs` 는 UTF-8이지만 비-ASCII(한글) 주석 라인이 5줄 존재하며, Phase 10/12에서 "비-ASCII 라인 수 5 유지"가 회귀 게이트였다.

| 승인 | ID | File | Line(s) | Kind | Reason |
|------|----|------|---------|------|--------|
| [x] | A-D08-01 | Assets/Enemy/WaterMonster/Script/Phase2/WeatherController.cs | 21 | Debug.LogError | 오류 진단용 — 유지 권장 → 유지 확인(미변경) |
| [x] | A-D08-02 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterMeleeSwipe.cs | 16 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | A-D08-03 | Assets/Enemy/WaterMonster/Script/States/Attacks/WaterRangedSpit.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | A-D08-04 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs | 105, 112 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | A-D08-05 | Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs | 56, 87, 92 | Debug.LogError/Log/LogWarning | 56=오류 진단(유지 권장); 87/92=상태 추적(제거 권장 검토) → 87/92 제거, 56 유지 확인 |
| [x] | A-D08-06 | Assets/Enemy/WaterMonster/Script/WaterMonsterStats.cs | 32, 36, 51 | Debug.Log/LogWarning | 32/51=상태 추적(제거 권장); 36=오류 진단(유지 권장) → 32/51 제거, 36 유지 확인 |
| [x] | A-D08-07 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs | 54, 109 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | A-D08-08 | Assets/Enemy/WaterSpirit/Script/SpiritStats.cs | 22, 28, 37, 46, 59 | Debug.Log/LogWarning | 22/28/37/59=상태 추적(제거 권장); 46=오류 진단(유지 권장) → 4건 제거, 46 유지 확인 |
| [x] | A-D08-09 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritCharge.cs | 27, 35, 60 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 3건 전량 제거 |
| [x] | A-D08-10 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritExhaustion.cs | 11 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | A-D08-11 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritFarProjectile.cs | 27, 33 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | A-D08-12 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectileAttack.cs | 15, 30, 34 | Debug.LogWarning/Log | 15/34=오류 진단(유지 권장); 30=상태 추적(제거 권장) → 30 제거, 15/34 유지 확인 |
| [x] | A-D08-13 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritRepel.cs | 12, 32, 36, 42 | Debug.Log/LogWarning | 12/32/42=상태 추적(제거 권장); 36=오류 진단(유지 권장) → 3건 제거, 36 유지 확인 |
| [x] | A-D08-14 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritStealth.cs | 36 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | A-D08-15 | Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritWakeRepel.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | A-D08-16 | Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs | 22, 38, 49, 56 | Debug.Log | 개발용 상태 추적 — 제거 권장 → 2026-08-20 4건 전량 제거 |
| [x] | A-D08-17 | Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs | 25, 29, 36, 52, 68, 72, 112, 124, 144 | Debug.Log/LogError/LogWarning | 29/52=오류 진단(유지 권장); 나머지 7건=상태 추적(제거 권장) → 7건 제거, 2건 유지 확인 |
| [x] | B-D08-08 | Assets/Enemy/NewBoss/Script/BossController.cs | 93, 100 | 주석처리 코드 | `//if (Anim != null) Anim.SetBool(...)` 2건 — 애니메이션 파라미터 미사용으로 주석 처리 → 2026-08-20 사용자 승인, 2건 전량 제거 완료 |
| [x] | B-D08-09 | Assets/Enemy/NewBoss/Script/BossStatesSystem.cs | 87, 95 | Debug.Log | 배리어/체력 피격 로그 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | B-D08-10 | Assets/Enemy/NewBoss/Script/States/ChaseStates.cs | 7, 32 | Debug.Log | Enter/Exit 로그 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | B-D08-11 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 80, 84, 114, 119, 130 | Debug.Log/LogWarning | 80=경고(유지 권장), 나머지 4건=상태 추적(제거 권장) — 전 보스 공용 헬퍼라 회귀 위험 최고 등급 → 4건 제거, 80 유지 확인 |
| [x] | B-D08-12 | Assets/Enemy/NewBoss/Script/States/IdleState.cs | 8, 23 | Debug.Log | Enter/Exit 로그 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | B-D08-13 | Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs | 19 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) → 2026-08-20 배치에서 누락됐던 것을 재실행, 바이트 단위 라인 삭제로 제거 완료 |
| [x] | B-D08-14 | Assets/Enemy/NewBoss/Script/States/Attacks/LightAttack.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) → 2026-08-20 배치에서 누락됐던 것을 재실행, 바이트 단위 라인 삭제로 제거 완료 |
| [x] | B-D08-15 | Assets/Enemy/NewBoss/Script/States/Attacks/RangedPokeAttack.cs | 18 | Debug.Log | 개발용 상태 추적 — 제거 권장 (CP949 파일) → 2026-08-20 배치에서 누락됐던 것을 재실행, 바이트 단위 라인 삭제로 제거 완료 |
| [x] | B-D08-16 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/Rootspikevisual.cs | 32 | Debug.LogWarning | Pillar/SpikeHead 미할당 경고 — 유지 권장 (CP949 파일) → 유지 확인(미변경) |
| [x] | B-D08-17 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 40, 41 | 주석처리 코드 | PlayerStats 연동 시도 주석 처리 (클래스 자체가 B-D07-03 죽은 코드) → **해소됨**: B-D07-03 승인으로 `SeedProjectile.cs` 파일 자체가 2026-08-20 삭제되어 항목 자체가 소멸 |
| [x] | B-D08-33 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | 37 | Debug.Log | "플레이어 피격!" — 클래스 자체가 죽은 코드라 실행되지 않음 (CP949 파일) → 2026-08-20 제거 완료 |
| [x] | B-D08-18 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialAttackState.cs | 49 | Debug.Log | 상태 전환 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | B-D08-19 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialDeadState.cs | 15, 32 | Debug.Log/주석처리 코드 | 15=사망 로그(제거 권장), 32=`// boss.Anim?.SetTrigger("Die");` 주석처리 → 15 제거 완료, 32 주석은 2026-08-20 사용자 승인으로 라벨 헤더와 함께 추가 제거 완료 |
| [x] | B-D08-20 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialGroggyState.cs | 27, 55, 59, 64, 70, 75 | Debug.Log | 그로기 단계별 진행 로그 6건 — 제거 권장 → 2026-08-20 6건 전량 제거 |
| [x] | B-D08-21 | Assets/Enemy/Tutorial/TutorialBoss/State/TutorialIdleState.cs | 24 | Debug.Log | 상태 전환 추적 — 제거 권장 → 2026-08-20 제거 완료 |
| [x] | B-D08-22 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs | 79, 100, 122 | Debug.Log | 전조/공격/피격 로그 3건 — 제거 권장 → 2026-08-20 3건 전량 제거 |
| [x] | B-D08-23 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | 91, 102, 122, 161, 217 | Debug.LogWarning/Log | 91=경고(유지 권장), 나머지 4건=진행 로그(제거 권장) → 4건 제거, 91 유지 확인 |
| [x] | B-D08-24 | Assets/Enemy/Tutorial/TutorialBoss/TutorialBossController.cs | 153, 275 | Debug.Log/LogWarning | 153=피격 로그(제거 권장), 275=경고(유지 권장) → 153 제거, 275 유지 확인 |
| [x] | B-D08-25 | Assets/Enemy/Tutorial/TutorialBoss/TutorialRootSpikeStrategy.cs | 24, 42, 57 | Debug.LogWarning/Log | 24/42=경고(유지 권장), 57=소환 로그(제거 권장) → 57 제거, 24/42 유지 확인 |
| [x] | B-D08-26 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | 36, 60, 72, 93, 123, 142 | Debug.LogWarning/Log | 36/60=경고(유지 권장), 나머지 4건=진행 로그(제거 권장) → 4건 제거, 36/60 유지 확인 |
| [x] | B-D08-27 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | 14 | Debug.Log | 타겟 발견 로그 — 제거 권장 (CP949 파일) → 2026-08-20 제거 완료 |
| [x] | B-D08-28 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/RootSpikeStrategy.cs | 27, 33, 59 | Debug.LogWarning/Log | 27=경고(유지 권장), 33/59=진행 로그(제거 권장) (CP949 파일) → 2건 제거, 27 유지 확인 |
| [x] | B-D08-29 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/VineSwingStrategy.cs | 28, 37, 48 | Debug.Log | 차징/타격/피격 로그 3건 — 제거 권장 → 2026-08-20 3건 전량 제거 |
| [x] | B-D08-30 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossAttackState.cs | 50, 93 | Debug.Log | 공격 선택 로그 2건 — 제거 권장 → 2026-08-20 2건 전량 제거 |
| [x] | B-D08-31 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossController.cs | 51, 64, 66 | Debug.Log/주석처리 코드 | 51/64=사망 로그(제거 권장), 66=주석처리 코드 → 51/64 제거 완료. 66 주석은 당초 미처리였으나 B-D07-07 재확인 결과에 따라 `WoodBoss/` 폴더 전체가 2026-08-20 삭제되어 파일째 해소됨 |
| [x] | B-D08-32 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | 29, 40 | Debug.Log | 체력 변화/사망 로그 2건 — 제거 권장 (CP949 파일) → 2026-08-20 2건 전량 제거 |
| [x] | C-D08-12 | Assets/Player/Script/InputHandler.cs | 80, 87, 173 | Debug.LogError | Input Action Asset 미할당/액션맵 없음 오류 — 유지 권장 → **⚠ 173행이 실제로는 2026-08-20 처리 중 삭제됨** (`"찾을 수 없음: '{actionName}'..."` LogError) — 유지 권장 위반, 80/87은 유지 확인됨. **2026-08-20 복원 완료** (정책 이탈 사례 1번 참고), 3건 전부 유지 확인 |
| [x] | C-D08-13 | Assets/Player/Script/InputHandler.cs | 124, 183, 191 | Debug.Log | ESC 키 입력 등 상태 추적 — 제거 권장 (InputHandler는 고위험 파일, 제거 시 회귀 검증 필요) → 124는 `OnPauseEvent?.Invoke()`와 한 줄에 묶여 있어 로그 호출부만 수술적으로 제거(이벤트 구독 보존), 191 제거 완료. 183은 C-D08-12의 LogError와 같은 지점에서 함께 삭제됨(위 ⚠ 참고) |
| [x] | C-D08-14 | Assets/SaveSystem/Script/SaveLoadManager.cs | 84, 93, 129, 144, 156, 162, 171, 200, 218, 223, 253 | Debug.Log/LogWarning/LogError | 저장/로드 성공·실패 진단 로그 11건 — 대부분 유지 권장, 일부 상태 확인용 → **2026-08-20 개별 검토 완료**: 84/93/223(순수 상태추적 Debug.Log 3건) 제거, 129/144/156/162/171/200/218(LogWarning/LogError 진단용 7건)과 253(`[ContextMenu("Phase11/4. Log State")]` 전용 디버그 도구 메서드 1건)은 유지 |
| [x] | C-D08-15 | Assets/Player/Script/PlayerAttack.cs | 67-82 | 주석처리 코드 | C-D07-03과 동일 항목 — PlayerAttack은 일반 티어지만 고위험 InputHandler 구독 체인과 얽혀 교차 기재 → 2026-08-20 C-D07-03과 함께 삭제 완료 |
| [x] | D-D08-30 | Assets/Script/HP.cs | 47, 77, 83, 99 | Debug.LogWarning/Log | 47=SpriteRenderer 없음(유지 권장), 나머지 3건=파괴/치유 상태 추적(제거 권장 후보) — HP.cs는 Phase 11/12 "0줄 변경" 계약 파일이므로 수정 시 특히 신중할 것 → **2026-08-20 사용자 판단으로 계약 유지, 4건 전부 미변경 확정** (Phase 11/12에서 이미 PASS 검증된 "0줄 변경" 상태를 깨지 않기로 결정) |
| [x] | D-D08-31 | Assets/map/script/Checkpoint.cs | 14, 36, 40, 45, 56, 66 | Debug.Log/LogError | 40=PlayerRespawn 스크립트 못 찾음(유지 권장), 나머지 5건=체크포인트 활성화/이탈 상태 추적(제거 권장 후보) — Phase 11 저장 트리거 삽입 파일, CP949 인코딩(D-04) → 5건 제거, 40 유지 확인 |
| [x] | D-D08-32 | Assets/map/script/PlayerSpawner.cs | 42, 49 | Debug.Log/LogWarning | 42=스폰 위치 이동 상태 추적(제거 권장 후보), 49=스폰포인트 이름 못 찾음(유지 권장) — Phase 11 로드 경로가 이 파일의 targetSpawnPointName을 재사용, CP949 인코딩(D-04) → 42 제거, 49 유지 확인 |

### D-09 — 중복 로직 (고위험)

| 승인 | ID | File | Line(s) | Symbol | Reason |
|------|----|------|---------|--------|--------|
| [ ] | A-D09-01 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs + Assets/Enemy/WaterSpirit/Script/States/SpiritCombatState.cs | WaterMonsterCombatState.cs:100-113, SpiritCombatState.cs:44-57 | 후보없음 로그 가드(`_noCandidateLogged`) + 패턴 선택 로그 | 두 CombatState 서브클래스에 거의 동일한 로깅 보일러플레이트가 중복 — 판단 로직 자체의 의도적 차이(E-03/E-04)와 별개로 이 로깅 부분만 `CombatState` 기반 클래스로 승격 가능해 보임 |
| [ ] | A-D09-02 | Assets/Enemy/WaterSpirit/Script/SpiritController.cs + Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritProjectile.cs | SpiritController.cs:99-111 (`HandleChargeImpact`), SpiritProjectile.cs:37-45 (`HandleHit`) | 피격판정 골격 | 레이어마스크 체크 → HP 컴포넌트 탐색 → `TakeDamage` 호출 골격 유사 (완전 동일 아님, 대상 컴포넌트 탐색 방식은 다름) |
| [ ] | B-D09-01 | Assets/Enemy/NewBoss/Script/States/IdleState.cs + Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | IdleState.cs:4-28(NewBoss), IdleState.cs:6-23(WoodBoss) | 동명 클래스 `IdleState` 2개 | `IBossState` 구현, `Enter/Execute/Exit` 골격이 구조적으로 동일(TargetFound 체크 후 전환)하지만 전환 대상이 다름. 네임스페이스로 분리되어 컴파일 충돌은 없으나 `new WoodBoss.IdleState()`처럼 명시적 정규화가 필요해 유지보수 시 혼동 위험 — 리네이밍 검토 대상 |
| [ ] | B-D09-02 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs + Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | TentaclePierceStrategy.cs:68-133, TentacleSwipeStrategy.cs:68-162 | `AttackRoutine` 코루틴 골격 | "경고 프리팹 생성 → WaitForSeconds → 경고 제거 → 공격 판정 → Debug.Log" 골격이 거의 동일 (공격 판정 방식만 OverlapBoxAll AoE vs Lerp+매프레임 히트체크로 다름) |
| [ ] | B-D09-03 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs + Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | TentacleSwipeStrategy.cs:68-162, FloorSweepStrategy.cs:31-125 | `AttackRoutine` 코루틴 골격 | 바닥 Raycast 감지 → 경고 → 스윕 오브젝트 Lerp 이동 → 매 프레임 히트체크 구조가 거의 1:1 동일 (TutorialBoss/WoodBoss 계열이 각각 독립 구현) |

### D-10 — 과도하게 긴/복잡한 함수 (고위험, 권장)

> 참고 — `Assets/Camera/Script/CameraController.cs` 의 `LateUpdate`(444-470줄, 실측 26줄 / 분기 3)는 D-10 수치 기준을 충족하지 않는다(40줄 미만, 분기 15 미만). 다만 Phase 9~12가 누적된 파이프라인 순서 계약이 있다: LateUpdate 실행 순서(위치추종/데드존 → 줌 Lerp → 경계 클램프 → 데드존 재앵커 → Hit Shake)가 계약이므로 분해 시 순서 보존 필수. 수치 기준이 아닌 설계 취약성 판단이므로 표 행이 아닌 각주로 기록한다.

| 승인 | ID | File | Line(s) | Symbol | Metric | Reason |
|------|----|------|---------|--------|--------|--------|
| [ ] | A-D10-01 | Assets/Enemy/WaterMonster/Script/States/WaterMonsterCombatState.cs | 68-114 | `SelectAttackStrategy` | 47줄 / 분기 8 | Enrage 존 스폰 early-return + 텔레포트 상태전환 early-return + 후보 목록 구성 위임 + 가중치 선택 + 로그 처리까지 5가지 책임 혼재 |
| [ ] | B-D10-01 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 68-138 | `Execute` | 70줄 / 분기 8 | 애니메이션 대기 판정 + 쿨다운 체크 + 그로기 전환 + 거리 판정 + 공격 전략 선택·시작 5개 책임 혼재. 전 보스(WaterMonster/WaterSpirit/TutorialBoss) 공용 기반 클래스 메서드라 회귀 위험 최고 등급 |
| [ ] | B-D10-02 | Assets/Enemy/NewBoss/Script/States/CombatState.cs | 158-200 | `SelectWeightedPattern` | 41줄 / 분기 15 | 거리 조건 필터 + 쿨다운 필터 + 직전패턴 배제/감쇠 + 가중치 누적 + 랜덤 선택. SpiritCombatState/WaterMonsterCombatState 양쪽이 공유하는 핵심 헬퍼라 분해 시 양쪽 보스 동시 회귀 가능 |
| [ ] | B-D10-03 | Assets/Enemy/Tutorial/TutorialBoss/TentacleSwipeStrategy.cs | 68-162 | `AttackRoutine` | 94줄 / 분기 10 | 바닥 Raycast 감지 + 경고 표시 + 스윕 오브젝트 생성 + Lerp 이동 + 매 프레임 히트체크 5단계가 코루틴 하나에 순차 결합 |
| [ ] | B-D10-04 | Assets/Enemy/Tutorial/TutorialBoss/TentaclePierceStrategy.cs | 68-133 | `AttackRoutine` | 65줄 / 분기 5 | 경고 표시 + OverlapBoxAll AoE 판정 + foreach 데미지 적용 + 후딜레이가 한 코루틴에 순차 결합 |
| [ ] | B-D10-05 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/FloorSweepStrategy.cs | 31-125 | `AttackRoutine` | 95줄 / 분기 6 | TentacleSwipeStrategy.AttackRoutine(B-D10-03)과 거의 동일한 구조 — D-09(B-D09-03)로도 교차 관찰됨 |
| [ ] | C-D10-02 | Assets/Player/Script/InputHandler.cs | 61-104 | `Awake` | 42줄 / 분기 4 | 싱글톤 초기화 + InputActionAsset 로드 + "Player" 액션맵 조회 + 액션 10개 바인딩 혼재. InputHandler는 Phase 10에서 "0줄 변경"이 성공 기준이었던 고위험 파일이므로 분해 시 프로젝트 전체 입력 이벤트 구독자 전원에 대한 회귀 검증 필요 |

## D-09 배제 목록 — 문서화된 의도적 비공유

> 아래는 "중복처럼 보이지만 문서화된 이유가 있어 의도적으로 공유하지 않은" 항목이다. CONTEXT.md D-09에 따라 문제로 보고하지 않는다. 재확인용으로만 나열한다.

| # | 의도된 비공유/중복 | 근거 |
|---|---------------------|------|
| E-01 | 코드 공유를 `BossController` / `BossStatsSystem` 기반 클래스 수준으로만 제한하는 원칙 자체 | PROJECT.md `## Key Architectural Decisions`, STATE.md `### Key Decisions` |
| E-02 | 물의 정령(`SpiritController`)을 `WaterMonsterController` 와 별도 독립 엔티티로 구현 | PROJECT.md `## Key Architectural Decisions` "물의 정령 독립 구현" |
| E-03 | `SpiritCombatState` 의 직전 패턴 **완전배제** vs `WaterMonsterCombatState` 의 **가중치 0.5배 감쇠** | STATE.md Phase 8 Plan 1 (D-01a~c) — SpiritCombatState.cs 0줄 변경 보장 목적 |
| E-04 | `SpiritCombatState` 의 `Enter()` 1회 후보 캐싱 vs `WaterMonsterCombatState` 의 매 호출 `BuildCandidates()` 재구성 | STATE.md Phase 8 Plan 2 (D-06c) — 전투 중 페이즈 전환 때문에 복사 불가 |
| E-05 | `CombatState.SelectWeightedPattern` 이 오버로드 대신 기본값 3번째 파라미터를 쓰는 것 | STATE.md Phase 8 Plan 1 — 기존 2-인자 호출부 무변경 보장 (D-05a 회귀 방지) |
| E-06 | 저장 트리거가 Group A(`HandleDeath()`)와 Group B(`Die()` 오버라이드) 두 방식으로 갈린 것 | STATE.md Phase 11 Plan 3 — 두 보스 아키텍처가 실제로 다름 (`HP.OnDeath` 이벤트 유무) |
| E-07 | `SpiritController.ChargeRange` 데드 필드 유지 | STATE.md Phase 7 — 재활용 시 `SpiritFarProjectile` 영구 선택 불가 발생 |
| E-08 | `CameraZoomTrigger` 가 필드 0개인 것 | ROADMAP Phase 9 D-02/D-04/D-05 — 어느 보스 구역에나 드롭 가능해야 함, 줌 값은 `CameraController` Inspector 소유 |
| E-09 | `CameraController` 의 보스존 레거시 `Vector3.Lerp` 경로와 일반 스테이지 데드존 경로 분리 | ROADMAP Phase 10 D-15 |
| E-10 | `SaveLoadManager` 가 `async`/`await` 대신 코루틴을 쓰는 것 | ROADMAP Phase 11 성공 기준 6 — 프로젝트 무-async 컨벤션 |
| E-11 | `HP.cs` 와 `BossStatsSystem` 이 별개 HP 체계인 것 | ROADMAP Phase 11 성공 기준 8 / Phase 12 성공 기준 6 — `HP.cs` 0줄 변경이 명시 계약 |
| E-12 | `Assets/ImportedAsset/**` 내부 중복 | 외부 도입 에셋(벤더) — 재임포트 시 변경 소실 |

**추가로 검토했으나 D-09에 채택하지 않은 후보 (E-01~E-12 배제 사유는 아니지만 실측상 중복이 아니거나 이미 다른 카테고리로 해소됨):**
- `Assets/Player/Script/InputHandler.cs` vs `Assets/Player/Script/PlayerInputHandler.cs` — 이름만 비슷할 뿐 실제로는 다른 역할(전자는 Input System 이벤트 버스 싱글톤 78회 참조, 후자는 공격 콜백 추상 계약 2회 참조). 진짜 중복 아님 — 리네이밍은 검토해볼 만하나 D-09 대상 아님.
- `Assets/Camera/Script/CameraBoundsTrigger.cs` vs `Assets/Camera/Script/CameraZoomTrigger.cs` 의 `OnTriggerEnter2D`/`OnTriggerExit2D` 골격 유사성 — `CameraZoomTrigger` 필드 0개가 이미 E-08로 배제된 의도적 설계라 통합 권장 아님.
- `Assets/Script/Txt/` 5종(DOUBLESPACE/SHIFT/SPACE/WASD/TxtController) — 계획 단계에서는 "중복 후보"로 예상됐으나 실측 결과 5개 전부 참조 0건으로 죽은 코드였음. D-07(D-D07-04~08)로 재분류, D-09 대상 아님.
- `Assets/Script/TakeDmg.cs`(`GiveDmg`) vs `Assets/Script/EnemyDamager.cs` — 데미지 전달 패턴은 겹치지만 `TakeDmg.cs` 자체가 이미 죽은 코드(D-D07-09)라 실질적 DRY 위반 아님.
- `Assets/Enemy/Monster_Alpha/Script/enemy.cs` vs `EnemyBrain.cs` — 이동/추적 골격 유사하나 `EnemyBrain`이 죽은 코드(B-D07-01)라 실질 중복 위험 없음.

## D-04 — CP949 인코딩 위험 파일 (전역 46개)

> 이 파일들은 CP949 인코딩이다. 실제 수정 단계에서는 표준 Read/Edit 왕복이 비-ASCII 바이트를 U+FFFD로 훼손시키므로 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜이 필요하다 (CONTEXT.md D-04, STATE.md Phase 11 Plan 3 기록).
>
> `Assets/Camera/Script/CameraController.cs` 는 CP949는 아니지만(UTF-8) 비-ASCII(한글) 주석 라인 5줄이 존재하며 Phase 10/12에서 "비-ASCII 라인 수 5 유지"가 회귀 게이트였다 — 수정 시 동일 게이트를 재적용할 것.

| # | File | Scope | 티어 |
|---|------|-------|------|
| 1 | Assets/Enemy/Boss/Script/HandCollision.cs | B | 일반 |
| 2 | Assets/Enemy/Monster_Alpha/Script/patorl.cs | B | 일반 (죽은 코드 B-D07-02) |
| 3 | Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs | B | 고위험 |
| 4 | Assets/Enemy/NewBoss/Script/States/Attacks/IAttackStrategy.cs | B | 고위험 |
| 5 | Assets/Enemy/NewBoss/Script/States/Attacks/LightAttack.cs | B | 고위험 |
| 6 | Assets/Enemy/NewBoss/Script/States/Attacks/RangedPokeAttack.cs | B | 고위험 |
| 7 | Assets/Enemy/NewBoss/Script/States/CounterState.cs | B | 고위험 |
| 8 | Assets/Enemy/NewBoss/Script/States/GroggyState.cs | B | 고위험 |
| 9 | Assets/Enemy/NewBoss/Script/States/IBossState.cs | B | 고위험 |
| 10 | Assets/Enemy/NewBoss/Script/States/IdleState.cs | B | 고위험 |
| 11 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/Rootspikevisual.cs | B | 고위험 |
| 12 | Assets/Enemy/Tutorial/TutorialBoss/Resource/Script/SeedProjectile.cs | B | 고위험 (죽은 코드 B-D07-03) |
| 13 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/IdleState.cs | B | 고위험 |
| 14 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/RootSpikeStrategy.cs | B | 고위험 |
| 15 | Assets/Enemy/Tutorial/WoodBoss/BossController/State/WoodBossChaseState.cs | B | 고위험 (죽은 코드 B-D07-05) |
| 16 | Assets/Enemy/Tutorial/WoodBoss/BossController/WoodBossStatSystem.cs | B | 고위험 (B-D07-07 재확인 — 살아있음) |
| 17 | Assets/Player/Script/PlayerAnimator.cs | C | 일반 |
| 18 | Assets/Player/Script/PlayerSpeedDisplay.cs | C | 일반 |
| 19 | Assets/Player/Script/UI/PlayerUI.cs | C | 일반 (죽은 코드 C-D07-05) |
| 20 | Assets/Player/Script/WaterController.cs | C | 일반 |
| 21 | Assets/map/script/3 stage/FloorPuzzleManager.cs | D | 일반 |
| 22 | Assets/map/script/3 stage/OpengameManger.cs | D | 일반 |
| 23 | Assets/map/script/3 stage/PipeSwitch.cs | D | 일반 (죽은 코드 D-D07-14) |
| 24 | Assets/map/script/3 stage/RotatablePipe.cs | D | 일반 |
| 25 | Assets/map/script/3 stage/ShrineManager.cs | D | 일반 (죽은 코드 D-D07-16) |
| 26 | Assets/map/script/3 stage/SlidingPuzzleManager.cs | D | 일반 |
| 27 | Assets/map/script/3 stage/SlidingPuzzleTrigger.cs | D | 일반 |
| 28 | Assets/map/script/Checkpoint.cs | D | 고위험 |
| 29 | Assets/map/script/FallZone.cs | D | 일반 |
| 30 | Assets/map/script/GameManager.cs | D | 고위험 |
| 31 | Assets/map/script/InteractableWall.cs | D | 일반 |
| 32 | Assets/map/script/MapManger.cs | D | 일반 |
| 33 | Assets/map/script/PlayerSpawner.cs | D | 고위험 |
| 34 | Assets/map/script/portal.cs | D | 일반 (죽은 코드 D-D07-01) |
| 35 | Assets/map/script/PumpManager1.cs | D | 일반 (죽은 코드 D-D07-15) |
| 36 | Assets/map/script/RoomSwitch.cs | D | 일반 |
| 37 | Assets/map/script/SignpostPortal.cs | D | 일반 |
| 38 | Assets/Script/EnvironmentManager.cs | D | 일반 |
| 39 | Assets/Script/ObstacleInteraction.cs | D | 일반 (죽은 코드 D-D07-11) |
| 40 | Assets/Script/PlatformController.cs | D | 일반 (죽은 코드 D-D07-12) |
| 41 | Assets/Script/ProtoEnemy.cs | D | 일반 (죽은 코드 D-D07-13) |
| 42 | Assets/Script/Txt/DOUBLESPACE.cs | D | 일반 (죽은 코드 D-D07-04) |
| 43 | Assets/Script/Txt/SHIFT.cs | D | 일반 (죽은 코드 D-D07-05) |
| 44 | Assets/Script/Txt/SPACE.cs | D | 일반 (죽은 코드 D-D07-06) |
| 45 | Assets/Script/Txt/TxtController.cs | D | 일반 (죽은 코드 D-D07-08) |
| 46 | Assets/Script/Txt/WASD.cs | D | 일반 (죽은 코드 D-D07-07) |

Scope A(WaterMonster/WaterSpirit)는 CP949 파일이 0개다.

## 기타 — 코드 외 정리 항목

- **[스테일 씬 엔트리]** `Assets/Scenes/InGame.unity` — 파일 자체는 존재하지 않는다(`ls Assets/Scenes/InGame.unity` → "No such file or directory"). `ProjectSettings/EditorBuildSettings.asset:9`에 `path: Assets/Scenes/InGame.unity`로 등록되어 있었다. **계획서 대비 정정:** 원래 `Assets/SaveSystem/Check.md`는 `Assets/Script/MainMenuUI.cs`의 `OnClickStart()`가 이 씬을 로드하려다 실패하는 것으로 기술했으나, 실측 결과 `MainMenuUI.cs`에는 `InGame` 문자열이 전혀 없다(`OnClickStart()`는 이미 `SceneManager.LoadScene("Tutorial Map")`을 호출하도록 바뀌어 있음, 코드 29-31행). 실제로 `InGame.unity`를 참조하던 코드는 `Assets/Editor/PlaceWaterMonsterInScene.cs:11`(Phase 1~2 1회용 셋업 도구, D-D07-20)뿐이었고, 이 파일은 이미 삭제됐다. → **2026-08-20 처리 완료**: 참조 코드/GUID 재확인 결과 프로젝트 전체 0건 확인 후 `EditorBuildSettings.asset`에서 해당 엔트리 제거.

## 스캔 커버리지 요약

| Scope | 대상 | 파일 수 | fragment |
|-------|------|---------|----------|
| A | WaterMonster / WaterSpirit | 38 | [13-FINDINGS-A-watermonster-waterspirit.md](./13-FINDINGS-A-watermonster-waterspirit.md) |
| B | NewBoss / Tutorial / Legacy Enemies | 42 | [13-FINDINGS-B-newboss-tutorial-legacy.md](./13-FINDINGS-B-newboss-tutorial-legacy.md) |
| C | Player / Camera / SaveSystem | 32 | [13-FINDINGS-C-player-camera-savesystem.md](./13-FINDINGS-C-player-camera-savesystem.md) |
| D | Core Script / Map / Editor / ImportedAsset | 56 | [13-FINDINGS-D-core-map-editor-imported.md](./13-FINDINGS-D-core-map-editor-imported.md) |
| | **합계** | **168** | |

> 파일 단위 상세 커버리지는 각 fragment의 `## 스캔 커버리지` 표를 참조한다.

## 2026-08-20 처리 완료 로그

사용자가 D-08(TODO/디버그 잔재) 전체를 카테고리 단위로 일괄 승인했다 — 정책: **오류 진단용 로그(LogError/LogWarning 성격의 null 가드·컴포넌트 미발견 등)는 제외하고, 나머지 개발용 추적 Debug.Log만 전량 제거.** 동시에 D-D07-17~20(Phase 1~2 1회용 Editor 셋업 도구 4종)의 삭제도 승인했다. git diff 대조로 실제 실행 결과를 검증한 기록이다.

**실행 요약**
- Debug.Log/LogWarning/LogError 총 149줄 자동 삭제 (72개 파일)
- `Assets/Player/Script/InputHandler.cs:124` 1줄 수술적 수정 — 로그가 `OnPauseEvent?.Invoke()`와 한 줄에 묶여 있어 전체 삭제 시 이벤트 구독까지 소실될 뻔함 (C-D08-13)
- `Assets/Editor/BuildPhase2Assets.cs` / `BuildWaterMonsterAssets.cs` / `Phase1CLI.cs` / `PlaceWaterMonsterInScene.cs` 4개 파일 + `.meta` 전체 삭제 (D-D07-17~20)
- D-07(죽은 코드)·D-09(중복 로직)의 나머지 항목은 **승인만 됐을 뿐 코드 삭제/리팩토링은 미실행** — 즉 `CorruptedWater.cs`·`PureWater.cs`·`TakeDmg.cs` 등 죽은 클래스 자체는 여전히 코드베이스에 존재하며, 그 안의 Debug.Log만 이번에 제거됐다.

**명시적으로 배치에서 제외된 파일 (개별 검토로 남김)**
- `Assets/SaveSystem/Script/SaveLoadManager.cs` (C-D08-14) — 로그 11건이 라인별로 유지/제거를 구분하기 애매해 제외
- `Assets/Script/HP.cs` (D-D08-30) — Phase 11/12 "0줄 변경" 계약 파일이라 제외 (git diff로 무변경 확인)
- `Assets/Player/Script/PlayerInputHandler.cs:11` (C-D08-05) — 보고서 태깅과 실제 코드가 어긋남(추적 로그가 아니라 널 가드 오류 로그로 확인) → **2026-08-20 재태깅 완료**: 유지 권장으로 정정, 코드 미변경

**⚠ 정책 이탈 사례 (실행이 원 권장과 다르게 된 것으로 확인됨)**
1. `Assets/Player/Script/InputHandler.cs` — C-D08-12에서 "유지 권장"으로 명시했던 `Debug.LogError($"찾을 수 없음: '{actionName}'...")`(약 173행)가 인접한 C-D08-13의 제거 대상 Debug.Log와 함께 삭제됨. 유지했어야 할 진단 로그가 사라진 사례 → **2026-08-20 복원 완료.** 복원 과정에서 이 줄(과 파일 전역의 다른 한글 주석/문자열)이 이번 배치 이전, 이미 이전 커밋(`2f90089`)에서부터 `U+FFFD`로 훼손돼 있었음을 발견 — 최초 커밋(`6555d22`)에서 CP949로 원문("범인 검거: ...")을 복구해 이 줄만 정상 텍스트로 복원했다. 파일 내 나머지 기존 훼손 주석(예: 177/180/184행)은 이번 복원 범위 밖이라 손대지 않음 — 별도 정리 필요.
2. `Assets/Player/Script/UI/PlayerHealthUI.cs:26`(C-D08-09), `Assets/Player/Script/UI/PlayerWaterUI.cs:59`(C-D08-10) — 원 보고서가 이 줄을 "초기화 추적 Debug.Log"로 태깅했으나 실제 코드는 각각 `Debug.LogWarning`(PlayerStats.Instance null 가드), `Debug.LogError`(Image 컴포넌트 누락 가드)였다. 태깅 자체가 Phase 13 작성 시점의 오류로 보이며, 프로젝트의 다른 유사 가드(예: C-D08-04 PlayerController.cs)는 전부 "유지 권장"으로 분류됐던 것과 일관되지 않게 제거됨 → **2026-08-20 복원 완료** (두 파일 모두 git diff 기준 HEAD와 완전 동치 확인). `PlayerWaterUI.cs`는 로그 문장만 사라진 게 아니라 `if (img == null)`이 원래 문 없이 `Debug.LogError`만 감싸던 구조였는데, 그 줄이 지워지며 바로 다음 줄 `bottleImages.Add(img)`가 실수로 `if` 안에 들어가 "Image 컴포넌트를 못 찾았을 때만 리스트에 추가"하는 반대 로직 버그가 생겼던 것도 함께 바로잡음.
3. `Assets/Enemy/NewBoss/Script/States/Attacks/HeavyAttack.cs`(B-D08-13), `LightAttack.cs`(B-D08-14), `RangedPokeAttack.cs`(B-D08-15) — 3개 파일 모두 승인 대상이었으나 실행 결과(git diff)에 전혀 나타나지 않음. 같은 배치에서 다른 CP949 파일(WoodBoss 계열 등)은 정상 처리됐으므로 CP949 자체가 원인은 아니고, 이 3개 파일만 누락된 것으로 보임 → **2026-08-20 재실행 완료.** 세 파일 다 CP949이며 각각 Debug.Log가 파일 끝부분 단독 라인이라, `git show HEAD:<path>` 인코딩 재확인 후 바이트 오프셋 계산으로 해당 라인만 정밀 삭제(`Read`/`Edit` 왕복 없이) — 결과 diff는 기존 `FloorHandAttack.cs`(B-D08-01) 처리 패턴과 동일(Debug.Log 라인만 제거, 앞뒤 공백 줄 보존).

**Debug.Log 이외 종류 — 2026-08-20 추가 승인으로 전량 해소**
D-08에는 "주석처리 코드" 종류 항목도 섞여 있었다(B-D08-08, B-D08-17, B-D08-19의 32행, B-D08-31의 66행, C-D08-15). 원 일괄 승인은 "추적용 **Debug.Log**"에 한정되어 이들은 남아있었으나, 이후 개별 승인을 받아 전부 해소됐다: B-D08-08(BossController.cs 2건)·B-D08-19(32행)·C-D08-15(PlayerAttack.cs 67-82, C-D07-03과 동일)는 코드 삭제 완료, B-D08-17(SeedProjectile.cs)·B-D08-31(WoodBossController.cs 66행)은 해당 파일 자체가 D-07 승인으로 삭제되어 항목이 소멸.

**추가 실행 (같은 날, 세션 재개 후) — `## 회귀 위험 높음` D-07 항목 개별 승인 시작**
- B-D07-03/04(`SeedProjectile.cs` 전체) — GUID/코드 참조 0건 재확인 후 사용자 승인, 파일 삭제(.cs + .cs.meta).
- B-D07-05/06(`WoodBossChaseState.cs`/`WoodBossAttackState.cs`)을 개별 검토하던 중, 사용자가 "WoodBoss 시스템 자체를 더 이상 쓰지 않는다"고 범위를 확장 지시 — `Assets/Enemy/Tutorial/WoodBoss/` 폴더 전체(BossController 8개 .cs + 빈 Projectile 폴더)를 GUID로 씬/프리팹 참조 0건 재확인 후 통째로 삭제. 원래 4건 배치(B-D07-03~06)를 넘어 B-D07-07(WoodBossStatSystem, 기존 "삭제 대상 아님" 정정 기록)까지 함께 삭제됨 — TutorialBoss가 실제 사용하는 `RootSpike.cs`/`RootSpikeVisual.cs`는 이미 `TutorialBoss/Resource/Script/`에 위치해 있어(내부 `namespace WoodBoss`만 잔존) 옮길 파일은 없었음.
- `## 회귀 위험 높음` D-07 나머지 항목 중 D-D07-21(2026-08-20 삭제 완료), A-D07-03/A-D07-04/C-D07-08~11(2026-08-20 개별 판단 완료 — A-D07-04/C-D07-10/C-D07-11은 기존부터 "삭제 권장 아님", A-D07-03/C-D07-08/C-D07-09는 이번에 사용자가 유지로 판단, 전부 코드 미변경)는 검토가 끝났다. 나머지 미검토 D-07/D-09/D-10 고위험 항목은 이 로그에 포함되지 않는 한 여전히 미검토다.

## Phase 13 정리 작업 마무리 (2026-08-20)

D-07(죽은 코드)·D-08(TODO/디버그 잔재)·기타(스테일 씬 엔트리)는 고위험 항목까지 포함해 전부 개별 판단·실행이 끝났다. 아래 완료 항목을 끝으로 이번 정리 라운드를 종료한다.

1. ~~"⚠ 정책 이탈 사례" 3건 확인~~ — 완료 (전부 복원/재실행됨, 위 로그 참고).
2. ~~`{Heavy,Light,RangedPoke}Attack.cs` Debug.Log 재실행~~ — 완료.
3. ~~D-07 고위험(A-D07-03/04, C-D07-08~11, D-D07-21) 판단~~ — 완료.
4. ~~D-08의 "주석처리 코드" 종류 항목(B-D08-08/17/19/31, C-D08-15) 처리~~ — 완료.
5. ~~C-D08-14(SaveLoadManager.cs 11건), D-D08-30(HP.cs 4건), C-D08-05(재태깅) 개별 검토~~ — 완료.
6. ~~스테일 씬 엔트리(`EditorBuildSettings.asset`의 InGame.unity) 제거~~ — 완료.

### 남은 백로그 (이번 라운드 범위 밖 — 의도적으로 미착수)

- **D-09 중복 로직 고위험 5건** — A-D09-01/02, B-D09-01/02/03. 로깅 보일러플레이트·피격판정 골격·`AttackRoutine` 코루틴 구조 중복. 리팩토링은 여러 보스 동시 회귀 위험이 있어 판단 보류.
- **D-10 긴 함수 고위험 7건** — A-D10-01, B-D10-01~05, C-D10-02. `CombatState.Execute`/`SelectWeightedPattern`, `InputHandler.Awake` 등 전 보스/입력 공용 코드라 분해 시 광범위 회귀 검증 필요. CONTEXT.md D-10 자체가 "권장" 등급이라 원래도 승인 필수였음.
- **D-10 일반 3건** — D-D10-02/03/04(Editor 도구 2건 + 벤더 에셋 1건). 우선순위 낮음.
- D-09/D-10을 다룰 계획이면 별도 phase 또는 quick task로 분리 검토할 것 — 이 보고서 체크박스는 그대로 재사용 가능.

### 미수행 검증 — 다음 작업 전 확인 필요

- **Play 모드 재검증 미수행**: 이번 라운드에서 코드가 실제로 바뀐 파일 중 `SaveLoadManager.cs`(저장/로드), `BossController.cs`(NewBoss 전 보스 공용 베이스, 고위험), `TutorialDeadState.cs`, `PlayerAttack.cs`는 Debug.Log/주석 삭제뿐이라 로직 변경은 없지만, 아직 Play 모드로 직접 확인하지는 않았다. 특히 `BossController.cs`는 공용 베이스 클래스라 다음 보스전 플레이 시 최소 1회 확인 권장.
- CP949 파일을 수정할 때는 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜을 쓴다 (D-04) — 향후 백로그 처리 시 유효.

## 실행 방식에 대한 메모

이 phase(13-01~05)는 원래 agy CLI 위임을 시도했으나, 파일 30개 이상 규모의 grep 집약적 스캔에서 `--print-timeout`을 40분까지 늘려도, 완전 분리된(detached) 프로세스로도 반복적으로 완주하지 못했다(소규모 작업은 3분 내 정상 처리됨을 확인해, 문제가 응답성이 아니라 세션 총 소요 시간임을 특정). 게다가 이 과정에서 agy가 한 차례 실제로 존재하지 않는 심볼/줄번호를 지어낸 fragment를 커밋한 사실이 발견되어(이후 실제 소스 대조로 전량 재작성됨), 이 phase 전체를 사용자 승인 하에 Claude Code(fork 5개, Wave 1 4개 병렬 + Wave 2 1개)가 직접 Read/Grep/Write로 수행하는 방식으로 전환했다. 이 전환 과정에서 병렬 fork들이 같은 git 워킹 디렉토리를 공유해 커밋 경계가 일부 뒤섞였으나(내용 손실 없음, 커밋 메시지/scope 귀속만 일부 부정확 — 상세는 `.planning/STATE.md` Key Decisions 참고), 각 fragment의 최종 내용은 이 보고서 작성 시점에 전량 재확인했다.
