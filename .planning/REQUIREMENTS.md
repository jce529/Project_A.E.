# Requirements — Milestone v2.0: 물의_정령_보스_구현

> **Scope:** 2스테이지 구조의 '분노한 물의 정령' 보스 순수 로직·상태머신 구현  
> **Constraint:** 애니메이션·시각 이펙트 제외  
> **Phase 번호:** Phase 5부터 시작 (v1.0 Phase 1~4 완료)

---

## v2.0 Requirements

### CORE — 보스 엔티티 기반

- [x] **CORE-01**: 물의 정령 보스가 독립 GameObject로 씬에 배치되고 플레이어를 감지하면 전투 상태로 진입한다
- [x] **CORE-02**: 보스는 HP 시스템을 가지며, 피격 시 HP가 감소한다
- [ ] **CORE-03**: HP가 50% 이하로 떨어지면 스테이지 2로 1회 전환된다
- [x] **CORE-04**: HP가 0이 되면 보스가 사망 처리된다

### S1 — 스테이지 1 공격 패턴

- [x] **S1-01**: 중거리 돌진 — 빠른 속도로 플레이어 방향 직선 돌진 후 쿨다운
- [x] **S1-02**: 자동추적 투사체 — 발사 시 플레이어 위치를 향해 날아가며 히트 시 데미지
- [x] **S1-03**: 거리유지 튕겨내기 — 플레이어 근접 시 knockback + 데미지

### S2 — 스테이지 2 추가 패턴

- [ ] **S2-01**: 스테이지 1 패턴(S1-01, S1-02, S1-03) 전부 유지
- [ ] **S2-02**: 은신 — 비활성화(콜라이더 off, 피격 불가) 후 다른 위치에 재등장
- [ ] **S2-03**: 분신 생성 — 진짜 1 + 분신 2 = 총 3개 동시 존재
- [ ] **S2-04**: 분신은 동일 공격 패턴 수행, 데미지 0
- [ ] **S2-05**: 진짜 보스에게만 피격 데미지 정상 적용 (`isDummy` 플래그 분기)

---

## Future Requirements (v3.0+)

- 애니메이션 및 시각 이펙트 연동
- 스테이지 전환 연출 (컷씬, 화면 효과)

---

## Out of Scope

- 애니메이션, 시각 이펙트, 파티클 — 이번 마일스톤 범위 외
- 스테이지 전환 연출 — 로직 검증 후 별도 마일스톤

---

## Traceability

| REQ-ID  | Phase | Status  |
|---------|-------|---------|
| CORE-01 | Phase 5 | Complete |
| CORE-02 | Phase 5 | Complete |
| CORE-03 | Phase 6 | Pending |
| CORE-04 | Phase 5 | Complete |
| S1-01   | Phase 5 | Complete |
| S1-02   | Phase 5 | Complete |
| S1-03   | Phase 5 | Complete |
| S2-01   | Phase 6 | Pending |
| S2-02   | Phase 6 | Pending |
| S2-03   | Phase 6 | Pending |
| S2-04   | Phase 6 | Pending |
| S2-05   | Phase 6 | Pending |

---

## Previous Milestone Requirements (v1.0 — 보스_물괴물_구현)

> 하기 요구사항은 v1.0에서 모두 완료 및 검증됨. 참조용으로 보존.

- ✓ REQ-WM-01: 물 속성 회복 필터
- ✓ REQ-WM-02: 공격 시 자가 HP 소모
- ✓ REQ-WM-03: NewBoss 상속 구조 재사용
- ✓ REQ-WM-P1~P4: 날씨/웅덩이/폭발/광폭화/장판 시스템
- ✓ REQ-WM-X-01: Player 레이어 타겟팅
