# Phase 1: 유니티 에디터 구성 및 검증 가이드

이 문서는 Phase 1(보스 코어 메커니즘) 구현 사항을 유니티 에디터에서 활성화하고 테스트하기 위한 상세 지침을 제공합니다.

---

## 1. 자동화 도구 사용 (권장)

개발 생산성을 위해 두 가지 에디터 스크립트가 준비되어 있습니다. 유니티 상단 메뉴의 **Tools** 항목을 확인하세요.

### A. 자산 생성 (`Tools > Build WaterMonster Assets`)
*   **기능**: 물괴물 전용 애니메이터 컨트롤러(`WaterMonster.controller`)와 필요한 프리팹(`HealPopup`, `WaterSpitProjectile`)을 자동으로 생성합니다.
*   **결과물 위치**: `Assets/Enemy/WaterMonster/Resources/` 폴더 내에 생성됩니다.

### B. 씬 배치 (`Tools > Place WaterMonster in InGame Scene`)
*   **기능**: `InGame` 씬을 자동으로 열고, 보스 오브젝트를 생성한 뒤 필요한 컴포넌트(`WaterMonsterController`, `WaterMonsterStats`)를 부착하고 초기화합니다.
*   **주의**: 기존에 배치된 보스가 있다면 삭제 후 새로 배치하거나, 기존 오브젝트의 컴포넌트를 갱신합니다.

---

## 2. 수동 설정 및 체크리스트

자동화 도구 사용 후 또는 커스텀 배치를 원할 경우 다음 사항을 반드시 확인해야 합니다.

### A. 태그 및 레이어 설정
*   **Tag**: 보스 오브젝트(또는 자식 히트박스)의 태그가 `HitBox`로 설정되어 있어야 합니다. (플레이어의 `WaveSlice` 타격 감지용)
*   **Layer**: 보스는 `Enemy` 또는 `Boss` 레이어에 할당되어야 하며, 플레이어의 공격 레이어 마스크와 일치해야 합니다.

### B. WaterMonsterStats 컴포넌트
*   **Max Health**: 보스의 최대 체력을 설정합니다. (예: 500)
*   **Water Heal Multiplier**: 1.0으로 설정 시, 플레이어의 데미지 수치만큼 HP가 회복됩니다.
*   **Max Water**: 반드시 **0**으로 설정되어야 합니다. (Phase 1에서는 베리어 시스템을 사용하지 않음)

### C. 플레이어 스킬 속성 설정
*   `Assets/Player/Prefabs/` 또는 씬 내의 플레이어 오브젝트에서 다음 스킬의 `Damage Element` 필드를 확인하세요.
    *   **WaveSlice**: `Water`로 설정되어야 함 (보스를 치료함).
    *   **FlashSlice / 기본 공격**: `None`으로 설정되어야 함 (보스에게 데미지를 입힘).

---

## 3. 기능 검증 (Play Mode)

플레이 모드 진입 후 다음 시나리오를 통해 구현을 확정합니다.

| 검증 항목 | 기대 결과 | 확인 방법 |
|:---|:---|:---|
| **물 속성 힐링** | `WaveSlice` 타격 시 보스의 HP가 상승함 | 인스펙터의 `Current Health` 수치 증가 및 초록색 숫자 팝업 확인 |
| **일반 데미지** | `FlashSlice`나 평타 타격 시 보스 HP가 감소함 | 보스의 HP 바 또는 인스펙터 수치 감소 확인 |
| **HP 코스트 공격** | 보스가 공격(근접/원거리) 시 자신의 HP가 소모됨 | 공격 모션 발생 시 HP가 미세하게 줄어드는지 확인 (최소 1 HP 유지) |
| **패턴 전환** | 플레이어와의 거리에 따라 공격 방식이 바뀜 | 근접 시 `Swipe`, 원거리 시 `Spit` 투사체 발사 확인 |

---

## 4. 트러블슈팅

*   **타격 시 아무 반응이 없음**: 보스 오브젝트에 `BoxCollider2D`(또는 `CircleCollider2D`)가 있는지, 그리고 `isTrigger` 설정이 플레이어 스킬의 감지 로직과 맞는지 확인하세요.
*   **물 공격에도 데미지가 들어감**: `WaveSlice` 스크립트의 `DamageElement`가 `Water`로 할당되어 있는지, 보스의 스탯이 `WaterMonsterStats`인지 확인하세요.
*   **그로기 상태로 빠짐**: 보스의 `MaxWater`가 0이 아니거나, `WaterMonsterController`가 아닌 일반 `BossController`가 부착되어 있는지 확인하세요.
