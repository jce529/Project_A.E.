# Phase 13 Audit — Findings D: Core Script / Map / Editor / ImportedAsset

**Scope:** `Assets/Script/**`, `Assets/map/**`, `Assets/Editor/**`, `Assets/ImportedAsset/**`
**Files scanned:** 56
**Risk tier:** 혼합 — 공유 파이프라인 5종 = 고위험, ImportedAsset 5종 = 벤더, 나머지 46종 = 일반
**Generated:** 2026-08-19

## D-07 — 죽은 코드

### D-07 일반 항목

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| D-D07-01 | Assets/map/script/portal.cs | 4 | `Portal` (class) | 기존 식별 고아 코드 재확인 (Assets/SaveSystem/Check.md:108) — 프로젝트 전체 코드 참조 0건(선언부 제외), 스크립트 GUID(`7310f58f8e5b15b4caf790a8c649492b`) 기준 씬/프리팹 부착 0건. `grep -rl Portal Assets --include=*.unity`가 히트를 반환하지만 전부 `SignpostPortal`(별개의 살아있는 클래스) 매치이며 실제 `Portal` 컴포넌트 부착은 아님(GUID 검증으로 확인) |
| D-D07-02 | Assets/Script/CorruptedWater.cs | 3 | `CorruptedWater` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `PureWater.cs`와 로직·로그 문구까지 거의 동일 (D-09 참고) |
| D-D07-03 | Assets/Script/PureWater.cs | 3 | `PureWater` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `CorruptedWater.cs`와 로직·로그 문구까지 거의 동일 (D-09 참고) |
| D-D07-04 | Assets/Script/Txt/DOUBLESPACE.cs | 4 | `DOUBLESPACE` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `Assets/Script/Txt/` 5개 파일 전부 동일 패턴(D-D07-04~08) — 원래 D-09(중복) 후보로 지목됐으나 실측 결과 전부 죽은 코드 |
| D-D07-05 | Assets/Script/Txt/SHIFT.cs | 4 | `SHITF` (class, 오탈자 그대로) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-06 | Assets/Script/Txt/SPACE.cs | 4 | `SPACE` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-07 | Assets/Script/Txt/WASD.cs | 4 | `WASD` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-08 | Assets/Script/Txt/TxtController.cs | 4 | `TutorialTrigger` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 파일명(TxtController)과 실제 클래스명(TutorialTrigger)이 다름 |
| D-D07-09 | Assets/Script/TakeDmg.cs | 3 | `GiveDmg` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `EnemyDamager.cs`와 `GetComponent<HP>()+TakeDamage()` 패턴이 겹치지만 이 파일 자체가 죽어있어 실질 DRY 위반은 아님 |
| D-D07-10 | Assets/Script/InteractivePrompt.cs | 3 | `InteractionPrompt` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 파일명과 클래스명 불일치 |
| D-D07-11 | Assets/Script/ObstacleInteraction.cs | 3 | `ObstacleInteraction` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-12 | Assets/Script/PlatformController.cs | 4 | `PlatformController` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-13 | Assets/Script/ProtoEnemy.cs | 4 | `ProtoEnemy` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. 이름 그대로 초기 프로토타입 계열 |
| D-D07-14 | Assets/map/script/3 stage/PipeSwitch.cs | 3 | `PipeSwitch` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `3 stage pump.unity` 등 관련 씬은 존재하나 이 컴포넌트는 어디에도 부착돼 있지 않음 |
| D-D07-15 | Assets/map/script/PumpManager1.cs | 3 | `PumpManager` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건 |
| D-D07-16 | Assets/map/script/3 stage/ShrineManager.cs | 3 | `ShrineManager` (class) | 참조 0건, 씬/프리팹 GUID 부착 0건. `3 stage shrine.unity` 씬은 존재하나 이 컴포넌트는 부착돼 있지 않음 |
| D-D07-17 | Assets/Editor/BuildPhase2Assets.cs | 7 | `BuildPhase2Assets` (class) | `[MenuItem]` 진입점 2개 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| D-D07-18 | Assets/Editor/BuildWaterMonsterAssets.cs | 7 | `BuildWaterMonsterAssets` (class) | `[MenuItem]` 진입점 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| D-D07-19 | Assets/Editor/Phase1CLI.cs | 4 | `Phase1CLI.ExecuteAll` | `[MenuItem]`/`[InitializeOnLoad]` 없음, 코드 참조 0건 — Unity `-executeMethod Phase1CLI.ExecuteAll` CLI 배치 호출용으로 추정됨. Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요 |
| D-D07-20 | Assets/Editor/PlaceWaterMonsterInScene.cs | 6 | `PlaceWaterMonsterInScene` (class) | `[MenuItem]` 진입점 보유 — 죽은 코드는 아니지만 Phase 1~2 1회용 셋업 도구 — 재실행 필요 여부 사용자 확인 필요. 존재하지 않는 `Assets/Scenes/InGame.unity` 를 대상으로 함 (## 기타 참고) |

### D-07 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Symbol | Reason |
|----|------|---------|--------|--------|
| D-D07-21 | Assets/map/script/GameManager.cs | 8 | `NextSpawnPointName` | 기존 식별 고아 코드 재확인 — 유일 writer 가 고아 Portal.cs:18, reader 0건. `grep -rn NextSpawnPointName Assets`은 선언(GameManager.cs:8)과 쓰기(portal.cs:18) 2건뿐이며 읽는 코드는 프로젝트 전체에 없음. 게다가 유일한 writer인 `Portal` 클래스 자체가 D-D07-01에서 확인된 죽은 코드이므로 이 프로퍼티는 완전히 고아 상태. Phase 11에서 `PlayerSpawner.targetSpawnPointName`이 실제 스폰 경로로 대체 채택됨 |

**참고 — unused using 점검 범위:** 이 범위 56개 파일 전체를 대상으로 주석 처리된 코드 블록(`^\s*//.*[;{}]`)을 전수 스캔했고 0건이었다. `using` 문 미사용 여부는 시간 제약상 고위험 5개 파일(HP.cs/DamageInfo.cs/Checkpoint.cs/PlayerSpawner.cs/GameManager.cs)만 표본 점검했으며 전부 사용 중으로 확인됨 — 나머지 51개 파일은 미점검.

## D-08 — TODO/FIXME 잔재 및 임시 디버그 코드

### D-08 일반 항목

> `Assets/ImportedAsset/**` 는 Debug.Log 0건.

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| D-D08-01 | Assets/Script/CorruptedWater.cs | 13, 22, 35, 39 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-02 죽은 코드) |
| D-D08-02 | Assets/Script/Chase.cs | 23 | Debug.LogError | 오류 진단용 — 유지 권장 (Player 오브젝트 미발견 가드) |
| D-D08-03 | Assets/Script/InteractivePrompt.cs | 66 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-04 | Assets/Script/ObstacleInteraction.cs | 14, 24, 33 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-11 죽은 코드) |
| D-D08-05 | Assets/Script/MainMenuUI.cs | 29, 38 | Debug.Log / Debug.LogWarning | 29줄 Log = 개발용 상태 추적(제거 권장), 38줄 LogWarning(세이브 파일 없음) = 오류 진단용(유지 권장) |
| D-D08-06 | Assets/Script/EnvironmentManager.cs | 56, 107 | Debug.Log | 개발용 상태 추적(환경 상태 전환/BGM 컷오프 변경 로그) — 제거 권장 |
| D-D08-07 | Assets/Script/ProtoEnemy.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-13 죽은 코드) |
| D-D08-08 | Assets/Script/PureWater.cs | 13, 22, 35, 39 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-03 죽은 코드) |
| D-D08-09 | Assets/Script/Combat/CombatSpawner.cs | 21, 51 | Debug.LogWarning | 오류 진단용(프리팹/컴포넌트 미발견) — 유지 권장 |
| D-D08-10 | Assets/Script/Combat/HealPopupSpawner.cs | 22 | Debug.LogWarning | 오류 진단용(프리팹 미발견) — 유지 권장 |
| D-D08-11 | Assets/Script/TakeDmg.cs | 15 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-09 죽은 코드) |
| D-D08-12 | Assets/map/script/PlayerRespawn.cs | 47 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-13 | Assets/map/script/FallZone.cs | 17, 25 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-14 | Assets/map/script/InteractableWall.cs | 14 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-15 | Assets/map/script/3 stage/FloorPuzzleManager.cs | 19, 25 | Debug.Log | 개발용 상태 추적 — 제거 권장 (컴포넌트 자체는 씬 부착 미확인이나 클래스는 참조됨) |
| D-D08-16 | Assets/map/script/3 stage/OpengameManger.cs | 34 | Debug.Log | 개발용 상태 추적 — 제거 권장 (클래스 `OpengameManager`는 참조 22건으로 활성) |
| D-D08-17 | Assets/map/script/PumpManager1.cs | 11, 21 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-15 죽은 코드) |
| D-D08-18 | Assets/map/script/RoomSwitch.cs | 26 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-19 | Assets/map/script/3 stage/ShrineManager.cs | 16, 26 | Debug.Log | 개발용 상태 추적 — 제거 권장 (파일 자체가 D-D07-16 죽은 코드) |
| D-D08-20 | Assets/map/script/3 stage/SlidingPuzzleManager.cs | 107 | Debug.Log | 개발용 상태 추적(퍼즐 클리어) — 제거 권장 |
| D-D08-21 | Assets/map/script/3 stage/SlidingPuzzleTrigger.cs | 52, 60, 65, 71, 85 | Debug.Log | 개발용 상태 추적 — 제거 권장 |
| D-D08-22 | Assets/Editor/AnimationEventCleaner.cs | 40 | Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-23 | Assets/Editor/BuildPhase2Assets.cs | 45, 123, 142, 147, 151 | Debug.Log / Debug.LogWarning | 에디터 도구 출력 — 유지 권장 |
| D-D08-24 | Assets/Editor/BuildWaterMonsterAssets.cs | 15, 61, 96 | Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-25 | Assets/Editor/PlaceWaterMonsterInScene.cs | 57 | Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-26 | Assets/Editor/Tools/FluidMaterialCreator.cs | 29, 119, 124 | Debug.LogWarning / Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-27 | Assets/Editor/Tools/CombatPrefabGenerator.cs | 150 | Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-28 | Assets/Editor/Tools/FluidNoiseTextureGenerator.cs | 75, 117 | Debug.Log | 에디터 도구 출력 — 유지 권장 |
| D-D08-29 | Assets/Editor/Tools/WaterSpiritGenerator.cs | 42, 46, 94 | Debug.Log / Debug.LogWarning | 에디터 도구 출력 — 유지 권장 |

