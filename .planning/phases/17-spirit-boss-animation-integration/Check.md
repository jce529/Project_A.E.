# Phase 17 Check - 정령 보스 애니메이션 연동

작성: 2026-10-01 (Plan 17-04). 기준선(baseline) 커밋: `3b0be41` (Phase 17 착수 직전).

## 정적 회귀 검사

| # | 항목 | 명령/방법 | 기대 | 실제 | 결과 |
|---|------|-----------|------|------|------|
| S1 | 빌드 | `dotnet build Assembly-CSharp.csproj --nologo -v quiet` (약 2분 17초) | 오류 0 | 오류 2건, 둘 다 `Library/PackageCache/com.unity.ai.assistant@.../PreviewElementFactory.cs(61,40)`, `SelectGeneratedAssetsFunctionCallElement.cs(177,39)` 의 CS0118 'Image' (기존부터 알려진 패키지 프로젝트 오류, Assembly-CSharp 소스 아님). Assets/ 소스 오류 0건 | PASS (기존 오류 2건 제외) |
| S2 | Editor 실컴파일 | `unity command recompile` 후 `console_status` | compilationFailed=false | `compilationFailed:false`, 컴파일 오류 0, `SpiritController.Stage2AnimController` 심볼이 eval 에서 해석됨 | PASS |
| S3 | 모든 Spirit 공격 전략 `AnimationName => ""` 유지 | `grep -rn AnimationName Assets/Enemy/WaterSpirit/Script` | 7개 전략 전부 `""` | SpiritCharge, SpiritExhaustion, SpiritFarProjectile, SpiritProjectileAttack, SpiritRepel, SpiritStealth, SpiritWakeRepel 7/7 `""` | PASS |
| S4 | 변경 파일 범위 | `git diff --stat 3b0be41 HEAD -- Assets` | 17-01/02/03 `files_modified` 와 일치 | SpiritController.cs, SpiritStats.cs, SpiritCharge/FarProjectile/Stealth/WakeRepel.cs, Stage2CombatState.cs, WaterSpirit.prefab(+meta), Spirit Clone.prefab - 10개 파일, 예상 목록과 정확히 일치 | PASS |
| S5 | `SpiritRepel.cs` / `SpiritProjectileAttack.cs` 무변경 | 위 diff 에 미포함 | 미포함 | 미포함 | PASS |
| S6 | `ChaseStates/IdleState/GroggyState`, 투사체/이펙트, 타 보스 무변경 | `git diff 3b0be41 HEAD --name-only \| grep -v WaterSpirit` | `.planning` 및 Spirit Clone.prefab 외 없음 | `.planning/*` 와 `Assets/Resources/Spirit Clone.prefab` 만 출력 (WaterMonster/NewBoss/Tutorial 변경 없음) | PASS |
| S7 | 프리팹 컨트롤러 참조 | `grep m_Controller` | WaterSpirit.prefab -> WaterSpirit.controller, Spirit Clone.prefab -> Stage2 override | WaterSpirit.prefab: guid `a0095bf9...` = WaterSpirit.controller; Spirit Clone.prefab: guid `93af4d61...` = WaterSpirit_Stage2.overrideController | PASS |
| S8 | Stage2 컨트롤러 교체가 Clone 트리거보다 먼저 | `OnStage2Trigger()`: 컨트롤러 대입 후 `ChangeState(new Stage2CombatState())` (Enter 에서 `PlayAnim("Clone")`) | 교체 -> 진입 순서 | 코드상 순서 일치, 런타임에서도 P8 로 확인 | PASS |
| S9 | 가드된 호출 | `PlayAnim` 은 `CanAnimate`(Anim, runtimeAnimatorController null 검사) 로 보호 | null 안전 | 일치 | PASS |
| S10 | 워크트리 기존 변경 보존 | `git status --short` | 기존 변경(2 stage.unity 등) 유지, 본 플랜 Assets 변경 없음 | 본 플랜은 Check.md/evidence/SUMMARY 만 수정. 실행 전후 `git status` 동일 | PASS |

참고(범위 밖 발견): `Assets/Enemy/WaterSpirit/Animations/`(컨트롤러/클립/오버라이드 전부)와 `Resource/` 는 여전히 **git 미추적(untracked)** 이다. 커밋된 `WaterSpirit.prefab`/`Spirit Clone.prefab` 이 이 에셋들을 GUID 로 참조하므로, 저장소만 체크아웃하면 컨트롤러가 비어 보인다. 이 계획의 범위가 아니어서 건드리지 않았다.

