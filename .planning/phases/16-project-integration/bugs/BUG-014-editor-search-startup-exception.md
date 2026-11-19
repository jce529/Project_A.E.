# BUG-014: 설치 에디터 검색 인덱스 시작 예외

- 심각도: 낮음
- 상태: 확인됨 — 미해결
- 발견일: 2026-11-19
- 발견 경로: Unity 6000.6.9f1 배치 Play Mode 초기화
- 영향 범위: UnityEditor.Search 검색 인덱스와 에디터 지연 콜백. 게임 플레이어 어셈블리 호출 경로는 아님.

## 현상과 재현

통합본을 배치 모드로 열고 Play Mode에 들어갈 때 SearchDatabase.GetDefaultSearchDatabase -> EnumerateAll 내부에서 ArgumentOutOfRangeException이 발생한다. 정확한 스택은 Integration/editor-environment.txt에 보존했다. 검색 인덱스의 근본 원인은 미확정이다.

## 기대/실제 결과

기대: 에디터 검색 초기화가 예외 없이 끝나야 한다. 실제: 해당 에디터 시작 예외가 기록된다. 별도로 게임 코드의 12개 씬 실행·저장·상호작용 검사는 통과한다.

## 대응과 완료 조건

검증 시작을 delayCall 대신 다음 EditorApplication.update에서 실행하여 검색 초기화 예외가 검사 자체를 유실시키지 않게 했다. 정확히 UnityEditor.Search.SearchDatabase와 SearchInit.IndexationOnStartup을 함께 포함한 에디터 스택만 별도 환경 기록으로 분리한다. 다른 모든 게임 Error/Exception/Assert는 실패 처리한다.

별도 에디터 검색 인덱스 진단 후 시작 예외가 사라지는지 확인해야 한다. 예외를 해결한 것으로 표시하지 않는다. 관련 증거: ../16-VERIFICATION.md 및 Integration/editor-environment.txt. 게임 코드를 바꿔 예외를 숨기지는 않았다.

- 진단 및 검증기 우회 코드 기준 커밋 (에디터 자체 오류는 미해결): `a161ba22d42daf7d7fff6be3d2feb91fa2b48958`
