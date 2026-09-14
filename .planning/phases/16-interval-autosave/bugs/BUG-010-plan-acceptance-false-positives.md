# BUG-010: Phase 16 검증 명령의 주석 오탐 및 카운트 오류

- 심각도: 낮음 (검증 명세)
- 상태: 수정됨 — 재검증 필요
- 발견일: 2026-09-14
- 발견 경로: execute-phase 16 계획 검토 및 인수 조건 대조
- 영향 범위: 16-01/02 인수 조건, 16-03 정적 회귀 검사

## 증상 및 재현
계획의 csharp 코드 블록을 그대로 파일로 만든 뒤 인수 조건의 grep를 실행한다.
주석에 SaveSlotDialog, SaveAtCheckpoint, NotifySaveWritten, GraphicRaycaster, EventSystem, HealPopup 등이 있어 사용 금지 검사가 실패한다.
16-02 Task 2 예시는 8줄을 추가하지만 기대값은 7줄이다. AutoSaveNotice는 2줄에 3회 등장하지만 grep -c는 3을 기대한다.
16-03의 baseline 대비 작업트리 diff는 기존 미커밋 Assets 변경까지 포함해 5개 파일 제한 검사를 실패시킨다.

## 기대 / 실제
기대: 실제 호출, 의존성, Phase 16 변경 범위만 검증한다.
실제: 설명 주석과 기존 사용자 변경 때문에 정상 구현이 실패한다.

## 근거 / 원인
- ../16-01-PLAN.md Task 1 코드 및 acceptance_criteria
- ../16-02-PLAN.md Task 1/2 코드 및 acceptance_criteria
- ../16-03-PLAN.md 정적 회귀 항목 1, 12, 16, 21
원인: grep의 행 수/출현 수 혼동, 주석과 실행 코드 미분리, dirty 작업트리 기준선 누락.

## 수정 방향 및 완료 기준
원본 계획을 보존하고 Check.md에 원래 명령 실제 결과와 오탐을 기록한다.
주석 제외 코드 검사, 실제 diff 추가 줄 수, 작업 전 SHA-256 및 baseline..HEAD 커밋 범위로 대체 검증한다.
각 대체 검증의 실측 결과와 커밋을 기록하면 해결 처리한다.

## 관련 문서
발견: ../16-01-PLAN.md, ../16-02-PLAN.md. 검증: [Check.md](../Check.md).

## 해결 기록
- 해결일: 미정
- 수정 커밋: 미정
- 구현: verify-static.cjs로 원문 행 수와 대체 판정을 분리. Check.md에 원문 18/22 PASS, 4 FAIL 및 대체 22/22 PASS 기록.
- 검증: 첫 실행 통과. 수정 커밋 생성 후 재검증 예정.