## Play 모드 체크리스트

방법: Unity CLI 로 연결된 Editor(6000.3.10f1) 의 `2 stage` 씬을 Play 모드로 실행. `EditorApplication.update` 훅(eval)으로 Animator 상태/클립/파라미터/트리거 변화를 `Time.time` 과 함께 파일에 기록하고, 로그 원본 일부는 `evidence/` 에 보관.
- 씬에 지면이 없어(보스가 허공에 있음) **Play 중에만 존재하는 임시 BoxCollider2D 지면(`P17_TempGround`)과 플레이어 위치 이동(tether)** 을 사용했고, 체력은 일부 구간에서 `ResetHealthToMax` 로 유지했다.
- Hit/Stage 2/사망은 `Stats.TakeDamage(float)` 직접 호출로 유발했다(플레이어 공격 입력 미사용). 보스 AI 는 실제 코드가 구동.
- 세션 A(Stage 1 -> Stage 2 -> 사망), 세션 B(Stage 1 전투 회귀, 체력 감소 허용). 세션 A 의 Animator 로그는 세션 B 시작 때 덮어써졌고, 세션 A 수치는 실행 중 출력한 로그에서 옮겼다(`evidence/play-A-death.log`, `play-A-sprite-name-counts.txt` 만 원본 보관).
- `capture_game_view` 캡처는 카메라가 플레이어를 따라가 보스가 화면 밖이라 증거로 쓰지 않았다. 클립/스프라이트 이름 로그가 증거다.

