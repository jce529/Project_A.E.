# Phase 12 — 피격 시 카메라 흔들림 검증 상태

**요약:** 12-01-PLAN.md Task 0~2 완료(코드 삽입 + 정적 회귀 12항목 전부 PASS). **Task 3(Play 모드
검증)만 남았고, 지금 사용자 응답을 기다리는 중** — 이 phase는 다른 phase들과 달리 현재 활성
대기 상태다.

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
**사용자 응답 대기 중.** Unity 에디터에서 위 6개 소섹션을 확인 후 "approved" / "생략" / 실패
항목+증상을 알려주면 `Assets/Camera/Check.md` 에 정직하게 기록하고 phase를 마무리한다.
