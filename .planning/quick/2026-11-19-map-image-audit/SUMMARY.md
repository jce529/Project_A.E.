# 맵 이미지 점검·정리 (2026-11-19)

방법: Unity CLI(에디터 6000.3.10f1)로 씬 8개를 열어 읽기 전용 점검(저장 없음), `AssetDatabase.GetDependencies`로 역참조 구축, `grep`으로 코드의 `Resources.Load`·경로 문자열 확인. 점검 후 처음 열려 있던 `TestScene`으로 복구. 삭제·수정 없음.

대상: Build Settings 3개(MainMenu, 1 stage, Tutorial Map) + `Assets/Scenes/` 나머지 5개. **`2 stage` 씬은 존재하지 않는다.** 맵 이미지 후보 176개 (`Assets/Tile`, `Assets/backgorund`, `Assets/Sprite`, `Assets/map`, `ImportedAsset/Resources/MineTile`, `Porforever`).

## (a) 씬별 점검 표

| 씬 | 문제 | 위치 | 심각도 | 캡처 |
|---|---|---|---|---|
| 1 stage | SpriteRenderer sprite 비어 있음 | `Main Camera/back3` (Default:1) | 중 | evidence/1stage_game.png, 1stage_scene.png |
| 1 stage | Missing Script 1개 | `UI Group/ESC/PauseMenuPanel` | 중 (이미지 문제 아님, 참고) | 〃 |
| Tutorial Map | SpriteRenderer sprite 비어 있음 | `back3` (Default:1, z=11) | 중 | evidence/tutorial_game.png |
| 1 stage / Tutorial Map | UI Image sprite null 45개 | `UI Group/ESC/PauseMenuPanel/...` (단색 패널) | 낮음(의도로 보임) | — |
| MapProgress_Stage1/21/22 | 흰 네모 플레이스홀더 11/21/27개 | Triggers/AreaTriggers/ZoneTriggers 등 | 플레이스홀더(의도된 임시 아트) | evidence/mp_stage1·21·22_scene.png |
| MainMenu | 이상 없음 (Image 12, null 0) | — | — | evidence/mainmenu_game.png |
| SampleScene | 이상 없음 (SR 12) | — | — | evidence/sample_game.png |
| TestScene | 이상 없음 (SR 2) | — | — | evidence/test_game.png |

- 타일맵: 1 stage 8개/255타일, Tutorial Map 8개/40,650타일 — 스프라이트 없는 타일 0, 깨진 참조 0.
- (c) 같은 위치에 서로 다른 이미지 혼재: 1 stage `back1`=`Tile/1 stage/Ground/background/1_.png`, Tutorial Map `back1`=`backgorund/맨 뒤꺼.png`, `back2`=`backgorund/2 1.png`. 같은 역할인데 씬별로 다른 파일을 쓰며(스테이지별 배경이라 의도일 수 있음) `back3`만 비어 있다.
- (e) 가려짐: 정렬 레이어는 모두 Default. 배경 SR은 order 0~2, 타일맵 7~9, 플레이어 100이라 배경이 타일을 가리는 문제는 없음. Tutorial Map 게임뷰 캡처는 카메라 위치가 배경만 비추는 구간이라 타일은 보이지 않았음(에디터 정지 상태, 캡처 시 카메라 추적 없음).
- 맵 씬 Triangle/Square/Circle SR은 Unity 기본 스프라이트(적/트리거 임시 표시)이며 맵 이미지가 아니라 제외.
- 참고: 캡처 도구가 `save_path`를 `Assets/` 하위로 저장해 `evidence/`로 옮겼다(`Assets/.planning`은 제거).
- 콘솔 에러 4건은 점검 이전 기록(`CameraController.target` 미할당, 씬 열기 때문)이며 컴파일 오류 아님.

## (b) 미사용 이미지

분류: 후보 176개 → 사용 중 112 (씬에서 도달 46 + 프리팹/타일에셋 경유만 66) / 확실히 미사용 37 / 불확실 27.

**옮김 (확실히 미사용, 37개, 86,089,528 B ≈ 82.1 MiB)** → `Assets/map/_Unused/` (원래 하위 구조 유지, GUID 유지). 어떤 에셋 의존에도, GUID 텍스트 검색(Assets/ProjectSettings/Packages)에도, 코드 문자열에도 없음.