| # | 시나리오 | 관찰 증거 | 결과 |
|---|----------|-----------|------|
| P1 | Idle at rest | A t=0.02: `Idle[WaterSpirit_Idle_S1]`, Move=0, ai=IdleState | PASS |
| P2 | Move on chase / 정지 시 Idle 복귀 | A t=105.42 `Move[WaterSpirit_Move_S1]` Move=1 v=3.0 (ChaseState) -> t=107.27 Move=0 `Idle` (CombatState 진입, 정지) | PASS |
| P3 | Charge: 0.5초 windup 시작 시점에 재생 | B: t=198.35 `Charge` 진입(v=0) -> t=198.85 v=12.0, chg=1 (+0.50s) -> 플레이어 피격 t=199.29. A: 107.28 -> 107.79(+0.51s). 종료 후 exit time 으로 Idle(약 1.09s 후) | PASS |
| P4 | Repel: WakeRepel 0.4s windup 시작에 1회, 데미지 시점 재트리거 없음 | B: t=203.86 `Repel` 진입 -> t=204.26 플레이어 HP 73->63 (+0.40s, -10 = RepelDamage). 232.68 -> 233.09 (+0.41s, -10). 세션 B 전체 로그에서 `T:`(미소비 트리거) 플래그 0건 | PASS |
| P5 | Ranged: 0.4s 조준 대기 전에 재생 | B: t=194.60 `Ranged` 진입 -> t=195.00 SpiritProjectile 0->1 (+0.40s) -> t=195.72 플레이어 HP -12 (= ProjectileDamage). 8회 이상 동일 패턴 | PASS |
| P6 | Stealth: 헤비콤보 시작에 재생 (보스+분신) | A t=297.08 BOSS/CLONE x2 모두 `Stealth[WaterSpirit_Stealth]`, hv=1 -> t=297.70(+0.62s) 전원 `Charge_S2`. (독립 Stealth 후보는 Spirit 패턴 후보에 없어 헤비콤보 경로만 존재) | PASS |
| P7 | Hit: 실제 보스 피격 시 | A t=263.42 `TakeDamage(10)` -> `T:Hit` -> t=263.52 `Hit[WaterSpirit_Hit_S1]` -> 종료 후 Move. Stage 2 에서도 t=288.80 `Hit_S2` | PASS |
| P8 | Stage 2 override + 스프라이트 변경(Clone 이전) | A t=288.38 HP 500 -> `ctrl=OVR:WaterSpirit_Stage2`, 클립명 `_S1` -> `_S2`. 스프라이트 이름 `Idle1_n/Move1_n/...` -> `Idle2_n/Move2_n/Clone2_0/Hit2_n/...` (`evidence/play-A-sprite-name-counts.txt`). 교체 후 Clone 상태 진입(t=288.46) | PASS |
| P9 | Clone 상태(보스 사이클 진입 시)가 보이는가 (재검증 17-06) | 클립 1.17s. 재진입 관측(Clone 진입 -> 다음 공격 상태 진입): 세션1 49.32 -> 50.52(1.20s), 65.36 -> 66.56(1.20s), 81.34 -> 82.51(1.17s), 97.33 -> 98.64(1.31s), 114.87 -> 116.07(1.20s); 세션2 207.08 -> 208.28, 224.70 -> 225.91, 241.21 -> 242.39(Idle), 257.18 -> 258.38, 273.68 -> 274.88, 291.61 -> 292.81 (모두 1.17~1.21s, 마지막 샘플 nt 0.86~0.90). 최초 진입: 세션1 31.84, 세션2 20.37 에서 Clone 이 시작되나 임계 데미지로 큐잉된 실제 Hit 가 0.04s 만에 중단(Hit 이후 Idle, 다음 패턴은 Clone 진입+1.17s 뒤인 32.95 / 21.49 에 시작, 즉 판단 보류 자체는 적용됨). 증거 `evidence/play-C-session1-stage2.log`, `play-C-session2-stage2-start.log` | PASS (재진입 11회 전부 >= 1.17s; 최초 진입은 계획이 허용한 실제 Hit 중단) |
| P10 | 분신 2기 Stage 2 애니메이션 | 사이클마다 CLONE 2기 생성, `ctrl=OVR:WaterSpirit_Stage2`, 클립 `Idle_S2/Move_S2/Ranged_S2/Charge_S2/Stealth` 재생 확인 (t=290.01~, 297.08~, 305.29~). 분신 헤비콤보 Stealth(297.08) -> Charge(297.70) 가 보스와 동시 | PASS |
| P11 | Groggy true -> 이후 false | A t=300.28 `Groggy[WaterSpirit_Groggy_S2]` Groggy=1 ai=GroggyState -> t=305.29 Groggy=0 (Clone 상태, Stage2CombatState 재진입). 약 5.0s | PASS |
| P12 | Death 클립이 비활성화 전에 끝까지 재생 (재검증 17-06) | 세션2(Stage 2, 비그로기 Ranged 중 사망): 사망 유발 t=293.25, Hit(0.05s) 후 Death 진입 t=293.34, nt 0.07 -> 0.92 연속 관측, 마지막 Death 샘플 t=294.93 nt=1.00, 비활성 첫 샘플 t=294.99 (Death 진입+1.65s >= +1.58s). 위치 불변. 증거 `evidence/play-C-session2-death.log`. **별도 발견(P19)**: 그로기 중 사망시에는 아래 참조 | PASS (일반 Stage 2 사망 경로) |
| P13 | 사망 중 이동/공격 없음 | `evidence/play-A-death.log`: 위치 불변 (-70.29,-91.14), v=0.0, chg=False, 사망 구간에 Death 이외 애니/공격 상태 없음. AI 객체는 Stage2CombatState 로 남지만 Update 가 `_dying` 으로 즉시 반환 | PASS |
| P14 | 사망 시 분신 정리 | 같은 로그: 유발 시 clones=2 -> 첫 샘플(+0.16s) clones=0 | PASS |
| P15 | 사망 시 보스 격파 저장 | `save.json` 226B -> 250B, `BossProgress: {"WaterSpirit": true}`, 콘솔 `[SaveLoadManager] Saved to ...save.json` | PASS |
| P16 | 격파 저장 후 로드 시 보스 부재 (Phase 15) | `SaveLoadManager.LoadSlot(0)` 후 씬 재로드: `WaterSpirit active=False`, `IsBossDefeated=True`. 참고: 세이브를 로드하지 않고 곧바로 Play 하면 캐시가 비어 보스가 활성(`defeated=False`) - Phase 15 설계(LoadGame 경로에서만 복원)와 일치 | PASS |
| P17 | 패턴 선택/데미지/투사체 스폰/쿨다운 무회귀 | 세션 B: Ranged(투사체 0->1->0, 12), Charge(15), Repel(10) 피해량이 Inspector 값(ProjectileDamage 12/ChargeDamage 15/RepelDamage 10)과 일치. 패턴 간 간격 약 3~4s 로 Charge/Ranged/Repel 순환 관측 | PASS (피해량/스폰/순환; 아래 U 항목 제외) |
| P18 | Stage 2 반복 사이클 (분신 생성 -> 일반 패턴 -> 헤비콤보 -> 그로기 -> 재진입) | A: 288.38 진입 -> 297.08 헤비콤보 -> 300.28 Groggy -> 305.29 재진입(Clone, 새 분신 2기). 사이클 1회 반복 관측 | PASS |
| P19 | (신규 발견, 17-06) 그로기 상태에서 사망 | 세션1: 사망 유발 t=129.29 시 보스는 GroggyState(Groggy bool=true). Hit(129.39) -> Groggy(129.43) -> Death(129.46) 진입 후 0.05s 만인 129.51 에 Groggy 로 되돌아가 Groggy 클립이 재생되고(nt 0.00 -> 0.40), 비활성은 t=131.41 (유발+2.12s, 상한 길이+0.5s 에 의해 종료). Death 클립은 사실상 재생되지 않음. 원인(추정, 미확인): Groggy bool 이 true 로 남아 Any State -> Groggy 전이가 Death 를 덮어씀. 증거 `evidence/play-C-session1-stage2.log` 끝부분 | **FAIL** (그로기 중 사망 한정, 후속 갭) |
| U1 | Exhaustion -> WakeRepel 강제 체인 | Repel 은 관측되나 Exhaustion 진입 자체는 로깅하지 않아 직접 확인 못 함. 플레이어가 6유닛 거리(WakeRepel maxDistance 1.5)라 자력 선택은 불가해 체인 경유로 추정될 뿐 | UNVERIFIED |
| U2 | 넉백(RepelForce) 크기/방향 | 측정 안 함 (tether 로 플레이어 위치를 조정해 신뢰 불가) | UNVERIFIED |
| U3 | 분신 피격 무적/데미지 분기, 분신 사망 연출 | 직접 타격 시도 안 함 | UNVERIFIED |
| U4 | 시각적 외관(스프라이트 품질, 위치) | 보스가 화면 밖이라 캡처 미사용. 스프라이트/클립 이름 로그만 확보 | UNVERIFIED |

