# Phase 10 — 카메라 데드존 3종 검증 상태

**요약:** 코드(10-01~10-03)와 정적 회귀 검사 9항목은 완료됐다. 2026-09-10 Unity MCP로
비보스 핵심 동작(Base Deadzone, Dynamic Offset, Peeking, XY Bounds/Trigger)을 실측 통과했다.
보스 구역과 Gizmo/Inspector 체감 항목은 사용자 요청 또는 자동화 한계로 미검증이다.

## 무엇이 바뀌었나
Base Deadzone(하드컷 정지 박스) + Dynamic Asymmetrical Deadzone(진행 방향 시야 확보 오프셋) +
Input-based Peeking(위/아래 보기) 3종을 Phase 9 CameraController 위에 레이어링. 보스 구역에서는
3개 기법 전부 비활성화되고 Phase 9 레거시 Lerp 로 복귀.

## 실제 체크리스트 위치
`Assets/Camera/Check.md` 88~235행 (Phase 10 섹션):
- 1) Base Deadzone (D-01/D-02/D-03/D-14)
- 2) Dynamic Asymmetrical Deadzone (D-04~D-07)
- 3) Input-based Peeking (D-08~D-13)
- 4) 보스 구역 비활성화 & Phase 9 회귀 (D-15/D-16/D-17)
- 5) Y축 하드컷 데드존 (quick task 260804-q6h 반영분)

## 정적 회귀 검사 (2026-08-04, 9/9 PASS)
인코딩 게이트, 삭제 라인 총량, 읽기전용 파일 무수정, LateUpdate 실행 순서,
ApplyNormalStageCamera 내부 순서, 하드컷 보존, 금지 심볼 부재, 구독 대칭/캐시,
BossZoomTrigger 무변경 — 전부 PASS (상세: Assets/Camera/Check.md 191~209행).

## 참고 — 관련이지만 별개인 quick task 섹션
`Assets/Camera/Check.md` 의 quick task 260805-m41 / 260805-q2u 섹션(257~478행)도 체크박스가
전부 미체크로 남아 있으나, 두 quick task의 실제 구현은 이후 260809-h9k 에서 폐기·대체됐고
260809-h9k 자체는 사용자와 함께 Play 모드 검증을 이미 완료했다 (STATE.md 참고). 즉 이 두 섹션은
**낡은 문서일 뿐 실제로 재검증할 대상이 아니다** — 혼동 주의.

## 2026-09-10 비보스 실측 결과

- X/Y 데드존 내부 정지, 양방향 경계 하드컷 통과.
- Dynamic Offset 방향 부호·좌우 대칭·스무딩·정지 유지·중앙 복귀 통과.
- Peeking 상/하·0.5초 임계·공중/입력잠금 가드·속도 급증 취소 통과.
- XY 화면 가장자리 클램프, CameraBoundsTrigger 진입/이탈, Player 태그 가드 통과.
- 상세 좌표는 `Assets/Camera/Check.md`의 `2026-09-10 Unity MCP 비보스 실측`에 기록했다.

## 현재 상태
부분 통과 — 핵심 비보스 로직은 구현 및 실측 완료. Gizmo/Inspector 체감, 실제 키보드 입력,
씬 전환 구독과 보스 구역 항목은 미검증이다.
