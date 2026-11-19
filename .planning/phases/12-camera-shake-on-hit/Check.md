# Phase 12 — 피격 시 카메라 흔들림 검증 상태

**요약:** 12-01-PLAN.md Task 0~2 완료(코드 삽입 + 정적 회귀 12항목 전부 PASS). 2026-09-10
Unity MCP로 Task 3을 실측했으며 기본 흔들림·감쇠·리프레시·고정 강도·튜닝·0초 가드는 통과했다.
같은 날 확인된 `Time.timeScale = 0` 실패(BUG-005)는 `ApplyHitShake()`에 일시정지 프레임 가드를
추가해 수정했고 재검증에서 1604프레임 위치 고정·재개 후 이어짐·0초 가드까지 전부 PASS했다.
보스 관련 항목은 사용자 요청으로 이번 검증에서 제외했다.

## 무엇이 바뀌었나
`CameraController` 에 `shakeMagnitude`(0.3)/`shakeDuration`(0.25) Inspector 필드 2개, `Shake()`,
`ApplyHitShake()` 삽입. `PlayerStats.TakeDamage` 에서 `base.TakeDamage` 직후 호출. `HP.cs` 0줄
변경 — 보스 피격 시에는 흔들리지 않는다.

## 실제 체크리스트 위치
`Assets/Camera/Check.md` 479행~ (Phase 12 섹션, 6개 소섹션 18개 이상 항목):
1) 기본 흔들림 (D-01~D-05)
2) 연속 피격 리프레시 (D-06)
3) 사망 피격 (D-03)
4) 보스 구역 동작 (D-07)
5) 파이프라인 회귀 (D-08 + Phase 9/10 회귀)
6) Inspector 튜닝 (D-09)

## 정적 회귀 검사 (2026-08-19, 12/12 PASS)
Inspector 필드 개수, 공개 트리거, 감쇠 헬퍼, 호출 위치(재앵커 블록 바깥), 사인파/AnimationCurve
미사용, 누적 금지, 재클램프 없음, 보스존 분기 불변, 인코딩 무결성(비-ASCII 5줄 유지),
HP.cs 0줄 변경 — 전부 PASS.

## 현재 상태
**비보스 항목 검증 완료.** 상세 프레임 실측은 `Assets/Camera/Check.md`의 Phase 12 결과 기록과
BUG-005 재검증 표에 있다. BUG-005는 2026-09-10 수정·재검증으로 해결됐다. 비보스 항목 중 남은 것은
사망 전환 순간의 마지막 피격(수동 관찰)뿐이다. 보스 피격·보스 구역 항목은 별도 보류한다.
