# Phase 28 Plan 02 Summary — 하얀 네모 플레이스홀더 통일

정적/에디터 스크립트 검증만 수행(Play 모드 미실행). 기준: `주창은` 병합(3d00614) 후 작업.

## 결과
- 공용 스프라이트 `Assets/map/Placeholder/WhiteSquare.png`: 16x16 px, PPU 16 → 정확히 1x1 유닛(sprite.bounds=1x1), Point 필터, 압축 없음, 밉맵 없음.
- 대상: `Assets/Prefabs/MapProgress/*.prefab` 3종(씬 3종은 프리팹 인스턴스라 자동 반영). **플레이스홀더로 바꾼 오브젝트 59개** (Stage1 11, Stage2-1 21, Stage2-2 27). 방식: SpriteRenderer(sprite=WhiteSquare, 흰색) + Transform.localScale 로 크기 지정, 기존 BoxCollider2D 는 `size=sprite.bounds.size`, `offset=sprite.bounds.center` 로 맞춤. 구역 트리거(Stage1Trigger, Area1~6, Zone1~9, 2-1 출구 Gate, TreeMonster 체류 구역)와 Fog 는 알파 0.25, isTrigger 유지. 승강대 Platform 은 콜라이더가 없어 non-trigger BoxCollider2D 를 추가.
- 부모 스케일 변경 시 직접 자식의 월드 위치/스케일을 보존하도록 보정(자식 왜곡 없음).
- `FlagStateVisual` 부착 오브젝트(ArtState_*)는 기본 상태 흰 박스(1x1)만 두고 `arts[]` 는 비워 둠(`spriteRenderer` 필드에 연결). 한 오브젝트에는 한 방식만 사용.
- 컴파일: `compilationFailed:false`. 콘솔 error 1건이 종료 직전 `console_status` 에서 보였으나 에디터를 닫은 뒤라 내용을 확인하지 못함(작업 중 CS 오류 0, 병합 직후 확인 시 consoleErrors 0 이었음).

## 건너뜀
| 오브젝트 | 사유 |
|---|---|
| 이미 아트가 배정된 오브젝트 | 해당 없음 — MapProgress 프리팹에는 스프라이트/Animator 가 배정된 오브젝트가 없었음. `1 stage`, `2 stage`, `Tutorial Map` 은 24~27 스크립트를 참조하지 않아(GUID grep 0건) 편집하지 않음. WaterSpirit/WaterMonster 등 기존 캐릭터 미접촉 |
| On/Off, LeverDefault/LeverChanged, GateClosed/GateOpen (상태 시각 자식) | 루트 흰 박스가 대표. 상태별 아트 도착 시 채움 |
| HintPanel, ViewPanel | UI 패널(비활성), 월드 오브젝트 아님 |
| Stage1SlideResult, Stage22ProgressMachine, Gate 루트(Gates/Gate_*), RisingLift 루트, LowPoint/HighPoint | 로직/마커(렌더링 대상 아님) |

## 렌더=콜라이더 전수 검증
- `eval_file`로 3개 추가 씬을 열어 흰 스프라이트 SpriteRenderer 전체 대상 `renderer.bounds` vs `BoxCollider2D.bounds`(크기·중심) 비교.
- 결과: **총 59, 콜라이더 보유 37, 불일치 0** (허용 오차 0.001, 실측 최대 차 0.0000). 콜라이더 없는 22개는 n/a(렌더 전용: ArtState·Fog·Waterway·Tree/Monster·Waterfall·Wheel 등).
- 전체 표: [evidence/bounds-table.md](evidence/bounds-table.md) (오브젝트별 렌더 크기, 콜라이더 크기, 차이, trigger, 일치 여부).
- Scene 캡처: `evidence/MapProgress_Stage1Progress.png`, `MapProgress_Stage21Progress.png`, `MapProgress_Stage22Progress.png`.

## 에디터/브랜치
worktree 에디터만 열고 닫음(메인 에디터 pid 41988 및 그 워커 미접촉, 종료 후 메인 생존 확인). 에디터가 바꾼 EditorBuildSettings/.vscode/.slnx 는 복원·삭제 후 커밋 제외.
