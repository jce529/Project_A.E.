# Item — Phase 16 아이템 코어(ItemData ScriptableObject) 검증 체크리스트

## 검증 대상 변경사항

| 파일 | 내용 |
|---|---|
| `Assets/Item/Script/IItem.cs` | Phase 16 이전 완료. `void UseEffect(PlayerInteraction player)`. **무수정** |
| `Assets/Item/Script/ItemData.cs` | 신규. 프로젝트 최초 ScriptableObject. `ItemData : ScriptableObject, IItem` + `ItemType`/`ConsumableEffectType` enum + `UseEffect` 본체 + 에디터 전용 ContextMenu 훅 |
| `Assets/Item/HealthPotion.asset` | 신규. 소모품 예시 — `health_potion_01` / Consumable / Heal / 20 |
| `Assets/Item/AncientKey.asset` | 신규. 진행아이템 예시 — `ancient_key_01` / Progression |

**16-01 이 고정한 guid** (`.asset` 이 에디터를 거치지 않고 손으로 작성됐기 때문에, guid 대조가 이 phase 검증의 핵심 포인트다):

| 파일 | guid |
|---|---|
| `Assets/Item/Script/ItemData.cs.meta` | `501b19c5008706d0a3f2bf69aacf52f6` |
| `Assets/Item/HealthPotion.asset.meta` | `1ca8a1cec6cae3595e071afccaf8623e` |
| `Assets/Item/AncientKey.asset.meta` | `65cc29b1f44b856171d66dbe8ddec240` |
| `Assets/Item/Check.md.meta` (이 플랜이 신규 할당) | `05511fb05942d6634e84eccc8a623d1f` |

## 정적 회귀 검사 결과

Unity 6000.3.10f1 배치모드 임포트(16-02 Task 1, 2026-09-20)로 실측한 결과다. Unity 종료 코드 `0`, 3개 grep 판정 전부 빈 출력(에러 0건), `git status --porcelain Assets/Item` 도 빈 출력 — 손으로 쓴 YAML 이 Unity 표준 출력과 완전히 일치해 재직렬화조차 일어나지 않았다.

| # | 항목 | 결과 |
|---|---|---|
| 1 | `ItemData : ScriptableObject, IItem` 존재 (D-01) | PASS |
| 2 | `[SerializeField]` 필드 정확히 4개, `displayName`/`icon`/`description` 0건 (D-06) | PASS |
| 3 | `ItemType` = `Consumable` / `Progression` 2값, 서브클래스 0건 (D-04) | PASS |
| 4 | `ConsumableEffectType` = `Heal` 1값 (D-03) | PASS |
| 5 | `UseEffect` 가 `player.GetComponent<PlayerStats>().Heal(amount)` 를 호출하고 널 가드가 0건 (D-02/D-03 + 프로젝트 관행) | PASS |
| 6 | `.asset` 2개의 `m_Script` guid 가 `ItemData.cs.meta` guid 와 일치 (D-07) | PASS |
| 7 | `Assets/Player/` 와 `Assets/Item/Script/IItem.cs` 0줄 변경 | PASS |
| 8 | Unity 6000.3.10f1 배치모드 임포트: 컴파일 에러 0 / Item 임포트 에러 0 / missing script 0 | PASS |

## 사전 준비

1. Unity 에디터로 프로젝트를 열고 컴파일 완료까지 대기. Console 에러 0건 확인.
2. Project 창에서 `Assets/Item` 폴더를 연다.
3. 플레이어가 배치된 씬(예: `Tutorial Map.unity`)을 연다.
4. `Assets/Item` 우클릭 → `Create` → `Items` → `Item Data` 메뉴가 존재하는지 확인한다(이 메뉴가 없으면 컴파일이 안 된 것).

## Play 모드 체크리스트

- [ ] 1. Project 창 `Assets/Item` 에 `HealthPotion`, `AncientKey` 두 에셋이 보이고, 아이콘이 깨진 스크립트(물음표)가 아니다.
- [ ] 2. `HealthPotion` 선택 시 Inspector 에 필드가 정확히 **4개**(`Id`, `Type`, `Effect Type`, `Amount`)만 보인다. 이름/아이콘/설명 필드가 없다 (D-06).
- [ ] 3. `HealthPotion` 값이 `Id = health_potion_01`, `Type = Consumable`, `Effect Type = Heal`, `Amount = 20` 이다.
- [ ] 4. `AncientKey` 값이 `Id = ancient_key_01`, `Type = Progression`, `Amount = 0` 이다.
- [ ] 5. `Type` 드롭다운을 열면 `Consumable` / `Progression` 두 개뿐이고, `Effect Type` 드롭다운은 `Heal` 하나뿐이다 (D-03/D-04).
- [ ] 6. Play 를 시작한다. 플레이어가 적 공격이나 낙하로 피해를 입어 **현재 체력 < 최대 체력** 상태를 만든다. 체력 UI 값을 적어 둔다.
- [ ] 7. Play 중 `HealthPotion.asset` 을 선택하고 Inspector 헤더의 ⋮(또는 헤더 우클릭) → `Phase16: Use On Player` 를 클릭한다. → **체력이 정확히 20 회복**된다(최대 체력에 걸리면 최대치에서 멈춘다). Console 예외 0건 (D-02/D-03 end-to-end).
- [ ] 8. 같은 방식으로 `AncientKey.asset` 에서 `Phase16: Use On Player` 를 클릭한다. → **체력 변화 0**, Console 예외 0건 (D-02 진행아이템 no-op).
- [ ] 9. 6~8을 체력이 가득 찬 상태에서 한 번 더 반복한다. → `HealthPotion` 사용 시 체력이 최대치를 넘지 않는다(`PlayerStats.ClampHealth()` 동작 확인).
- [ ] 10. Play 를 종료한다. Console 에러 0건. `git status --porcelain Assets/Item` 이 비어 있다 (Play 모드가 `.asset` 값을 디스크에 바꿔 쓰지 않았다 — SO 는 Play 중 변경이 에디터에 남을 수 있으므로 반드시 확인).

## 임포트 부작용

Task 1 의 Unity 배치모드 임포트 실행 후 `Assets/Item/` **밖**에서 `ProjectSettings/EditorBuildSettings.asset` 1개 파일의 개행 정규화(diff 내용 없음, 줄바꿈만 영향)가 관측되었다. 이 플랜의 범위(`Assets/Item/`)가 아니므로 **커밋하지 않았다**.

## 알려진 한계 / 범위 밖

- 인벤토리 자료구조·슬롯·스택은 Phase 17.
- 월드 아이템 픽업과 `PlayerInteraction` 배선은 Phase 17 — 그래서 이 phase 에서는 `Phase16: Use On Player` ContextMenu 훅이 `UseEffect` 의 유일한 호출 경로다.
- 세이브/로드 연동(`SaveData.Items`)은 Phase 18.
- 아이콘/표시 이름/설명 등 UI 메타데이터는 UI phase (D-06 의도적 배제).
- `id` 중복/공백 검증 도구 없음 — 수동 관리 (D-05).
- 진행아이템의 `Effect Type`/`Amount` 필드는 Inspector 에 계속 보이지만 무시된다. 조건부 숨김은 Phase 17 과제.

## 결과 기록

- 검증 일자:
- 검증자:
- PASS/FAIL 요약:
