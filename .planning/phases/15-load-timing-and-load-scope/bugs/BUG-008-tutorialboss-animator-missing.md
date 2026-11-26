# BUG-008: TutorialBoss Animator 누락으로 Idle 진입 예외

- 심각도: 중간
- 상태: 확인됨 — 미해결
- 발견일: 2026-09-11
- 발견 경로: BUG-007 수정 후 Tutorial Map Play Mode 전환 검증
- 영향 범위: TutorialBoss 상태 머신 초기화와 Tutorial Map 전투 진행

## 현상

Tutorial Map 로드 후 `TutorialBoss.TutorialIdleState.Enter()`가 Idle 트리거를 설정하는 순간 `MissingComponentException`이 발생한다.

```text
MissingComponentException: There is no 'Animator' attached to the "Tutorial Boss" game object,
but a script is trying to access it.
UnityEngine.Animator.SetTrigger (System.String name)
TutorialBoss.TutorialIdleState.Enter (BossController boss)
TutorialBoss.TutorialBossController.Start ()
```

## 재현 절차

1. `Tutorial Map`을 Play Mode로 실행한다.
2. TutorialBoss의 `Start()`와 초기 Idle 상태 진입을 기다린다.
3. Console에서 `TutorialIdleState.Enter()`의 `Animator.SetTrigger` 예외를 확인한다.

## 기대 결과

- TutorialBoss가 필요한 Animator를 가진 상태로 Idle 상태에 진입한다.
- 애니메이션 컴포넌트가 선택 사항이라면 모든 상태가 이를 안전하게 처리한다.
- TutorialBoss 초기화 중 예외가 발생하지 않는다.

## 실제 결과

- `BossController.Awake()`가 `GetComponent<Animator>()`로 Animator를 찾지만 Tutorial Boss 오브젝트에 유효한 Animator가 없다.
- Idle 상태의 트리거 호출에서 `MissingComponentException`이 발생한다.

## 근거

- `Assets/Enemy/NewBoss/Script/BossController.cs`: `Anim`이 없으면 동일 GameObject에서 `Animator`를 조회한다.
- `Assets/Enemy/Tutorial/TutorialBoss/State/TutorialIdleState.cs`: 상태 진입 시 `IdleTrigger`를 설정한다.
- `Assets/Scenes/Tutorial Map.unity`: Play Mode에서 Tutorial Boss의 Animator 누락 오류가 재현됐다.

## 원인

TutorialBoss의 씬/프리팹 배치와 상태 머신의 애니메이션 의존 계약이 일치하지 않는다. 상태 코드는 Animator 트리거 사용을 전제로 하지만 런타임 Tutorial Boss에는 유효한 Animator가 없다.

## 제안 수정 방향

1. TutorialBoss가 사용해야 하는 Animator Controller와 Animator 배치를 확인한다.
2. 애니메이션이 필수라면 씬/프리팹에 올바른 Animator를 연결하고 초기화 시 명확히 검증한다.
3. 애니메이션이 선택 사항이라면 Unity Object의 유효성을 명시적으로 검사하고 모든 상태에서 같은 규칙을 적용한다.

## 완료 조건

- [ ] TutorialBoss의 Animator 계약이 씬/프리팹과 일치한다.
- [ ] Tutorial Map 진입과 Idle 상태 전환에서 예외가 발생하지 않는다.
- [ ] 공격·피격·사망 애니메이션 경로가 회귀 없이 동작한다.
- [ ] 수정 커밋과 Play Mode 검증 결과를 기록한다.

## 관련 문서

- `.planning/phases/15-load-timing-and-load-scope/15-UAT.md`
- `.planning/phases/15-load-timing-and-load-scope/bugs/README.md`
- `.planning/phases/15-load-timing-and-load-scope/bugs/BUG-007-inputhandler-lost-on-scene-transition.md`

## 해결 기록

- 해결일:
- 수정 커밋:
- 검증 증거:
