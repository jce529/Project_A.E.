# BUG-004: MainMenuUI 중복 컴포넌트 및 빈 참조

- 심각도: 낮음
- 상태: 수정됨 — 재검증 필요
- 발견일: 2026-09-10
- 영향 범위: 메인 메뉴 유지보수 및 향후 초기화 로직

## 현상

`Assets/Scenes/MainMenu.unity`에 `MainMenuUI` 컴포넌트가 두 개 직렬화되어 있다.

- `MenuController`의 인스턴스는 `loadGameButton`과 `slotSelectPanel`이 정상 연결되어 있고 버튼 이벤트의 대상이다.
- `Start_B` 버튼 GameObject에도 같은 컴포넌트가 붙어 있지만 두 참조가 모두 null이다.

현재 `MainMenuUI.Start()`는 null 참조를 건너뛰므로 이 중복 인스턴스만으로 즉시 예외가 발생하지는 않는다. 그러나 동일 초기화 로직이 두 번 실행되고, 향후 필수 초기화가 추가되면 버튼에 붙은 불완전한 인스턴스가 회귀 원인이 될 수 있다.

## 근거

- `Assets/Scenes/MainMenu.unity:3205` — `Start_B` GameObject의 `MainMenuUI`
- `Assets/Scenes/MainMenu.unity:3208-3210` — `loadGameButton`, `slotSelectPanel`이 null
- `Assets/Scenes/MainMenu.unity:4737` — `MenuController` GameObject의 `MainMenuUI`
- `Assets/Scenes/MainMenu.unity:4740-4742` — 주 인스턴스 참조가 정상 연결됨
- 시작/이어하기 버튼의 PersistentCall은 `MenuController`의 컴포넌트를 대상으로 한다.

중복 컴포넌트와 참조 상태는 씬 YAML로 확인됐다. 제거 후 실제 메뉴 동작은 Play 모드 검증이 필요하다.

## 재현/확인 절차

1. Unity에서 `MainMenu` 씬을 연다.
2. `Start_B`와 `MenuController`를 각각 선택한다.
3. 두 GameObject에 `MainMenuUI`가 붙어 있는지 확인한다.
4. `Start_B`의 불필요한 `MainMenuUI`를 제거한 뒤 씬을 저장한다.
5. Play 모드에서 새 게임과 이어하기 버튼을 확인한다.

## 기대 결과

- 메인 메뉴 흐름을 소유하는 `MainMenuUI`가 `MenuController`에 하나만 존재한다.
- `loadGameButton`과 `slotSelectPanel` 참조가 연결되어 있다.
- 시작/이어하기 버튼이 단일 컨트롤러를 호출한다.

## 실제 결과

- 최신 `MainMenu.unity`에는 `MainMenuUI` 스크립트 참조가 `MenuController`의 1건만 남아 있다.
- 남은 인스턴스의 `loadGameButton`과 `slotSelectPanel` 참조는 유효하다.
- 새 게임/이어하기 버튼의 Play 모드 실동작은 아직 미검증이다.

## 수정 방향

- `Start_B` 버튼에 붙은 중복 `MainMenuUI`만 제거한다.
- 버튼의 OnClick 대상인 `MenuController`와 해당 직렬화 참조는 유지한다.
- 제거 전후 씬 diff를 확인해 의도하지 않은 UI 직렬화 변경이 섞이지 않게 한다.

## 완료 조건

- [x] `MainMenu.unity`의 `MainMenuUI` GUID 참조가 1건이다.
- [x] 남은 인스턴스의 `loadGameButton`과 `slotSelectPanel`이 유효하다.
- [ ] 새 게임 버튼이 빈 슬롯 선택 규칙대로 동작한다.
- [ ] 이어하기 버튼과 슬롯 패널이 정상 동작한다.
- [ ] 검증 일자와 수정 커밋을 아래에 기록한다.

## 해결 기록

- 검증 일자: 2026-09-10 (정적 검증). Play 모드 실측 미수행
- 수정 커밋: `e81edbf`
- 비고: `Start_B`(GameObject `978357633`)의 중복 `MainMenuUI`(`978357638`)와 `m_Component` 항목을 제거해 씬 diff 17줄 삭제만 발생. 남은 인스턴스는 `MenuController`의 `1424678021` 1건이며 `loadGameButton`(`1900001002`), `slotSelectPanel`(`322991605`) 참조가 씬 내에 실재함을 확인. 두 버튼의 PersistentCall 대상도 `1424678021`로 변화 없음. 새 게임/이어하기 실동작은 Play 모드 항목으로 미검증.

## 최신 감사 (2026-09-11)

- `MainMenuUI` GUID 참조 1건을 재확인했다.
- 구조 수정은 완료됐으나 두 버튼의 Play 모드 완료 조건이 남아 있어 상태를 `수정됨 — 재검증 필요`로 정정했다.
