# Phase 16 verification

2026-11-19. 통합 범위 완료.

- Unity 6000.6.9f1에서 런타임 199개, 에디터 11개 소스 컴파일 통과.
- 정규 빌드 씬 11개와 A 비교 씬 1개: 각 120프레임, 게임 코드 오류 0.
- 모든 검사 씬의 Missing Script 0, 공유 InputHandler/GameStateManager 유일성 검사 통과.
- 대상 선택·거리·중복 콜라이더·프레임 중복·비활성·웅덩이 풀/흡수·잠긴 퍼즐·키 표시: 전용 검사 통과.
- 물 자원 경계/정화/500회 조작 캐시, 대화와 메뉴 중첩, 저장 스키마, 입력 바인딩 복원 통과.
- 저장은 격리 경로에서 원자적 교체와 이전 파일 백업을 검사. 사용자 세이브를 테스트로 덮어쓰지 않음.
- 원본 3,684개 파일 해시 대조: 변경 없음. 최종 에셋 GUID 중복 및 잃어버린 원본 참조 0.

BUG-010, BUG-011, BUG-012, BUG-013 해결 증거는 bugs/ 문서와 Integration 보고서에 연결한다. Phase 15 BUG-008의 초기 Animator 계약도 복구했다. 전체 보스전 애니메이션/클리어 가능성은 기존 수동 UAT 범위로 남는다.

- [BUG-010](bugs/BUG-010-integration-api-contracts.md)
- [BUG-011](bugs/BUG-011-missing-ui-panel-script.md)
- [BUG-012](bugs/BUG-012-ground-prefab-dangling-transforms.md)
- [BUG-013](bugs/BUG-013-legacy-scene-missing-scripts.md)

설치 에디터의 UnityEditor.Search.SearchDatabase 시작 예외는 정확한 에디터 전용 스택에 한해 게임 코드 실패 수에서 분리하고 Integration/editor-environment.txt에 원문을 남겼다. 다른 Error/Exception/Assert는 모두 씬 실패로 집계한다.

검증 주 보고서: Integration/verification-summary.json, play-verification.txt, scene-migration.txt, interaction-verification.txt, service-verification.txt, storage-verification.txt, asset-audit.json, original-preservation.json.
