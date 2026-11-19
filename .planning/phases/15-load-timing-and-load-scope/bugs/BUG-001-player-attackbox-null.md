# BUG-001: Player 기본 공격 프리팹 참조 누락

- 심각도: 낮음 (최초 기록 높음 → 2026-09-10 재평가, 아래 추가 조사 참고)
- 상태: 수정됨 — 재검증 필요
- 발견일: 2026-09-10
- 영향 범위: 플레이어 기본 공격

## 현상

`Player` 프리팹의 `PlayerAttack.attackBox`가 null로 직렬화되어 있다. `OnBasicAttack()`은 별도 null 가드 없이 이 값을 `Instantiate`에 전달한다.

기본 공격 입력이 `OnBasicAttack()`까지 도달하면 공격 판정 오브젝트를 생성하지 못하고 런타임 예외가 발생할 가능성이 높다.

## 근거

- `Assets/Player.prefab:371` — `attackBox: {fileID: 0}`
- `Assets/Player/Script/PlayerAttack.cs:13` — `public GameObject attackBox;`
- `Assets/Player/Script/PlayerAttack.cs:85` — `Instantiate(attackBox, ...)`

현재 소스와 프리팹 직렬화 값으로 참조 누락은 확인됐다. 실제 입력 시 Console 예외와 이후 콤보 상태 변화는 Play 모드 확인이 필요하다.

## 재현 절차

1. `Player` 프리팹을 사용하는 플레이 씬을 연다.
2. Play 모드에 진입한다.
3. 기본 공격 입력을 한 번 수행한다.
4. 공격 판정 오브젝트 생성 여부와 Console 예외를 확인한다.

## 기대 결과

- 기본 공격 위치에 유효한 공격 판정 프리팹이 생성된다.
- 공격 지속시간 후 판정 오브젝트가 제거된다.
- Console에 null 프리팹 관련 예외가 없다.

## 실제 결과

- 최신 프리팹 직렬화 상태에서 `attackBox`는 `Assets/Player/Resourcess/AttackBox.prefab`을 참조한다.
- 기본 공격 Play 모드 실제 결과는 아직 미검증이다.

## 수정 방향

- 현재 공격 시스템이 기대하는 `PlayerAttackDamager` 포함 프리팹을 확인해 `Player.attackBox`에 연결한다.
- 참조가 필수 구성이라면 `Awake` 또는 `OnValidate`에서 명확한 오류를 출력하는 방어 검사를 추가하는 방안도 검토한다.
- 삭제된 구형 `AttackBox.cs`를 근거 없이 복원하지 않는다. 현재 소비자는 `PlayerAttackDamager`를 찾는다.

## 완료 조건

- [x] `Assets/Player.prefab`의 `attackBox`가 유효한 프리팹을 참조한다.
- [ ] 기본 공격 1~3타가 모두 판정을 생성한다.
- [ ] 좌우 방향 전환 시 공격 방향이 정상이다.
- [ ] 기본 공격 관련 Console 예외가 없다.
- [ ] 검증 일자와 수정 커밋을 아래에 기록한다.

## 해결 기록

- 검증 일자: 2026-09-10 (정적 검증). Play 모드 실측 미수행
- 수정 커밋: `5b037f8`
- 비고: `Assets/Player.prefab:371`의 `attackBox`를 `Assets/Player/Resourcess/AttackBox.prefab` 루트(fileID `2283162787729905416`, guid `709704cd680fc3b4ab4d0b240ce3406d`)에 연결. 씬 12개의 기존 오버라이드와 동일 값이므로 씬은 무수정. 남은 완료 조건 3건(1~3타 판정, 방향 전환, Console 예외)은 Play 모드 항목으로 미검증.

## 추가 조사 (2026-09-10)

프리팹 에셋의 `attackBox`는 실제로 null이지만, **씬에 배치된 Player 인스턴스는 전부 프리팹 오버라이드로
값을 채우고 있다.** 따라서 현재 플레이 경로에서 기본 공격이 깨지지는 않는다.

- `Assets/Scenes/Tutorial Map.unity:434801` — `propertyPath: attackBox` /
  `objectReference: {guid: 709704cd680fc3b4ab4d0b240ce3406d}` = `Assets/Player/Resourcess/AttackBox.prefab`
- 동일한 오버라이드가 `1 stage`, `2 stage`, `3 stage main`, `3 stage extra/*` 8종, `_Recovery/*` 까지
  Player 프리팹이 놓인 **모든 씬**에 존재한다 (총 12개 씬).
- `Assets/Player/Resourcess/AttackBox.prefab` 은 `PlayerAttackDamager`(guid `a78d7c25b94f9ae4cb4fbff6d826ebdc`)를
  포함하므로 `PlayerAttack.cs:88`의 `GetComponent<PlayerAttackDamager>()` 소비자와 일치한다.
- `Player.prefab`(guid `ae14109d864ad904cb8f45a57e5fa1bd`)은 씬 배치 외에 참조되지 않는다. 스크립트의
  `Resources.Load` 호출 3건은 InputActions / WaterSpitProjectile / HealPopup 이며 플레이어 프리팹을
  런타임에 생성하는 경로는 없다.

### 재평가

- 심각도: 높음 → **낮음 (잠재 위험)**. 런타임 차단 요인이 아니다.
- 남은 위험: 프리팹 에셋 기본값이 null이라 **새 씬에 Player를 새로 배치하면 그 인스턴스만 조용히 깨진다.**
  또한 12개 씬이 같은 값을 각자 오버라이드로 들고 있어 프리팹 단일 소스 원칙에 어긋난다.
- 권장 조치: `Assets/Player.prefab`의 `attackBox`에 `AttackBox.prefab`을 직접 연결해 기본값을 세우고,
  씬별 중복 오버라이드는 그대로 두어도 무해하다 (동일 값). 씬 12개를 건드리지 않는 최소 변경이다.

## 최신 감사 (2026-09-11)

- `Assets/Player.prefab:371`에서 유효한 AttackBox 프리팹 참조를 재확인했다.
- 코드 수정은 유지되고 있으나 1~3타, 방향 전환, Console 무예외 조건의 Play 모드 증거가 없어 상태를 표준 단계인 `수정됨 — 재검증 필요`로 정정했다.
