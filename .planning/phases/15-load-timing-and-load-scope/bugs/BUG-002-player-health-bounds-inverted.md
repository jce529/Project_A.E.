# BUG-002: Player 최대 체력과 성장 상한 역전

- 심각도: 중간
- 상태: 해결됨
- 발견일: 2026-09-10
- 영향 범위: 플레이어 체력 성장, 세이브/로드 체력 복원

## 현상

`Player` 프리팹은 `maxHealth = 400`, `maxTotalHealth = 200`으로 저장되어 있다. 이름과 `AddHealth()` 구현에 따르면 `maxTotalHealth`는 `maxHealth`가 성장할 수 있는 상한이지만 현재 시작 최대 체력보다 작다.

이 구성에서는 `AddHealth()`의 `maxHealth < maxTotalHealth` 조건이 처음부터 거짓이므로 최대 체력 증가가 실행되지 않는다.

## 근거

- `Assets/Player.prefab:393` — `maxHealth: 400`
- `Assets/Player.prefab:397` — `maxTotalHealth: 200`
- `Assets/Player/Script/PlayerStats.cs:40` — `if (maxHealth < maxTotalHealth)`일 때만 성장
- `Assets/Player/Script/PlayerStats.cs:72-74` — 저장값 복원 시 상한, 최대 체력, 현재 체력을 그대로 적용

값의 역전과 성장 차단은 현재 구현으로 확인된다. 어느 값이 잘못됐는지는 기획 근거가 없어 확정할 수 없다.

## 재현 절차

1. `Player` 프리팹을 사용하는 씬에서 Play 모드에 진입한다.
2. `PlayerStats.AddHealth()`가 호출되는 성장 아이템 또는 테스트 경로를 실행한다.
3. 호출 전후 `MaxHealth`, `MaxTotalHealth`, 현재 체력을 비교한다.

## 기대 결과

- `maxHealth`가 성장 상한보다 낮을 때 증가한다.
- `maxHealth`는 `maxTotalHealth`를 초과하지 않는다.
- 저장 후 로드해도 같은 불변식이 유지된다.

## 실제 결과

- 최신 `Assets/Player.prefab`은 `maxHealth=100`, `maxTotalHealth=200`이다.
- `RestoreStats()`는 저장된 최대 체력을 `1..maxTotalHealth` 범위로 보정하고 현재 체력을 보정된 최대치로 복원한다.
- Unity MCP에서 초기값 `100/100/200`, 성장 후 `101/101/200`, 역전 저장값 `400/200` 로드 후 `200/200/200`을 확인했다.

## 결정 필요 사항

- 의도한 시작 최대 체력이 400이라면 `maxTotalHealth`를 400 이상으로 올릴지 결정한다.
- 의도한 성장 상한이 200이라면 `maxHealth`를 200 이하로 낮출지 결정한다.
- 기존 세이브 파일에 이미 역전된 값이 들어 있을 때 로드 시 보정할지도 결정한다.

## 완료 조건

- [x] 기획상 시작 최대 체력 100과 최종 성장 상한 200을 확정했다.
- [x] 프리팹에서 `0 < health <= maxHealth <= maxTotalHealth`를 만족한다.
- [x] `AddHealth()`가 상한 전에 증가함을 Unity MCP로 확인했다.
- [x] 역전된 기존 저장 데이터를 상한 기준으로 보정함을 확인했다.
- [x] 검증 일자와 현재 커밋 상태를 아래에 기록했다.

## 해결 기록

- 검증 일자: 2026-09-11
- 수정 커밋: `5b037f8` (프리팹 시작값), `e81edbf` (체크포인트 회복·로드 불변식)
- 비고: Phase 15 UAT 테스트 5 통과. `Player.prefab` 100/200 및 `PlayerStats.RestoreStats()` 불변식 보정 재확인.

## Phase 15 이관 (2026-09-10)

이 항목은 단독 수정 대신 **Phase 15 「로드 시점 및 로드 범위 정의」**에서 다룬다.
`maxHealth`/`maxTotalHealth` 값 확정과 "로드 시 저장된 값을 어디까지 신뢰할지"가
같은 결정이기 때문이다.

### 추가 확인된 사실

- `Assets/Scenes/Tutorial Map.unity` — Player 인스턴스가 `maxHealth`를 **100**으로 오버라이드한다.
  이 씬은 `MainMenuUI.newGameSceneName`이 가리키는 실제 새 게임 진입점이므로,
  튜토리얼에서는 `maxHealth(100) < maxTotalHealth(200)`이 성립해 성장이 정상 동작한다.
- 오버라이드가 있는 씬은 `Tutorial Map`과 `_Recovery/*` 뿐이다. 나머지 **9개 스테이지 씬은
  프리팹 기본값 400을 그대로** 사용하므로 그 씬들에서만 성장이 차단된다.
- `Assets/ImportedAsset/HealthHeartSystem/Scenes/ExampleScene.unity:830-832` — 참조 구현도
  `maxHealth(3) < maxTotalHealth(30)` 패턴이다.
- `Assets/Player/Script/UI/PlayerHealthUI.cs:56` — 하트 개수를 `MaxTotalHealth / healthPerHeart`로
  생성한다. 따라서 상한을 400으로 올리는 선택지는 하트를 80개 생성해 UI 레이아웃에 영향을 준다.
- `Assets/SaveSystem/Script/SaveLoadManager.cs:384` → `PlayerStats.RestoreStats()`는 저장값 3개를
  **불변식 검사 없이 그대로 대입**한다. 역전된 세이브 파일이 있으면 로드가 그 상태를 그대로 되살린다.

### Phase 15에서 결정할 사항

- 시작 최대 체력과 성장 상한의 확정값 (프리팹 기본값 + 9개 스테이지 씬 처리 방침).
- `RestoreStats()`에서 `0 < health <= maxHealth <= maxTotalHealth`를 강제할지, 강제한다면
  역전된 기존 세이브를 어느 값 기준으로 보정할지.

### 결정 완료 (2026-09-10, Phase 15 discuss)

- **시작 `maxHealth` 100 / 성장 상한 `maxTotalHealth` 200** 으로 확정 (15-CONTEXT D-09).
- 로드 시 `0 < health <= maxHealth <= maxTotalHealth` **강제 보정**하기로 확정 (15-CONTEXT D-08).
- 구현은 Phase 15에서 수행한다. 상세는
  `.planning/phases/15-load-timing-and-load-scope/15-CONTEXT.md` 참조.
