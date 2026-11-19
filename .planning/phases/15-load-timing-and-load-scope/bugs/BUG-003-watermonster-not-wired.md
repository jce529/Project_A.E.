# BUG-003: WaterMonsterController 씬/프리팹 배치 없음

- 심각도: 높음
- 상태: 확인됨 — 미해결
- 발견일: 2026-09-10
- 영향 범위: WaterMonster 보스 콘텐츠 전체

## 현상

`WaterMonsterController`와 관련 상태/공격 코드는 존재하지만, 현재 저장소의 `.unity` 또는 `.prefab` 자산 어디에도 컨트롤러 스크립트 GUID가 직렬화되어 있지 않다. 런타임에서 이 컴포넌트를 `AddComponent`로 생성하는 코드도 찾지 못했다.

따라서 현재 체크아웃만 기준으로 보면 WaterMonster 보스 구현은 실제 씬 또는 프리팹 인스턴스로 진입할 수 없다.

## 근거

- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs.meta` — GUID `c30f08a94007d4d498944914904f4327`
- `Assets/**/*.unity`, `Assets/**/*.prefab` — 위 GUID 참조 0건
- `Assets/**/*.cs` — `WaterMonsterController` 런타임 생성 경로 없음
- 관련 상태 코드의 타입 검사와 참조는 존재하지만 컨트롤러 인스턴스를 생성하지는 않는다.

자산 배치 부재는 확인됐다. 의도적으로 아직 배치하지 않은 개발 단계인지, 삭제 과정에서 연결이 사라졌는지는 별도 결정이 필요하다.

## 재현/확인 절차

1. WaterMonster가 등장해야 하는 대상 씬을 확정한다.
2. 해당 씬 Hierarchy와 연결된 프리팹을 확인한다.
3. `WaterMonsterController`, `WaterMonsterStats` 및 필수 상태/공격 참조가 있는지 확인한다.
4. Play 모드에서 보스 진입과 상태머신 시작 여부를 확인한다.

## 기대 결과

- 대상 씬에 WaterMonster 프리팹 또는 인스턴스가 존재한다.
- `WaterMonsterController`와 필수 컴포넌트/프리팹 참조가 직렬화되어 있다.
- 전투 진입 시 상태머신이 정상적으로 시작된다.

## 실제 결과

- 현재 저장소의 씬/프리팹에는 `WaterMonsterController` 참조가 없다.
- Play 모드 대상 씬 확인: 미검증.

## 수정 방향

- 먼저 실제 배치 대상 씬과 사용할 보스 프리팹을 확정한다.
- 기존 에디터 셋업 도구는 최신 정리 커밋에서 삭제되었으므로 무조건 복원하지 말고, 현재 컴포넌트 계약에 맞춰 수동 배치하거나 새 셋업 흐름을 만든다.
- 배치 후 컨트롤러가 요구하는 Stats 타입과 공격 프리팹 참조를 Inspector에서 검증한다.

## 완료 조건

- [ ] WaterMonster의 공식 대상 씬과 프리팹을 확정한다.
- [ ] 씬 또는 프리팹에 `WaterMonsterController` GUID 참조가 존재한다.
- [ ] 필수 직렬화 필드에 null 참조가 없다.
- [ ] Play 모드에서 전투 진입, 공격 선택, 페이즈 전환, 사망 저장을 검증한다.
- [ ] 검증 일자와 수정 커밋을 아래에 기록한다.

## 해결 기록

- 검증 일자:
- 수정 커밋:
- 비고:

## Phase 15 이관 (2026-09-10)

이 항목 중 **"격파한 보스가 로드 후에도 격파 상태로 남는가"** 는
**Phase 15 「로드 시점 및 로드 범위 정의」**에서 다룬다. 보스를 씬에 배치하는 순간
바로 따라오는 질문이기 때문이다.

### 추가 확인된 사실

- 보스 **프리팹 자체가 저장소에 없다**. `Assets/Enemy/WaterMonster/Resources/`에는
  간헐천·투사체·웅덩이 등 부속 프리팹만 있고 보스 본체가 없다.
- `Assets/Enemy/WaterMonster/Script/WaterMonsterController.cs` 는 프리팹 참조 7개
  (`_geyserEffectPrefab`, `_prisonProjectilePrefab`, `_colorPrisonPrefab`, `_wavePrefab`,
  `_speedUpZonePrefab`, `_slowDownZonePrefab`, `WeatherController`)와
  **씬 소속 `BoxCollider2D _battleAreaBounds`** 를 요구한다. 프리팹만으로는 완결되지 않는다.
- `Assets/Scenes/3stage/3 stage extra/3 stage boss.unity` 에는 `BossHealthBarController`가
  이미 배치돼 있으나 보스 오브젝트가 없고 `bossController` 참조가 비어 있다.
  → 대상 씬 후보로 가장 유력하다.
- `.planning/EDITOR-GUIDE.md`가 안내하는 `Place WaterMonster in Scene` 에디터 툴은 정리 커밋에서
  삭제됐고, 그 툴이 대상으로 삼던 `InGame` 씬도 현재 저장소에 없다.
- `SaveOnBossDefeated("WaterMonster")`와 `IsBossDefeated("WaterMonster")`가 모두
  `WaterMonsterStats.cs`에 구현되어 진행도 저장·복원 코드 경로는 갖춰졌다.

### 분리된 두 결정

1. (Phase 15) 로드 시 `BossProgress`를 어떻게 적용할지 — 씬의 보스를 비활성화할지, 스폰을 막을지.
2. (미정) WaterMonster 보스 프리팹 제작과 대상 씬 배치 자체. 규모상 별도 페이즈가 필요하다.

### 결정 완료 (2026-09-10, Phase 15 discuss)

- 로드 시 `BossProgress`를 읽어 **격파된 보스는 등장시키지 않는다**로 확정 (15-CONTEXT D-06).
- 판정 주체는 **보스 쪽** — 각 보스가 자기 ID로 `IsBossDefeated()`를 조회해 스스로 물러난다
  (15-CONTEXT D-07). 로드 경로가 보스 목록을 순회하지 않는다.
- 위 두 항목만 Phase 15 범위다. **보스 프리팹 제작과 씬 배치는 여전히 미결**이며 별도 페이즈가
  필요하다. 상세는 `.planning/phases/15-load-timing-and-load-scope/15-CONTEXT.md` 참조.

## 최신 감사 (2026-09-11)

- `WaterMonsterController` GUID의 씬·프리팹 참조는 여전히 0건이다.
- 보스 진행도 저장과 격파 상태 복원 코드는 구현됐지만 실제 보스 본체와 공식 대상 씬은 없다.
- 완료 조건의 핵심인 배치와 Play 모드 전투 검증이 충족되지 않아 `확인됨 — 미해결`을 유지한다.