- `Assets/backgorund/3.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/바닥 1.png`
- `Assets/backgorund/오브젝트 2.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/이속 감소2.png`
- `Assets/Tile/object/오브젝트 2.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/이속 증가1.png`
- `Assets/Tile/background/1.png`
- `Assets/Tile/background/맨 뒤꺼2.png`
- `Assets/backgorund/제목 없음-1.png`
- `Assets/Tile/1 stage/Ladder/KakaoTalk_20260501_224318529_02.png`
- `Assets/Tile/background/맨 뒤꺼.png`
- `Assets/backgorund/1.png`
- `Assets/Tile/object/오브젝트 1.png`
- `Assets/backgorund/KakaoTalk_20260212_200435374_02.png`
- `Assets/Tile/object/오브젝트 3.png`
- `Assets/backgorund/KakaoTalk_20260329_222838290.png`
- `Assets/backgorund/KakaoTalk_20260212_200435374_01.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/바닥 2.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/이속 증가2.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/KakaoTalk_20260501_224318529.png`
- `Assets/backgorund/2.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/이속 증가(웅덩이.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/이속 감소1.png`
- `Assets/backgorund/KakaoTalk_20260212_200435374.png`
- `Assets/Tile/Ground/TutoSurface/이속 증가(웅덩이.png`
- `Assets/Tile/object/오브젝트 4.png`
- `Assets/backgorund/오브젝트 1.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/KakaoTalk_20260501_224318529_01.png`
- `Assets/backgorund/4.png`
- `Assets/Tile/background/3.png`
- `Assets/Tile/Ground/Surface/3.png`
- `Assets/backgorund/KakaoTalk_20260212_200435374_03.png`
- `Assets/backgorund/b90c60abd8298af3.png`
- `Assets/Tile/background/4.png`
- `Assets/Tile/background/2.png`
- `Assets/backgorund/오브젝트 3.png`
- `Assets/Tile/1 stage/Ground/TutoSurface/공중 타일 1.png`

용량의 대부분: `backgorund/제목 없음-1.png` 52.3 MiB, `KakaoTalk_20260329_222838290.png` 17.4 MiB.
같은 크기라 중복으로 보이는 쌍: `backgorund/{1,3,4,2,오브젝트1~3}.png` ↔ `Tile/background/*`, `Tile/object/*` (둘 다 미사용이라 모두 이동).

**남김 (불확실, 27개, 4,622 B)**: `ImportedAsset/Resources/MineTile/Texture/{Border,Misc,Gems,Background}/*` — 참조는 없지만 `Resources` 폴더 안이라 빌드에 포함되며 런타임 로드 가능성을 배제할 수 없어 옮기지 않았다. (코드의 `Resources.Load`는 프리팹/오디오/`Items`만 대상이며 이 경로는 없음.)

**남김 (사용 중이나 씬에서 안 닿음, 66개)**: 타일 에셋/`Ground.prefab`/팔레트에서만 참조되는 타일 이미지(`Tile/3 stage/ground/*` 등). 사용 중으로 분류.

검증: 이동 후 전 에셋 의존 재검사 missing 0, `compiling:false`, `compilationFailed:false`. git은 이름 변경(R) 74건(png+meta 37쌍) + 새 폴더 meta 12건(A)으로 인식.

## (c) 사용자 판단 필요
1. `back3` SpriteRenderer(1 stage, Tutorial Map)가 비어 있다. 의도적 빈 슬롯인지, 이동된 `backgorund/3.png`·`4.png` 등을 쓰려던 것인지 확인 필요(해당 파일은 `_Unused`에 있어 GUID 유지, 다시 연결 가능).
2. 큰 파일(`제목 없음-1.png` 등 70 MiB)은 원본 보관 가치 확인 후 삭제 여부 결정(이번엔 삭제 안 함).
3. MineTile 불확실 27개를 `_Unused`로 옮길지.
4. `PauseMenuPanel` Missing Script(1 stage) — 이미지 아님, 별도 처리 필요.
5. `2 stage` 씬이 없음(요청 배경과 불일치) — 앞으로 추가될 씬인지 확인.