> TODO/FIXME/HACK 주석: 이 범위에서 0건 (프로젝트 전체도 0건).

### D-08 회귀 위험 높음 — 신중 검토 필요

| ID | File | Line(s) | Kind | Reason |
|----|------|---------|------|--------|
| D-D08-30 | Assets/Script/HP.cs | 47, 77, 83, 99 | Debug.LogWarning / Debug.Log | 47줄 LogWarning(SpriteRenderer 없음) = 오류 진단용(유지 권장), 나머지 3건은 파괴/치유 상태 추적(제거 권장 후보) — HP.cs는 Phase 11/12 "0줄 변경" 계약 파일이므로 수정 시 특히 신중할 것 |
| D-D08-31 | Assets/map/script/Checkpoint.cs | 14, 36, 40, 45, 56, 66 | Debug.Log / Debug.LogError | 40줄 LogError(PlayerRespawn 스크립트 못 찾음) = 오류 진단용(유지 권장), 나머지 5건은 체크포인트 활성화/이탈 상태 추적(제거 권장 후보) — Phase 11 저장 트리거 삽입 파일, CP949 인코딩(D-04) |
| D-D08-32 | Assets/map/script/PlayerSpawner.cs | 42, 49 | Debug.Log / Debug.LogWarning | 42줄 Log(스폰 위치 이동) = 상태 추적(제거 권장 후보), 49줄 LogWarning(스폰포인트 이름 못 찾음) = 오류 진단용(유지 권장) — Phase 11 로드 경로가 이 파일의 targetSpawnPointName을 재사용, CP949 인코딩(D-04) |

