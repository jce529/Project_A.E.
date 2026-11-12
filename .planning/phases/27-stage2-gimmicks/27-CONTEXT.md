# Phase 27 CONTEXT — 2스테이지 기믹 구현

discuss 생략(사용자 질문 없음). 아래는 계획 가정(Assumptions)이다.

## Assumptions
- A1. 상호작용은 `IPlayerInteractable`(CanInteract/Interact) 구현 + 트리거/일반 콜라이더. `PlayerInteraction` 이 반경 2 내 최근접 대상을 고른다.
- A2. 영구 상태는 `SaveLoadManager.Instance.SetGimmick(key,bool)` / `GetGimmick(key,false)` (Phase 23 계약). 이 worktree 에는 아직 미병합이므로 `Gimmick2Store` 한 곳에서만 호출하고, 병합 전에는 컴파일 오류 가능(23 병합으로 해소).
- A3. 키: 2-1 구역 `s2.a.*`, 2-2 구역 `s2.b.*`. 소문자.
- A4. 석등/석판/망원경/나무 = 2-1(a), 물레방아·상승장치/레버·물길/수문·폭포 = 2-2(b).
- A5. 각 컴포넌트는 `event Action<bool> Changed` + 상태 프로퍼티를 공개(상태 머신이 구독/폴링). 이벤트는 값이 실제로 바뀔 때만 발생. 로드된 초기 상태는 Start 에서 적용하되 이벤트 없이 프로퍼티로만 노출.
- A6. 적 전멸 판정은 `HP.OnDeath`(Assets/Script/HP.cs) 구독. 파괴된 오브젝트(null)는 사망으로 간주.
- A7. 가고일 = 잡몹 나무. 별도 몬스터 클래스 없이 `TreeMonsterTransform` 이 변신 트리거만 담당한다(변신 시 `treeObject` 비활성 / `monsterObject` 활성).
- A8. 씬·프리팹은 수정하지 않고 SUMMARY 에 Inspector 연결 목록을 남긴다. UI 는 Inspector 연결 GameObject 의 SetActive 로 표시.
- A9. 퍼즐 UI 열림 중 입력 잠금은 `GameStateManager.SetState(Puzzle)` / `Playing` 사용(있을 때만).
- A10. 석등 개별 ON/OFF 는 퍼즐 미해결 동안 휘발(저장 안 함), 해결 플래그만 영구. 해결 후 석등은 정답 패턴으로 고정.

## Plans / Waves
- Wave 1: 27-01 (Gimmick2Store 기반)
- Wave 2 (병렬): 27-02 석등, 27-03 석판·망원경, 27-04 나무 변신, 27-05 레버·수문, 27-06 물레방아·상승장치