## Animator 전이 확인

컨트롤러 구조(`get_animator_controller`): Idle<->Move(Move bool), Any State -> {Groggy(bool), Charge, Repel, Clone, Ranged, Stealth, Hit, Death}(hasExitTime=false), 공격 상태들은 exit time 으로 Idle 복귀.

| 상태 | Any State 진입 | exit time 복귀 | 결과 |
|------|----------------|----------------|------|
| Charge | 관측 (B 198.35) | 관측 (Charge 1.08s 클립 후 Idle, A 107.28 -> 108.37) | PASS |
| Repel | 관측 (B 203.86) | 관측 (A 214.56 -> 216.15, 1.59s) | PASS |
| Ranged | 관측 (B 194.60) | 관측 (B 194.60 -> 196.69, 2.09s) | PASS |
| Stealth | 관측 (A 297.08) | **미관측**: 매번 0.62s 후 Charge 트리거가 덮어씀 | 진입 PASS / 복귀 UNVERIFIED |
| Clone | 관측 (A 288.46, 290.01, 305.29; 17-06 11회 이상) | 17-06 에서 Clone 이 nt 0.86~0.90 까지 유지되고 이후 공격 상태(Ranged/Charge/Idle)로 전이됨. 클립 종료(nt 1.0) 자체는 샘플 간격(약 1s) 때문에 직접 보지 못함 | 진입 PASS / 가시성 P9 PASS / 순수 exit time 복귀는 UNVERIFIED |
| Hit | 관측 (A 263.52, 288.80) | 관측 (0.45~0.9s 후 Move/Idle) | PASS |
| Groggy | 관측 (A 300.28, Groggy bool) | bool false 로 해제 (305.29) | PASS |
| Death | 관측 (A 368.14, 17-06 293.34) | 종료 상태(복귀 없음). 17-06 세션2 에서 nt 1.00 까지 재생 후 비활성 | 진입 PASS, 재생 완료 P12 PASS (그로기 중 사망은 P19 FAIL) |

- stale trigger: 소비되지 않고 장시간 남은 트리거는 없었다. 최대 잔존은 약 0.1s (A 263.42 `T:Hit` -> 263.52 Hit 진입, 288.38 `T:Clone T:Hit` -> 288.46/288.80, 368.04 `T:Death` -> 368.14 Death). 세션 B 로그에서 `T:` 0건.
- 관찰: Ranged/Hit 등 진행 중에 Stage 2 전환/사망 트리거가 들어오면 약 0.1s 지연 후 소비됨.
- 관찰: Stage 2 진입 첫 프레임에 생성된 분신 스프라이트는 프리팹 기본값(`HeroKnight_45`)이었다가 곧 `Idle2_0` 이 됨. 최초 진입 때 생성된 분신 2기는 0.08s 만에 ChaseState 전환(Exit -> CleanupClones)으로 제거되고, 이어 재진입에서 다시 생성됨(기존 제어 흐름이며 Phase 17 변경 아님).

