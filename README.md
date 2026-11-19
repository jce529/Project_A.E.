# AQUA ECLIPSE 통합 프로젝트

Unity **6000.6.9f1**로 이 폴더를 열고 `Assets/Scenes/MainMenu.unity`에서 시작합니다.

원본 `D:\Projeect_A.E_260521`과 `D:\Projeect_A.E\Projeect_A.E`의 현재 파일을 별도 폴더에 병합했습니다. 원본의 미커밋 변경도 포함했습니다. 원본 폴더는 통합 작업의 수정 대상이 아닙니다.

## 통합 기준

- A: 애니메이션 이벤트 기반 콤보, 플레이어 프리팹, 튜토리얼/대화, 나무 보스와 일반 적, 1스테이지 기믹.
- B: 저장 슬롯/사망 복원, 설정/오디오/메뉴, 카메라 줌·경계·데드존·흔들림, 보스 가중치 패턴, 2·3스테이지.
- 공통 입력·상호작용·게임 상태는 단일 경로로 연결했습니다. 기존 공개 클래스와 직렬화 필드 이름은 유지합니다.
- 이전에 검증한 물 자원 캐시·이벤트 UI·웅덩이 풀 개선을 후속 상호작용 변경과 함께 적용했습니다. B의 보스 선택 가중치/쿨다운 정책은 유지합니다.
- 정규 `1 stage`는 B의 확장 레이아웃입니다. A의 배치는 `Assets/Scenes/IntegrationVariants/1 stage - Partner A.unity`로 보존하며 정규 빌드 목록에서는 제외합니다. 두 레이아웃의 지형을 임의로 겹치지 않았습니다.
- 이동된 동일 에셋 12건은 GUID를 기준으로 한 번만 등록했습니다. 같은 경로의 GUID 변경은 재매핑했습니다.
- 충돌한 원본 파일은 `Integration/SourceSnapshots/A`, `B`에 별도로 보존합니다. 이 작업 사본은 Git에서 제외합니다.

## 가용성과 확장

- `PersistentManagers`는 공유 입력·게임 상태·오디오의 수명을 소유합니다. 각 씬에는 UI/카메라/플레이어가 남습니다.
- `PlayerInteraction`이 한 입력에 대상 하나를 선택합니다. 새 상호작용은 `IPlayerInteractable`을 구현합니다.
- `WaterController`의 변경 메서드를 사용하고 `OnWaterChanged`를 구독합니다. `bottles` 배열을 직접 바꾸면 잔량 캐시와 이벤트가 갱신되지 않습니다.
- 저장은 기존 3슬롯 JSON 스키마를 유지합니다. `AtomicFile`이 임시 파일을 완전히 기록한 뒤 교체하며, 이전 파일은 `.bak`에 남습니다. 기존 플레이어 세이브를 검증 목적으로 덮어쓰지 않습니다.
- 패키지는 `Packages/`에 원래 라이선스와 함께 고정했습니다. 이 통합본은 설치된 엔진과 같은 6000.6 패키지 계열을 사용합니다. B의 AI 에디터 보조 패키지는 런타임 의존성이 없어 포함하지 않았고 원래 패키지 선언은 차이 사본에 남겼습니다.
- 빌드 목록에는 메뉴·튜토리얼·1·2·3스테이지 씬 11개를 등록합니다. 새 씬을 추가할 때는 빌드 목록과 포털 목적지를 함께 갱신합니다.

## 검증

상세 결과는 `Integration/`의 `asset-audit.json`, `service-verification.txt`, `storage-verification.txt`, `play-verification.txt`, `scene-migration.txt`를 확인합니다. 결과 파일이 없으면 해당 검증이 아직 완료되지 않은 것입니다.

Unity를 닫고 다음 명령으로 재검증할 수 있습니다. 이 명령은 통합본 씬의 연결을 다시 정리하고 저장하므로 새 작업은 먼저 커밋합니다.

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.6.9f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'D:\Projeect_A.E_Merged' -executeMethod MergeVerification.Run -logFile 'D:\Projeect_A.E_Merged\Integration\unity-verification.log'
```

전체 보스전 완주, 스테이지 클리어 가능성, 화면 연출과 밸런스는 120프레임 시작 검사와 별도로 사람이 확인해야 합니다. 기존 미완료 기능과 버그 기록은 `.planning/`에서 이어갑니다.

## 팀 작업

모든 파트원은 이 통합본을 공통 기준으로 사용하고 기능별 브랜치를 만듭니다. 같은 씬을 동시에 편집하지 않고, 맵·기믹·UI를 프리팹 또는 별도 씬으로 나누면 충돌을 줄일 수 있습니다. 에셋을 옮길 때 `.meta`를 함께 옮깁니다. 라이브러리·캐시·사용자 설정은 공유하지 않습니다.

원본 Git 기준: A `9c14c07dfd56554a82bb2f389f578e4d512617be`, B `34306e3f7305a4d3f9b457282291bb94a515b7a6`. 공통 조상은 A의 위 커밋입니다. 이 폴더의 최초 커밋은 두 워킹트리의 통합 스냅샷이며 원본 커밋 이력을 대체하지 않습니다.

구버전 씬의 누락 스크립트는 Git 원문과 GUID를 회수해 Compatibility/LegacyPhase1에서 호환합니다. 원문과 블롭 ID는 Integration/RecoveredSources 및 recovered-script-blobs.json에 보존했습니다.

현재 설치의 UnityYAMLMerge는 비대화형 실행에서 25초 시간 제한을 초과해 자동 병합 드라이버로 활성화하지 않았습니다. 씬 선택과 컴포넌트 연결 결과는 실제 Unity 검증을 기준으로 확정했습니다.

## 최종 통합 검증 결과

2026-11-19: 정규 씬 11개와 비교 씬 1개가 각 120프레임 실행을 통과했습니다. 런타임 199개/에디터 11개 소스 컴파일, 상호작용 전용 검사, 저장·입력·물·대화 회귀를 통과했습니다. 기존의 빈 UI 스크립트, Ground 계층 링크, 구형 씬의 세 스크립트를 복구했습니다.

흡수는 기본 상호작용의 빈 병 규칙과 커스텀 공격/스킬 입력의 기존 B API를 함께 지원합니다. PlayerAbsorb.requireEmptyBottleForCustomActions로 커스텀 입력에도 빈 병 조건을 적용할 수 있습니다.

에디터 검색 인덱스의 시작 예외는 별도 환경 기록에 남겼습니다. 게임 코드 오류와 섞어 숨기지 않으며 정확한 UnityEditor.Search 시작 스택만 분리합니다. 전체 보스전·연출·밸런스 수동 QA와 기존 미배치 보스 항목은 기존 UAT/BUG 기록에서 이어갑니다.
