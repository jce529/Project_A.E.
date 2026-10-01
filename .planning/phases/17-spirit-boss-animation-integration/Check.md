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
| S8 | Stage2 컨트롤러 교체가 Clone 트리거보다 먼저 | `OnStage2Trigger()`: 컨트롤러 대입 후 `ChangeState(new Stage2CombatState())` (Enter 에서 `PlayAnim("Clone")`) | 교체 -> 진입 순서 | 코드상 순서 일치 (런타임 확인은 아래 Play 항목) | PASS (정적) |
| S9 | 가드된 호출 | `PlayAnim` 은 `CanAnimate`(Anim, runtimeAnimatorController null 검사) 로 보호 | null 안전 | 일치 | PASS |
| S10 | 워크트리 기존 변경 보존 | `git status --short` | 기존 변경(2 stage.unity 등) 유지, 본 플랜 Assets 변경 없음 | 본 플랜은 Check.md/SUMMARY 만 수정 | PASS |

## Play 모드 체크리스트

(Task 2 에서 실측 후 갱신. 관찰 전에는 모두 UNVERIFIED.)

## Animator 전이 확인

(Task 2 에서 실측 후 갱신.)

## Console 및 회귀

(Task 2 에서 실측 후 갱신.)

## 현재 상태

정적 검사 S1-S10 PASS. Play 모드 항목은 아직 미관찰 (UNVERIFIED).