## Console 및 회귀

- Console(세션 A/B 전체 `console` 조회): Animator 관련 경고/오류(controller 없음, 파라미터 없음, 타임아웃) **0건**. 남은 항목은 `The referenced script (Unknown) on this Behaviour is missing!`(15~20회, 씬 로드 시), `DontDestroyOnLoad only works for root GameObjects...`(3~7회), `InputHandler: Input Action Asset이 할당되지 않았습니다!`(1회), 그리고 내 `capture_game_view` 경로 오류 2건(도구 사용 오류). WaterSpirit 프리팹의 두 스크립트 GUID 는 정상 해석되고 메시지에 Animator/WaterSpirit 언급이 없어 Phase 17 변경에 귀속되지 않는다. Phase 17 이전 기준선과 A/B 비교는 하지 않았다. -> PASS (Phase 17 관련 신규 경고 없음)
- 회귀: 패턴 순환, 피해량(12/15/10), 투사체 스폰, Stage 2 사이클, 그로기 5s, 사망 시 분신 정리/저장/로드 후 부재 모두 관측(P3~P18).
- 정리: Play 종료 후 `save.json` 을 백업본으로 복원(SHA-256 `7df6fe74...8907`, 원본과 동일). Play 전용 임시 지면/훅은 Play 종료로 소멸. `capture_game_view` 가 `Assets/Temp` 로 저장한 PNG 는 삭제. `git status` 는 실행 전 스냅샷과 동일(씬/프리팹 변경 없음; `2 stage.unity` 는 기존부터 수정 상태이며 저장하지 않음).
- 재검증 (17-06): 2026-10-01, 17-05 수정 후 같은 방법(EditorApplication.update 훅 + 임시 지면 + 플레이어 tether)으로 Play 2세션. 세션1 = Stage 2 사이클 5회 + 그로기 중 사망, 세션2 = Stage 2 사이클 다수 + 비그로기 사망. 회귀 스팟체크: P3 Charge 창 Charge 애니 진입 -> IsCharging=True/v=12 가 +0.52s(101.15 -> 101.67), +0.52s(116.07 -> 116.59), 헤비콤보 +0.54s(89.74 -> 90.28) = PASS; P11 Groggy=1 약 5.0s(44.30 -> 49.32, 60.33 -> 65.36 등) = PASS; P13 사망 중 위치 (246.87,-146.22) 불변 = PASS; P14 사망 직후 clones 2 -> 0 (첫 샘플) = PASS; P15 세션1 사망 후 save.json `BossProgress.WaterSpirit=true` 관측 = PASS. 정리: save.json 을 백업본에서 복원(SHA-256 `7df6fe74...8907` 일치), Assets/Temp 없음, git status 는 실행 전과 동일(추가는 evidence/*.log 뿐). 계획 문구와 달리 조건부 eval 폴링은 일부 지연되어 사망 유발은 훅 내부 조건으로 수행했다.
- 환경 노트: `Application.runInBackground` 는 런타임에서만 true 로 설정(ProjectSettings 변경 없음). 이 설정이 없으면 창이 비활성일 때 Play 루프가 멈춘다.

## 현재 상태

- 정적 S1-S10 PASS. Play 모드 P1-P18 PASS (17-06 재검증으로 P9, P12 가 PASS 로 전환, P3/P11/P13/P14/P15 회귀 없음).
- **FAIL 1건 (신규, 17-06 관측)**: P19 그로기 중 사망 시 Death 클립이 Groggy 로 덮어써져 사실상 재생되지 않음(Death 0.05s 후 Groggy 재생, 비활성은 길이+0.5s 상한에서 발생). 원인은 추정이며 코드 수정은 이 플랜 범위 밖이다. 일반 Stage 2 경로(P12)는 PASS.
- UNVERIFIED: U1 Exhaustion 체인, U2 넉백, U3 분신 피격/사망, U4 시각 품질, Stealth 및 Clone 의 순수 exit time 복귀.
- Phase 완료 판정: P9/P12 갭은 닫혔으나 P19 가 새로 관측되었으므로 "필수 항목 전부 PASS" 기준에는 미달이다. P19 를 수정하거나 사용자가 수용 가능으로 판단하기 전까지 Phase 17 완료로 선언하지 않는다.