## D-10 — 과도하게 긴/복잡한 함수 (권장)

> D-10은 권장 수준 관찰이다. 실제 수정은 사용자 승인 필수 (CONTEXT.md D-10).

### D-10 일반 항목

| ID | File | Line(s) | Symbol | Metric | Reason |
|----|------|---------|--------|--------|--------|
| D-D10-01 | Assets/Editor/BuildPhase2Assets.cs | 49-152 | `PlacePhase2Objects` | 104줄 / 분기 6 | 씬 오브젝트 9종 생성 + 컴포넌트 AddComponent + SerializedObject 필드 와이어링 4종이 단일 메서드에 전부 혼재 — 오브젝트 생성부/와이어링부로 분해 가능 |
| D-D10-02 | Assets/Editor/Tools/FluidNoiseTextureGenerator.cs | 11-85 | `GenerateNoiseTexture` | 75줄 / 분기 3 | 텍스처 생성 + 픽셀 채우기 이중 for 루프 + 저장 + import 설정 호출이 한 메서드에 혼재 |
| D-D10-03 | Assets/Editor/BuildWaterMonsterAssets.cs | 18-63 | `BuildAnimator` | 46줄 / 분기 2 | 애니메이터 컨트롤러 생성 + 스테이트 여러 개 등록 + 트랜지션 설정이 한 메서드에 혼재 |
| D-D10-04 | Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/HeroKnight.cs | 43-178 | `Update` | 136줄 / 분기 36 | 입력 처리 + 상태머신 전환 + 애니메이터 파라미터 갱신이 전부 한 Update 안에 혼재 [벤더 — 외부 도입 에셋, 정리 권장 아님] |

