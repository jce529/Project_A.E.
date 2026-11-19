# 1·2스테이지 화이트박스 SUMMARY (2026-11-19)

## 결과
- 1스테이지(`Assets/Scenes/1 stage.unity`)와 2스테이지(`Assets/Scenes/2 stage/2 stage.unity`, 2-1 → 2-2 가로 배치)에 흰색 임시 타일로 Surface(땅)·Platform(원웨이)·Wall·Ladder 를 칠했다. 기존 Surface 아트 타일은 지우고 칠했다(원본은 `backup/`).
- 좌표 정본: `stage1_zones.json`, `stage2_zones.json`. 설명·가정·검수 기록: `SPEC.md`. 캡처: `evidence/stage1_overview.png`, `evidence/stage2_overview.png`.
- 도구: `Assets/Editor/Whitebox/WhiteboxTools.cs`(`CreateAssets`, `Paint`, `Reach`, `Verify`). 타일: `Assets/Tile/Placeholder/`(`WhiteTile128.png`, `WhiteTile.asset`, `WhitePalette.prefab`). 타일맵 `color` 만 옅게 달리했다(Platform 옅은 파랑/회색, Wall 회색, Ladder 옅은 노랑, 붉은 계열 없음).

## 검증
- 컴파일 오류 0, Play 후 콘솔 에러: 2스테 0, 1스테 1건(기존 BUG-008 `Tutorial Boss` Animator 누락, 이 작업과 무관).
- Verify: 4개 타일맵 모두 JSON 칸 수와 일치(1스테 16006/43/270/0, 2스테 7570/15/492/182). 콜라이더 구성(Surface/Wall = Tilemap+Composite+Static RB, Platform = Effector) 기존과 동일.
- Reach: 링크 없이 1스테 430/739, 2스테 1018/1212. 기믹 링크(02 물 상승 장판, 10 물가르기 막, 물레방아, 7번 수문, 7→8 틈) 포함하면 전부 도달. 닿지 않는 틈은 구덩이 x158–176(폭 17)만 의도적으로 남음.
- 2스테 Play: Player 가 (150.7,-10.1)→y -14.85 에 착지해 정지. 첫 Play 는 콜라이더 생성 지연으로 한 번 떨어졌고 두 번째부터 정상(타일 7.5천 칸 콜라이더 첫 생성 시간).

## 알려진 문제·미작업
- (해결) 1스테 Play 에서 Player 가 기존 `cameraBound`(비트리거 BoxCollider2D) 위에 걸려 떠 있던 문제는 사용자 승인으로 `isTrigger=true` 로 바꿔 해소했다. 이후 Player 가 Surface 위(y -15.96)에 정지한다. 비포커스 에디터는 Play 프레임이 거의 안 돌아 `EditorApplication.Step()` 으로 150프레임 진행해 확인했다.
- 맵 진행 오브젝트(`MapProgress_*`) 위치 정렬: **미작업**. 기존 오브젝트(Tutorial Boss, fallzone, Check Group, portal)는 옛 위치 그대로다.
- 슬라이드는 계단형 지형이고 실제 물 효과는 오브젝트 담당. 2-2 4~6번 세부 길은 층 바닥+사다리 수준.
- 에디터 크래시 1회(작업 중, 사용자가 확인). 같은 시각 메인 에디터도 같이 종료된 것으로 보이며, 이후 이 작업은 워크트리 에디터만 사용했다.

## 병합 시 주의
- 두 씬 파일을 이 브랜치에 커밋했다. 메인 체크아웃에 커밋 안 된 씬 수정이 있으면 `1 stage.unity`, `2 stage/2 stage.unity` 가 충돌한다. 타일맵 `Grid` 하위 데이터와 2스테 `Player` 위치·`Surface` 머티리얼이 바뀐 부분이다. 병합 대신 `Paint` 를 메인 씬에 다시 실행하는 방법도 가능하다.
- `Assets/Editor/Whitebox/`, `Assets/Tile/Placeholder/` 는 신규 폴더라 충돌 없음.
