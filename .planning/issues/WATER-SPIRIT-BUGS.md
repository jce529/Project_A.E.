# Predicted Bugs & Improvements: Water Spirit Boss

물의 정령(Water Spirit) 보스 구현 코드 분석을 통해 발견된 잠재적 버그와 개선 필요 사항을 정리한 문서입니다.

## 1. 분신(Clone) 관리 및 메모리 누수
- **현상:** `Stage2CombatState`에서 생성된 분신들이 특정 상황에서 제거되지 않고 씬에 남음.
- **버그 케이스:**
    - 헤비 콤보 시퀀스 도중 진짜 보스가 사망할 경우.
    - 보스가 강제로 상태가 변하거나(예: 다른 페이즈로 스킵) 씬이 전환될 경우.
- **위험도:** 중 (메모리 누수 및 게임플레이 논리 오류)
- **해결 방안:**
    - `SpiritStats.Die()` 메서드에서 보스 사망 시 등록된 모든 분신(`_clones`)을 `Destroy` 하도록 수정.
    - `Stage2CombatState.Exit()`에서 잔여 분신 정리 로직 추가.

## 2. AI 공격 코루틴 중첩 (Race Condition)
- **현상:** 분신의 AI(SpiritCombatState)와 헤비 콤보 명령(`TriggerHeavyCombo`)이 동시에 실행되어 코루틴이 꼬임.
- **버그 케이스:** 분신이 독자적인 쿨다운으로 공격을 수행하던 중 진짜 보스가 헤비 콤보를 시작하면 두 동작이 겹쳐서 실행됨.
- **위험도:** 고 (예기치 못한 속도 증가, 데미지 중첩, 물리 오류)
- **해결 방안:**
    - `TriggerHeavyCombo()` 호출 시 `StopAllCoroutines()`를 먼저 실행하여 기존 공격을 중단시킴.
    - 또는 분신은 `SpiritCombatState`를 직접 돌리지 않고, 진짜 보스의 상태에 완전히 동기화되도록 구조 변경.

## 3. 은신 중 물리 엔진 충돌 (Falling through Floor)
- **현상:** 은신 중 콜라이더를 끄면 중력에 의해 보스가 맵 밖으로 추락할 수 있음.
- **버그 케이스:** `Rigidbody2D`의 중력이 켜져 있는 2D 플랫폼 게임 환경.
- **위험도:** 고 (보스가 맵 밖으로 사라져 전투 불능)
- **해결 방안:**
    - `SpiritStealth` 시작 시 `Rigidbody2D.linearVelocity = Vector2.zero` 설정.
    - 은신 중 `Rigidbody2D.simulated = false` 또는 `gravityScale = 0` 처리 후 복귀 시 원복.

## 4. 헤비 콤보 동기화 오차
- **현상:** `Stage2CombatState`의 타이머 계산과 실제 코루틴 소요 시간이 어긋남.
- **버그 케이스:** 프레임 드랍이나 델타 타임 계산 오차로 인해 공격이 끝나지 않았는데 그로기 상태로 전환됨.
- **위험도:** 저 (어색한 애니메이션 및 연출)
- **해결 방안:**
    - 타이머 기반(`_heavyComboElapsed`) 대신, 코루틴 종료 시 이벤트를 발생시키거나 플래그를 확인하는 방식으로 변경.

## 5. 상태 인터셉터 논리 오류 (Inheritance Issue)
- **현상:** `GetType() == typeof(CombatState)`는 상속받은 자식 클래스에서 `false`를 반환함.
- **버그 케이스:** `GroggyState` 종료 후 `new CombatState()`를 반환할 경우, 매 프레임 `ChangeState`가 호출되는 무한 루프 발생 가능.
- **위험도:** 중 (CPU 부하 및 보스 동작 정지)
- **해결 방안:**
    - `CurrentState is CombatState` 또는 `CurrentState.GetType().IsSubclassOf(typeof(CombatState))`를 사용하여 상속 구조를 지원하도록 변경.

---
**작성일:** 2026-05-05
**대상 파일:**
- `Assets/Enemy/WaterSpirit/Script/SpiritController.cs`
- `Assets/Enemy/WaterSpirit/Script/SpiritStats.cs`
- `Assets/Enemy/WaterSpirit/Script/States/Stage2CombatState.cs`
- `Assets/Enemy/WaterSpirit/Script/States/Attacks/SpiritStealth.cs`
