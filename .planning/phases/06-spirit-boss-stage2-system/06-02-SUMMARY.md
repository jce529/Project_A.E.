# Phase 6: Stage 2 Orchestration (Plan 06-02) SUMMARY

## 1. 구현 내용

### SpiritStealth.cs (신규)
- **은신 메커니즘 (D-04):** `ExecuteAttack` 호출 시 모든 자식 `Collider2D`를 비활성화하여 피격 불가 상태로 전환. `StealthDuration` 동안 대기 후 텔레포트.
- **링 텔레포트 (D-05):** 플레이어(Target) 위치를 중심으로 `MinTeleportRadius` ~ `MaxTeleportRadius` 사이의 환형 범위 내 랜덤 지점으로 이동.
- **복구:** 텔레포트 직후 모든 콜라이더를 다시 활성화하여 정상 상태로 복귀.

### SpiritController.cs (업데이트)
- **헤비콤보 명령 수신 (D-09):** `TriggerHeavyCombo()` 메서드와 `HeavyComboRoutine` 코루틴 추가.
- **시퀀스 자동화:** 진짜 보스와 분신 모두 이 메서드를 통해 `SpiritStealth` → `SpiritCharge` 연계를 순차적으로 실행.

### Stage2CombatState.cs (본격 구현)
- **분신 관리 (D-06):** `Enter` 시점에 `DummyPrefab`을 2회 `Instantiate`하고 `SpiritStats.IsDummy=true` 설정 및 참조 리스트 보유.
- **사이클 오케스트레이션 (D-10):** 일반 패턴 3회 실행 후 헤비콤보 단계로 자동 전환.
- **동기화 (D-09):** 헤비콤보 진입 시 보유한 모든 분신과 본체에 동시에 `TriggerHeavyCombo()` 호출.
- **그로기 전환 (D-08):** 헤비콤보 시퀀스 종료(시간 추적) 후 모든 분신을 `Destroy`하고 `GroggyState`로 전환하여 5초간 무력화.

## 2. 검증 결과 (Verification)

| 항목 | 검증 방법 | 결과 |
| :--- | :--- | :--- |
| **은신+텔레포트** | `SpiritStealth.cs` 내 `Collider2D.enabled`, `Random.insideUnitCircle.normalized` 확인 | **PASS** |
| **분신 스폰** | `Stage2CombatState.Enter` 내 `Object.Instantiate` 및 `IsDummy` 설정 확인 | **PASS** |
| **사이클 카운팅** | `SelectAttackStrategy` 내 `_patternsExecuted` 카운터 및 임계치(3회) 비교 확인 | **PASS** |
| **헤비콤보 배분** | `TriggerHeavyComboCycle` 에서 본체+분신 리스트 순회 호출 확인 | **PASS** |
| **분신 삭제+그로기** | `OnHeavyComboFinished` 에서 `Object.Destroy` 및 `ChangeState(new GroggyState())` 확인 | **PASS** |

## 3. 다음 단계 (Next Steps)
- **Phase 6 완료:** 모든 Stage 2 핵심 로직이 구현되었으므로, 실제 밸런싱(쿨다운, 데미지 조정) 및 시각 효과(v3.0+) 단계로 진행 가능.
- **Unity Editor 설정:** `SpiritController` 인스펙터에서 `DummyPrefab`, `StealthDuration`, `Min/MaxTeleportRadius` 등 Plan 01에서 추가된 필드 값이 올바르게 할당되었는지 확인 필요.
