# Phase 27 SUMMARY — 2스테이지 기믹 구현

구현 완료(정적 점검만, Play/컴파일 미검증). 위치: `Assets/map/script/Stage2/`. 씬·프리팹 미수정(.meta 는 Unity 임포트 시 생성).
**선행 조건:** `SaveLoadManager.SetGimmick/GetGimmick`(Phase 23) 병합 전에는 `Gimmick2Store` 에서 컴파일 오류. 호출은 `Gimmick2Store` 한 곳뿐.
공통: `IPlayerInteractable`(콜라이더 필요, PlayerInteraction 반경 2), `event Action<bool> Changed`는 값이 바뀔 때만 발생. 저장된 초기 상태는 Start 에서 적용(이벤트 없음, 프로퍼티로 조회).

| 컴포넌트 | 공개 프로퍼티 / 이벤트 / 메서드 | 키 | Inspector 연결 · 필수 구성 |
|---|---|---|---|
| LanternSwitch | IsOn, Locked, `Toggled(bool)`, SetOn | (휘발) | onVisual, offVisual; 콜라이더 |
| LanternPuzzle | Solved, `Changed(bool)` | s2.a.lantern.solved | lanterns[3], answer[3] (정답 패턴); 해결 시 석등 정답 고정·잠금 |
| StoneTabletHint | Unlocked, IsOpen, `Changed(bool)`(=Unlocked), Open/Close | s2.a.tablet.unlocked | areaEnemies(HP[]), hintPanel(기본 비활성 무방), 닫기 버튼→Close(); Pause 입력으로도 닫힘; 콜라이더 |
| Telescope | Viewed, IsViewing, `Changed(bool)`(=Viewed), Open/Close | s2.a.telescope.viewed | viewPanel(정적 이미지), 닫기 버튼→Close(); 콜라이더 |
| TreeMonsterTransform | Transformed, Timer, `Changed(bool)` | s2.a.tree.transformed | isTrigger Collider2D(자신), treeObject, monsterObject(변신 후 몬스터, 기본 비활성), playerTag="Player", dwell 2s |
| WaterwayLever | IsChanged(true=B), `Changed(bool)`, Set | s2.b.waterway.changed | waterwayA, waterwayB, leverDefault/ChangedVisual; 콜라이더 |
| FloodgateWaterfall | Stopped, `Changed(bool)`, Stop | s2.b.waterfall.stopped | waterfalls[], gateClosed/OpenVisual; 콜라이더 |
| WaterWheel | Running, `Changed(bool)`, Set | s2.b.waterwheel.running | wheelVisual, lift(RisingLift, 선택); 콜라이더 |
| RisingLift | Raised, `Changed(bool)`, SetRaised(raised, fire) | s2.b.lift.raised | platform, lowPoint, highPoint(Transform), speed |

참고: 석등 개별 ON/OFF 는 저장 안 함(해결 플래그만 영구). 석판/망원경 UI 열림 중 GameStateManager Puzzle 상태로 전환. 가고일=잡몹 나무(별도 몬스터 클래스 없음). 25/26 세션에 위 이벤트명 통보함(응답 없음).
미병합/미검증: Phase 23 병합, Unity 컴파일, Play 동작.

## 추가(타 세션 계약 반영)
- StoneTabletHint: Inspector `unlockedKey`(기본 s2.a.tablet.unlocked), `readKey`(기본 s2.a.tablet.read, 열 때 true 저장), `requiredFlagKey`(비어있지 않으면 해당 플래그 true 일 때만 열림). 2-2 석판 인스턴스는 unlockedKey/readKey 를 s2.b.tablet.* 로, requiredFlagKey=s2.b.waterfall.stopped 로 설정(Stage22Progress.CanAccessTablet 과 등가). 2-1 은 기본값, 필요 시 requiredFlagKey 지정.
- Phase 23 병합 완료(b43dd2c). Stage2Events / Stage21Progress / Stage22Progress 는 직접 호출하지 않고 플래그 기반으로만 연동.
