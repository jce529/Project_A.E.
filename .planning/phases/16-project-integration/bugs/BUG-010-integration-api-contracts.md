# BUG-010: 통합 시 입력·상호작용·대화 API 충돌

- 심각도: 높음
- 상태: 해결됨
- 발견일: 2026-11-19
- 발견 경로: 두 현재 워킹트리의 3방향 비교 및 전체 Roslyn 컴파일
- 범위: PlayerAttackBase/InputHandler/PlayerInteraction/DialogueManager/GameStateManager, 에디터 검증

## 재현과 기대 결과

양쪽 Assets를 단순 합치면 중복 입력 경로, 서로 다른 TryInteract 반환 형식, 대화 상태 API, 최신 엔진에서 제거된 GetInstanceID 호출 때문에 컴파일 또는 동작이 충돌합니다. 하나의 키 입력이 하나의 행동으로 처리되고 대화/메뉴 상태가 유지되어야 합니다.

## 근거와 원인

Integration/code-decisions.json의 conflict/both-added 항목 및 작업 폴더 project-merge의 컴파일 로그. 같은 클래스를 두 프로젝트가 서로 다른 계약으로 확장했습니다.

## 수정

공유 입력은 B의 런타임 액션 사본/설정 저장을 유지하고 A의 조작 차단/대화/상호작용 우선 처리를 결합했습니다. TryInteract는 bool을 반환하고 이벤트에는 void 어댑터를 연결합니다. 인스턴스 IsDialogueActive는 보존하고 정적 AnyDialogueActive를 추가했습니다. 에디터의 오브젝트 ID 비교는 GetHashCode로 갱신했습니다.

## 완료 조건과 검증

런타임 196개, 에디터 11개 C# 컴파일 통과. 서비스 회귀와 씬 실행 증거는 Integration/service-verification.txt 및 play-verification.txt에 기록합니다. 실제 실행을 통과하기 전에는 해결됨으로 표시하지 않습니다. 수정 커밋: 통합 저장소의 최초 통합 커밋에서 확인.

## 최종 검증 (2026-11-19)

Integration/verification-summary.json 및 unity-verification.log의 통합 검사 통과. 12개 씬 각 120프레임, 게임 코드 오류 0; 상호작용 전용 검사와 저장/서비스 회귀 통과. 상세 증거는 각 보고서 참조. 수정 커밋은 후속 기록에 기재한다.

- 수정 커밋: `a161ba22d42daf7d7fff6be3d2feb91fa2b48958`