### D-10 회귀 위험 높음 — 신중 검토 필요

(없음) — 고위험 5개 파일(HP.cs/DamageInfo.cs/Checkpoint.cs/PlayerSpawner.cs/GameManager.cs)을 확인했으나 40줄/분기15 기준을 넘는 메서드가 없음.

## D-04 — CP949 인코딩 위험 파일

> 이 파일들은 CP949 인코딩이다. 실제 수정 단계에서는 표준 Read/Edit 왕복이 비-ASCII 바이트를 U+FFFD로 훼손시키므로 `git show HEAD:<path>` + 순수 바이트 스크립트 프로토콜이 필요하다 (CONTEXT.md D-04, STATE.md Phase 11 Plan 3 기록).

| # | File | 비고 |
|---|------|------|
| 1 | Assets/map/script/3 stage/FloorPuzzleManager.cs | 일반 |
| 2 | Assets/map/script/3 stage/OpengameManger.cs | 일반 |
| 3 | Assets/map/script/3 stage/PipeSwitch.cs | 일반 |
| 4 | Assets/map/script/3 stage/RotatablePipe.cs | 일반 |
| 5 | Assets/map/script/3 stage/ShrineManager.cs | 일반 |
| 6 | Assets/map/script/3 stage/SlidingPuzzleManager.cs | 일반 |
| 7 | Assets/map/script/3 stage/SlidingPuzzleTrigger.cs | 일반 |
| 8 | Assets/map/script/Checkpoint.cs | 고위험 |
| 9 | Assets/map/script/FallZone.cs | 일반 |
| 10 | Assets/map/script/GameManager.cs | 고위험 |
| 11 | Assets/map/script/InteractableWall.cs | 일반 |
| 12 | Assets/map/script/MapManger.cs | 일반 |
| 13 | Assets/map/script/PlayerSpawner.cs | 고위험 |
| 14 | Assets/map/script/portal.cs | 일반 |
| 15 | Assets/map/script/PumpManager1.cs | 일반 |
| 16 | Assets/map/script/RoomSwitch.cs | 일반 |
| 17 | Assets/map/script/SignpostPortal.cs | 일반 |
| 18 | Assets/Script/EnvironmentManager.cs | 일반 |
| 19 | Assets/Script/ObstacleInteraction.cs | 일반 |
| 20 | Assets/Script/PlatformController.cs | 일반 |
| 21 | Assets/Script/ProtoEnemy.cs | 일반 |
| 22 | Assets/Script/Txt/DOUBLESPACE.cs | 일반 |
| 23 | Assets/Script/Txt/SHIFT.cs | 일반 |
| 24 | Assets/Script/Txt/SPACE.cs | 일반 |
| 25 | Assets/Script/Txt/TxtController.cs | 일반 |
| 26 | Assets/Script/Txt/WASD.cs | 일반 |

