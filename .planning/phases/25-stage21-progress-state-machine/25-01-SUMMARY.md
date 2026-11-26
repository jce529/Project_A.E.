# 25-01 SUMMARY
구현(정적 점검만, Play/컴파일 미검증 — Phase 23 병합 후 SetGimmick 필요): `Assets/Stage2/Script/Stage21*.cs`

## s2.a.* 키
| 키 | 쓰는 쪽 | 의미 |
|---|---|---|
| s2.a.area{1-6}.visited | Stage21AreaTrigger | 구역 방문(선형 흐름) |
| s2.a.area{N}.cleared | AreaCleared 이벤트 | 구역 적 전멸 |
| s2.a.tablet.unlocked | 5번 전멸 시 | 석판 조사 가능 |
| s2.a.tablet.read | TabletOpened | 석판 열람 |
| s2.a.lantern.solved | Phase 27 석등 | 퍼즐 해결 → 2-2 허용 |
| s2.a.fog.removed | TryRemoveFog | 안개 영구 제거 |

## 인터페이스 (phase-27에 통보, 이름 불일치 시 25가 맞춤 — 회신 대기)
| 이름 | 방향 | 용도 |
|---|---|---|
| Stage2Events.RaiseLanternSolved() | 27→25 | 석등 해결 |
| Stage2Events.RaiseAreaCleared(int) | 전투→25 | 지정 구역 전멸 (발행자 미구현: 스텁 계약) |
| Stage2Events.RaiseTabletOpened() | 27→25 | 석판 열람 |
| Stage21Progress.CanInvestigateTablet() | 27이 조회 | 5번 전투 전 false |
| Stage21Progress.CanEnterStage22() | 조회 | 퍼즐 해결 여부 |
| Stage2Events.TabletUnlocked / FogRemoved | 25→구독 | 상태 변화 알림 |

## Inspector 연결 목록 (2 stage 씬, 직접 편집 안 함)
- 각 구역 트리거 콜라이더(1~6): Stage21AreaTrigger, area 번호 지정, 안개 해제 구역은 removesFog 체크
- 6번 출구 트리거: Stage21Stage22Gate — onAllowed→2-2 씬 전환, onBlocked→차단 안내/장벽
- 안개 오브젝트: Stage21FogController (fog 비우면 자기 자신)
- 5번 전투 전멸 시 Stage2Events.RaiseAreaCleared(5) 를 호출할 전투 담당 코드 필요(미구현)

## 남은 이슈
- Phase 23 브랜치 미병합(커밋 해시 수신 대기), .meta 파일은 에디터가 생성
- Success Criteria 3(재로드 후 안개 유지)은 Play 검증 필요

## Phase 27 확정 API 대응 (adfea8f 병합 후)
phase-27 은 Stage2Events 대신 자체 컴포넌트(LanternPuzzle/StoneTabletHint/Telescope/TreeMonsterTransform, Assets/map/script/Stage2/)가
플래그를 직접 기록하고 `Changed(bool)` 이벤트를 낸다. Phase 25 는 **플래그를 읽는 방식**이라 그대로 호환된다:
- `CanInvestigateTablet()` = `s2.a.tablet.unlocked` (27 의 StoneTabletHint 가 구역 전멸 시 기록)
- `CanEnterStage22()` / `TryRemoveFog()` = `s2.a.lantern.solved` (27 의 LanternPuzzle 이 기록)
- Stage2Events.RaiseLanternSolved/RaiseAreaCleared/RaiseTabletOpened 는 선택 경로(미사용 가능). 27 병합 후 정리 여부 결정.
- 27 전용 키(미사용): s2.a.telescope.viewed, s2.a.tree.transformed
