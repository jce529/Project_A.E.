# Phase 21 버그 목록

| ID | 제목 | 심각도 | 상태 | 문서 |
|----|------|--------|------|------|
| BUG-011 | ControlsSettingsPanel 의 BasicAttack 엔트리(bindingIndex 1) 범위 초과로 라벨 갱신이 중단됨 | 중간 | 확인됨 — 미해결 (Inventory 엔트리는 우회 적용) | [BUG-011](BUG-011-controls-basicattack-bindingindex-out-of-range.md) |

## 관찰됨 (버그 문서 없음 - 원인 미확정)
- `CameraBoundsTrigger.OnTriggerExit2D` 에서 `NullReferenceException` 1회 (Play 검증 중 `CameraBoundsTrigger.cs:67`, `CameraController.Instance` 접근). 프로젝트의 Console Error Pause 때문에 Play 가 자동 정지했다. Phase 21 변경과 무관해 보이며(재현 조건 불명) 추가 조사가 필요하다.
- 일시정지(Paused) 상태에서도 메뉴 버튼 LMB 클릭이 BasicAttack 으로 새는 기존 누수가 있다. Phase 21 은 Inventory 상태만 막았다 (21-01 SUMMARY 참고).