**참고 (계획서 대비 실측 차이):** 13-04-PLAN.md의 `<encoding_hazard>`는 "반드시 포함되는 것"에 `Assets/ImportedAsset/Hero Knight - Pixel Art/` 하위 4개를 예시로 들었으나, `iconv -f UTF-8 -t UTF-8` 실측 결과 ImportedAsset 트리는 CP949 0개이고 대신 `Assets/Script/` 루트 4개(EnvironmentManager/ObstacleInteraction/PlatformController/ProtoEnemy)가 CP949였다. 총 26개라는 숫자는 계획서와 일치하며, 구성 내역만 실측값으로 정정함 — 이 파일이 신뢰할 근거는 실제 `iconv` 명령 실행 결과다.

## D-09 후보 관찰 (raw — Plan 05에서 교차 검증)

- `Assets/Script/CorruptedWater.cs`(전체 43줄, D-D07-02) 와 `Assets/Script/PureWater.cs`(전체 42줄, D-D07-03) — 클래스명만 다르고 Trigger 처리/로그 문구까지 사실상 동일한 죽은 스크립트 2벌. `Assets/Script/CorruptedWater.cs:13`과 `Assets/Script/PureWater.cs:13`의 Debug.Log 문자열이 동일.
- `Assets/Script/Txt/` 5종(`Assets/Script/Txt/DOUBLESPACE.cs:4`, `Assets/Script/Txt/SHIFT.cs:4`, `Assets/Script/Txt/SPACE.cs:4`, `Assets/Script/Txt/WASD.cs:4`, `Assets/Script/Txt/TxtController.cs:4`) — PLAN.md는 이 5종을 "튜토리얼 프롬프트 표시/숨김 로직 중복(D-09)" 후보로 지목했으나, 실측 결과 5개 전부 프로젝트 참조 0건·씬/프리팹 부착 0건(D-D07-04~08)이었다. 즉 "중복"이 아니라 "전부 죽은 코드"가 실제 소견이다.
- Editor 프리팹 저장 보일러플레이트: `Assets/Editor/Tools/CombatPrefabGenerator.cs:147`(`PrefabUtility.SaveAsPrefabAsset` + `AssetDatabase.SaveAssets`), `Assets/Editor/BuildPhase2Assets.cs:41`(`PrefabUtility.SaveAsPrefabAsset` 후 `AssetDatabase.SaveAssets`) — 두 파일 모두 "임시 GameObject 생성 → 컴포넌트 부착 → 프리팹 저장 → DestroyImmediate" 골격이 반복됨.
- `Assets/Script/TakeDmg.cs:8`(`GiveDmg.DealtoTarget` — `GetComponent<HP>()` 후 `TakeDamage`)와 `Assets/Script/EnemyDamager.cs:36`(동일 패턴) — 로직은 겹치지만 `GiveDmg`가 이미 죽은 코드(D-D07-09)이므로 실질적 DRY 위반은 아님. 참고용으로만 기재.

