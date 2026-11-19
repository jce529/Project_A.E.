# BUG-012: Ground 프리팹의 없는 하위 Transform 참조

- 심각도: 중간
- 상태: 해결됨
- 발견일: 2026-11-19
- 발견 경로: 통합본 Unity 전체 임포트/씬 검사
- 영향 범위: Assets/Tile/Ground.prefab

## 현상과 근거

A와 B의 동일한 71,249바이트 프리팹에 실제 오브젝트 8개, m_Children 7개 중 6개가 없는 fileID를 가리킴. Unity Importer가 Transform child cannot be loaded 오류를 기록.

## 재현

원본의 해당 에셋을 Unity에서 임포트하거나 씬을 열어 컴포넌트 누락/임포트 오류를 확인한다. 통합 전부터 있는 자산 결함이다.

## 기대/실제 결과

기대: 패널 스크립트가 연결되고 프리팹 계층이 유효해야 한다. 실제: 위 참조가 누락되어 Unity가 오류를 기록했다.

## 수정과 완료 조건

파일 안에 실제 존재하지 않는 6개 하위 Transform 링크만 제거. 나머지 오브젝트·타일 데이터·GUID는 보존.

Integration/legacy-asset-repairs.json에 정확한 변경을 기록한다. 재임포트 오류 0, 전체 씬의 누락 스크립트 0 및 실행 검증 통과 후 해결로 표시한다. 관련 계획: ../16-PLAN.md. 수정 커밋: 최종 통합 커밋에서 확인.

## 최종 검증 (2026-11-19)

Integration/verification-summary.json 및 unity-verification.log의 통합 검사 통과. 12개 씬 각 120프레임, 게임 코드 오류 0; 상호작용 전용 검사와 저장/서비스 회귀 통과. 상세 증거는 각 보고서 참조. 수정 커밋은 후속 기록에 기재한다.
