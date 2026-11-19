# BUG-013: 비교용 1스테이지의 구형 컴포넌트 구현 누락

- 심각도: 중간
- 상태: 해결됨
- 발견일: 2026-11-19
- 발견 경로: 통합본 Unity 씬 검사
- 영향 범위: A의 구형 1스테이지 비교 씬

## 현상과 근거

원본 A 씬은 양쪽 현재 Assets에 없는 GUID 3개를 참조한다. WaterUIController(80e5ff633294c774588593184e492249), boss(bc79f38268778704c94149fbfba14edb), HealthBarController(5c0f78e4acfe21a4594760304164796e). Unity에서 Missing Script 3건이 검출됐다.

## 재현과 기대 결과

A의 1 stage 씬을 통합 코드로 열면 누락 컴포넌트가 나타난다. 기대 결과는 원래 타입과 필드가 복원되고 구형 HUD와 보스 감지가 동작하는 것이다.

## 원인과 수정

옛 스크립트가 삭제되었지만 씬의 참조는 남아 있었다. Git 전체 객체 목록에서 원본 코드와 메타파일을 찾았다. 정확한 블롭 ID는 Integration/recovered-script-blobs.json, 원문은 Integration/RecoveredSources에 보존했다.

Assets/Compatibility/LegacyPhase1에 원래 GUID를 복원했다. boss는 원래 감지 구현을 사용한다. HealthBarController/WaterUIController는 기존 직렬화 필드와 공개 콜백을 유지하면서 공통 PlayerHealthUI/PlayerWaterUI로 전달하는 호환 어댑터를 사용한다. 정규 씬의 구현을 다시 분기하지 않는다.

## 완료 조건과 검증

런타임 199개, 에디터 11개 컴파일 통과. 누락 스크립트 0 및 실제 비교 씬 시작 검증은 Integration/scene-migration.txt, play-verification.txt로 확인한다. 실행 통과 전에는 해결됨으로 표시하지 않는다. 관련 계획: ../16-PLAN.md. 수정 커밋: 최종 통합 커밋에서 확인.
## 최종 검증 (2026-11-19)

Integration/verification-summary.json 및 unity-verification.log의 통합 검사 통과. 12개 씬 각 120프레임, 게임 코드 오류 0; 상호작용 전용 검사와 저장/서비스 회귀 통과. 상세 증거는 각 보고서 참조. 수정 커밋은 후속 기록에 기재한다.

- 수정 커밋: `a161ba22d42daf7d7fff6be3d2feb91fa2b48958`