## 기타 — 코드 외 정리 항목

- **[스테일 씬 엔트리]** `Assets/Scenes/InGame.unity` — 파일 자체는 존재하지 않는다(`ls Assets/Scenes/InGame.unity` → "No such file or directory"). 그러나 `ProjectSettings/EditorBuildSettings.asset:9`에 `path: Assets/Scenes/InGame.unity` 로 여전히 등록되어 있다. **계획서 대비 정정:** 13-04-PLAN.md와 `Assets/SaveSystem/Check.md`는 이 스테일 엔트리를 `Assets/Script/MainMenuUI.cs`의 `OnClickStart()`가 로드하려다 실패하는 것으로 기술했으나, 실측 결과 `MainMenuUI.cs`에는 `InGame` 문자열이 전혀 없다(`OnClickStart()`는 현재 `SceneManager.LoadScene("Tutorial Map")`을 호출함, 코드 29-31행) — 즉 이 부분 서술은 이미 코드가 바뀌어 outdated 상태다. 실제로 `InGame.unity`를 참조하는 코드는 `Assets/Editor/PlaceWaterMonsterInScene.cs:11`(`var scenePath = "Assets/Scenes/InGame.unity";`, MenuItem 기반 Phase 1~2 1회용 셋업 도구, D-D07-20)뿐이다. 정리 방향: Build Settings에서 스테일 엔트리 제거 또는 `PlaceWaterMonsterInScene.cs`가 여전히 필요한지 사용자 확인 필요.

## 스캔 커버리지

