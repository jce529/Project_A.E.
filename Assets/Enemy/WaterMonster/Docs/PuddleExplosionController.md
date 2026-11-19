# PuddleExplosionController 설명서

**파일:** `Assets/Enemy/WaterMonster/Script/Phase3/PuddleExplosionController.cs`
**네임스페이스:** 없음 (global) — `WaterMonster.Phase2`의 `PuddleStackManager`/`PuddlePool`을 참조

## 현재 상태

코드는 완성되어 있지만 **씬/프리팹 어디에도 부착되어 있지 않아 지금은 동작하지 않는다**
(Phase 13 감사 A-D07-04). 필요한 GameObject에 붙이기만 하면 그대로 쓸 수 있는 코드이며,
삭제 대상이 아니라 미사용 상태로 방치된 것으로 판단해 문서화로 처리한다.

## 하는 일

`PuddleStackManager`(파괴 불가 웅덩이 개수)와 `PuddlePool`(전체 활성 웅덩이 개수)의
임계치 도달 이벤트를 구독해서, 웅덩이들을 경고색으로 물들인 뒤(`warningDuration`) 일제히
폭발시켜 반경 내 `Player` 레이어에 데미지를 주는 연출/전투 로직이다.

- `enableIndestructibleExplosion` + `indestructibleThreshold`(기본 5) → 파괴 불가 웅덩이가
  이 수에 도달하면 `PuddleStackManager.OnThresholdReached` 발생 시 폭발
- `enableTotalExplosion` + `totalThreshold`(기본 8) → 전체 활성 웅덩이가 이 수에 도달하면
  `PuddlePool.OnTotalThresholdReached` 발생 시 폭발

## 활성화 방법 (씬에 배치하는 법)

이 컴포넌트 하나만 붙여서는 동작하지 않는다. 아래 세 컴포넌트가 **모두** 씬에 존재해야 한다
(셋 다 `Awake()`에서 `Instance`를 등록하는 싱글턴이고, 자동 생성 코드가 없다):

1. 씬에 빈 GameObject(예: `PuddleManagers`)를 만들고 `PuddleStackManager`, `PuddlePool`
   (둘 다 `Assets/Enemy/WaterMonster/Script/Phase2/`)을 부착한다.
2. 같은 GameObject(또는 다른 아무 GameObject)에 `PuddleExplosionController`를 부착한다.
3. Inspector에서 필요 시 임계치/폭발 반경/데미지/경고색을 조정한다.

## 의존성 체크리스트

| 항목 | 필요 이유 |
|---|---|
| `PuddleStackManager` 씬 배치 | `Instance`가 null이 아니어야 파괴 불가 웅덩이 이벤트 구독됨 |
| `PuddlePool` 씬 배치 | `Instance`가 null이 아니어야 전체 폭발 이벤트 구독됨 |
| `WaterPuddle` 프리팹 (`Assets/Enemy/WaterMonster/Resources/WaterPuddle.prefab`) | 폭발 대상 웅덩이 오브젝트, `SpriteRenderer` 보유 필요(경고색 적용) |
| `Player` 레이어 | `ApplyExplosionDamage`의 `Physics2D.OverlapCircleAll` 대상 레이어 (프로젝트에 이미 존재) |
| 플레이어 GameObject의 `PlayerStats` 컴포넌트 | `TakeDamage(explosionDamage)` 호출 대상 |

## 주의사항

- `PuddleStackManager`/`PuddlePool`이 씬에 없으면 `Start()`/`OnEnable()`의 null 체크 때문에
  **에러 없이 조용히 아무 것도 구독하지 않는다** — 붙여놓고도 반응이 없다면 이 둘의 배치 여부부터 확인.
- `OnEnable()`과 `Start()`에서 동일한 구독 로직이 중복 실행된다(주석상 "Awake 직후 Instance가
  없었을 수 있어 재구독"). 두 매니저가 이미 씬에 있는 상태라면 중복 구독으로 이벤트 핸들러가
  두 번 불릴 수 있으니, 실제로 붙일 때 `OnThresholdReached`/`OnTotalThresholdReached` 이벤트가
  두 번 발화되지 않는지 Play 모드에서 확인 필요.
