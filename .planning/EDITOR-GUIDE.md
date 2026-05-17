# 보스 제작 및 에디터 종합 가이드

이 문서는 프로젝트 A.E의 모든 보스 구현 사항을 유니티 에디터에서 설정하고 테스트하기 위한 종합 가이드입니다.

---

# 1. 물괴물(Water Monster) 보스 [v1.0]

최종보스 '물괴물'의 모든 페이즈 구현 사항을 설정합니다.

## 1.1 페이즈별 자동화 도구 (Tools 메뉴)

유니티 상단 메뉴의 **Tools** 항목을 통해 수동 설정 없이 대부분의 환경을 구축할 수 있습니다.

*   **Build WaterMonster Assets**: 보스 전용 애니메이터와 투사체 프리팹을 생성합니다.
*   **Place WaterMonster in Scene**: `InGame` 씬에 보스를 배치하고, 기본 체력(500) 및 컴포넌트를 자동 연결합니다.
*   **Phase2 > Build WaterPuddle Prefab**: `WaterPuddle` 태그 등록 및 프리팹 생성.
*   **Phase2 > Place Phase2 Objects in Scene**: 씬에 날씨 시스템 및 매니저 자동 배치.

## 1.2 주요 컴포넌트 설정 (Inspector)

| 컴포넌트 | 주요 필드 | 설명 |
|:---|:---|:---|
| **WaterMonsterStats** | `Max Health` | 보스의 전체 체력 (Phase 2 트리거는 70% 지점) |
| **PuddleSpawner** | `Spawn Interval` | 웅덩이가 생성되는 간격 (초 단위) |
| **PuddleStackManager** | `Explosion Threshold` | 폭발이 일어날 흡수 웅덩이 개수 |

---

# 2. 물의 정령(Water Spirit) 보스 [v2.0]

2스테이지 구조의 '물의 정령' 보스 설정 가이드입니다.

## 2.1 프리팹 구성 (Prefabs)

물의 정령 보스는 **진짜 보스**와 **분신(Dummy)** 두 종류의 프리팹이 필요합니다.

### A. 진짜 보스 프리팹 (WaterSpirit)
1.  새 빈 오브젝트를 생성하고 이름을 `WaterSpirit`으로 설정합니다.
2.  `SpiritController`와 `SpiritStats` 컴포넌트를 부착합니다.
3.  `Rigidbody2D` (Gravity Scale: 0, Collision Detection: Continuous)와 `CapsuleCollider2D`를 추가합니다.
4.  **Layer**: `Enemy` 레이어로 설정합니다.

### B. 분신(Dummy) 프리팹 (WaterSpirit_Dummy)
1.  위의 `WaterSpirit` 프리팹을 복제하여 이름을 `WaterSpirit_Dummy`로 변경합니다.
2.  이 프리팹은 `SpiritController.DummyPrefab` 필드에 할당될 대상입니다.
3.  별도의 설정 변경 없이 `SpiritStats.IsDummy` 플래그는 런타임에 자동으로 true로 설정됩니다.

## 2.2 주요 컴포넌트 상세 설정 (Inspector)

### SpiritController (보스 제어)
| 필드 | 권장값 | 설명 |
|:---|:---|:---|
| **Projectile Prefab** | (프리팹 할당) | S1-02 투사체 패턴에 사용될 프리팹 |
| **Dummy Prefab** | `WaterSpirit_Dummy` | 스테이지 2에서 소환될 분신 프리팹 |
| **Repel Range** | 2.5 | 플레이어를 밀쳐내는 근접 거리 임계치 |
| **Charge Range** | 8.0 | 돌진 공격을 시도하는 중거리 임계치 |
| **Stealth Duration** | 0.5 | 은신 후 재등장까지 걸리는 시간 (초) |
| **Min/Max Teleport Radius** | 3.0 / 6.0 | 플레이어 주변 어느 범위로 순간이동할지 결정 |

### SpiritStats (능력치 및 트리거)
*   **Max Health**: 보스의 전체 체력 설정 (예: 1000).
*   **HP 50% 트리거**: 체력이 50% 이하로 떨어지면 자동으로 스테이지 2(분신 소환 및 은신 패턴 추가)로 전환됩니다.

## 2.3 스테이지 2 사이클 흐름

1.  **진입**: 보스 HP 50% 도달 → 보스 주변에 분신 2개 즉시 생성.
2.  **일반 패턴**: 진짜와 분신이 각자 거리 기반 패턴을 3회 수행.
3.  **헤비 콤보**: 3회 패턴 후, 3체 모두 **동시 은신** → 플레이어 주변 텔레포트 → **동시 돌진**.
4.  **그로기**: 헤비 콤보 종료 후 분신은 사라지고 진짜 보스는 5초간 **그로기** 상태 진입.
5.  **반복**: 그로기 해제 후 다시 분신 소환과 함께 1번부터 사이클 반복.

---

# 3. 플레이 테스트 체크리스트

1.  **분신 데미지**: 분신을 공격했을 때 데미지 텍스트가 뜨지 않거나 HP가 줄어들지 않는가?
2.  **은신 무적**: 은신(사라진) 동안에는 보스 콜라이더가 꺼져서 플레이어 공격이 통과하는가?
3.  **그로기 복구**: 그로기 상태가 끝난 후 정확히 스테이지 2 상태로 복귀하여 분신을 다시 소환하는가?