| # | File | Lines | Risk | Scanned |
|---|------|-------|------|---------|
| 1 | Assets/Editor/AnimationEventCleaner.cs | 47 | 일반 | yes |
| 2 | Assets/Editor/BuildPhase2Assets.cs | 184 | 일반 | yes |
| 3 | Assets/Editor/BuildWaterMonsterAssets.cs | 99 | 일반 | yes |
| 4 | Assets/Editor/Phase1CLI.cs | 18 | 일반 | yes |
| 5 | Assets/Editor/PlaceWaterMonsterInScene.cs | 60 | 일반 | yes |
| 6 | Assets/Editor/Tools/CombatPrefabGenerator.cs | 154 | 일반 | yes |
| 7 | Assets/Editor/Tools/FluidMaterialCreator.cs | 130 | 일반 | yes |
| 8 | Assets/Editor/Tools/FluidNoiseTextureGenerator.cs | 120 | 일반 | yes |
| 9 | Assets/Editor/Tools/WaterSpiritGenerator.cs | 96 | 일반 | yes |
| 10 | Assets/ImportedAsset/HealthHeartSystem/Scripts/HealthBarHUDTester.cs | 23 | 벤더 | yes |
| 11 | Assets/ImportedAsset/Hero Knight - Pixel Art/ColorSwap/ColorSwap_HeroKnight.cs | 130 | 벤더 | yes |
| 12 | Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/DestroyEvent_HeroKnight.cs | 13 | 벤더 | yes |
| 13 | Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/HeroKnight.cs | 195 | 벤더 | yes |
| 14 | Assets/ImportedAsset/Hero Knight - Pixel Art/Demo/Sensor_HeroKnight.cs | 41 | 벤더 | yes |
| 15 | Assets/map/script/3 stage/FloorPuzzleManager.cs | 27 | 일반 | yes |
| 16 | Assets/map/script/3 stage/OpengameManger.cs | 36 | 일반 | yes |
| 17 | Assets/map/script/3 stage/PipeSwitch.cs | 19 | 일반 | yes |
| 18 | Assets/map/script/3 stage/RotatablePipe.cs | 19 | 일반 | yes |
| 19 | Assets/map/script/3 stage/ShrineManager.cs | 41 | 일반 | yes |
| 20 | Assets/map/script/3 stage/SlidingPuzzleManager.cs | 114 | 일반 | yes |
| 21 | Assets/map/script/3 stage/SlidingPuzzleTrigger.cs | 95 | 일반 | yes |
| 22 | Assets/map/script/Checkpoint.cs | 68 | 고위험 | yes |
| 23 | Assets/map/script/FallZone.cs | 28 | 일반 | yes |
| 24 | Assets/map/script/GameManager.cs | 22 | 고위험 | yes |
| 25 | Assets/map/script/InteractableWall.cs | 16 | 일반 | yes |
| 26 | Assets/map/script/MapManger.cs | 25 | 일반 | yes |
| 27 | Assets/map/script/PlayerRespawn.cs | 48 | 일반 | yes |
| 28 | Assets/map/script/PlayerSpawner.cs | 52 | 고위험 | yes |
| 29 | Assets/map/script/portal.cs | 33 | 일반 | yes |
| 30 | Assets/map/script/PumpManager1.cs | 26 | 일반 | yes |
| 31 | Assets/map/script/RoomSwitch.cs | 38 | 일반 | yes |
| 32 | Assets/map/script/SignpostPortal.cs | 30 | 일반 | yes |
| 33 | Assets/Script/AudioManager.cs | 65 | 일반 | yes |
| 34 | Assets/Script/Chase.cs | 54 | 일반 | yes |
| 35 | Assets/Script/Combat/CombatSpawner.cs | 56 | 일반 | yes |
| 36 | Assets/Script/Combat/DamageInfo.cs | 14 | 고위험 | yes |
| 37 | Assets/Script/Combat/HealPopup.cs | 39 | 일반 | yes |
| 38 | Assets/Script/Combat/HealPopupSpawner.cs | 31 | 일반 | yes |
| 39 | Assets/Script/CorruptedWater.cs | 43 | 일반 | yes |
| 40 | Assets/Script/Damager.cs | 45 | 일반 | yes |
| 41 | Assets/Script/EnemyDamager.cs | 73 | 일반 | yes |
| 42 | Assets/Script/EnvironmentManager.cs | 108 | 일반 | yes |
| 43 | Assets/Script/HP.cs | 107 | 고위험 | yes |
| 44 | Assets/Script/InteractivePrompt.cs | 68 | 일반 | yes |
| 45 | Assets/Script/MainMenuUI.cs | 44 | 일반 | yes |
| 46 | Assets/Script/ObstacleInteraction.cs | 36 | 일반 | yes |
| 47 | Assets/Script/PlatformController.cs | 64 | 일반 | yes |
| 48 | Assets/Script/ProtoEnemy.cs | 31 | 일반 | yes |
| 49 | Assets/Script/PureWater.cs | 42 | 일반 | yes |
| 50 | Assets/Script/TakeDmg.cs | 18 | 일반 | yes |
| 51 | Assets/Script/Txt/DOUBLESPACE.cs | 31 | 일반 | yes |
| 52 | Assets/Script/Txt/SHIFT.cs | 30 | 일반 | yes |
| 53 | Assets/Script/Txt/SPACE.cs | 30 | 일반 | yes |
| 54 | Assets/Script/Txt/TxtController.cs | 29 | 일반 | yes |
| 55 | Assets/Script/Txt/WASD.cs | 37 | 일반 | yes |
| 56 | Assets/Script/WaveEffect.cs | 30 | 일반 | yes |

## 요약

| 카테고리 | 항목 수 | 고위험 항목 수 |
|----------|---------|----------------|
| D-07 | 21 | 1 |
| D-08 | 32 | 3 |
| D-10 | 4 | 0 |
