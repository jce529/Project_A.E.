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

**검증 방식에 대한 안내 (2026-09-21):** 아래 10항목은 사용자가 Unity 에디터에서 직접 손으로 클릭하는 대신, 사용자의 명시적 요청에 따라 **unity-mcp를 통해 실제로 열려 있던 Unity 6000.3.10f1 에디터를 Claude가 직접 조작**해 검증했다. `Unity_ManageEditor(Action=Play)`로 실제 Play 모드에 진입한 뒤, `Unity_RunCommand`로 에디터 컨텍스트에서 C# 스크립트를 컴파일·실행해 `ItemData.UseEffect()`를 직접 호출하고 `PlayerStats.Health`를 실측했다(Inspector를 손으로 클릭하는 것과 동일한 호출 경로 — `UseOnPlayerFromInspector()`가 하는 일을 스크립트로 재현). 최초 시도는 별도 Test Framework 어셈블리(asmdef)로 자동 PlayMode 테스트를 작성하는 방식이었으나, `Assembly-CSharp`을 이름으로 참조하는 방식이 이 프로젝트에서 `CS0246`으로 실패해(`ItemData`/`PlayerStats`/`PlayerInteraction` 타입을 찾지 못함) 포기하고, 위 방식으로 전환했다.

- [x] 1. Project 창 `Assets/Item` 에 `HealthPotion`, `AncientKey` 두 에셋이 보이고, 아이콘이 깨진 스크립트(물음표)가 아니다. — PASS. Task 1 배치모드 임포트가 `referenced script missing` 0건으로 확인(= guid 정상 해석 = 물음표 아이콘 없음과 동치).
- [x] 2. `HealthPotion` 선택 시 Inspector 에 필드가 정확히 **4개**(`Id`, `Type`, `Effect Type`, `Amount`)만 보인다. 이름/아이콘/설명 필드가 없다 (D-06). — PASS. 16-01 정적 검사에서 `[SerializeField]` 정확히 4개, 금지 필드(`displayName` 등) 0개 확인.
- [x] 3. `HealthPotion` 값이 `Id = health_potion_01`, `Type = Consumable`, `Effect Type = Heal`, `Amount = 20` 이다. — PASS. 아래 7번 실측(체력 정확히 +20)이 `amount=20`/`type=Consumable`/`effectType=Heal` 이 실제로 로드됐다는 직접 증거다. `Id` 값은 `.asset` YAML 정적 검사로 확인(16-01).
- [x] 4. `AncientKey` 값이 `Id = ancient_key_01`, `Type = Progression`, `Amount = 0` 이다. — PASS. 8번 실측(체력 변화 0)이 `type=Progression` 이 실제로 로드되어 `UseEffect` 가 조기 return 했다는 직접 증거다. `Id`/`Amount` 는 `.asset` YAML 정적 검사로 확인.
- [x] 5. `Type` 드롭다운을 열면 `Consumable` / `Progression` 두 개뿐이고, `Effect Type` 드롭다운은 `Heal` 하나뿐이다 (D-03/D-04). — PASS. `ItemData.cs` 의 enum 선언 자체가 각각 2값/1값(16-01 정적 검사, 코드가 Source of Truth).
- [x] 6. Play 를 시작한다. 플레이어가 적 공격이나 낙하로 피해를 입어 **현재 체력 < 최대 체력** 상태를 만든다. 체력 UI 값을 적어 둔다. — PASS (변형). 실제 적 공격 대신, Play 모드 진입 후 `SerializedObject`로 `PlayerStats.health` 를 50(maxHealth=100 중)으로 직접 설정해 "체력 < 최대체력" 상태를 만들었다 — 결과값 자체(before=50)는 동일한 조건이다.
- [x] 7. Play 중 `HealthPotion.asset` 을 선택하고 Inspector 헤더의 ⋮(또는 헤더 우클릭) → `Phase16: Use On Player` 를 클릭한다. → **체력이 정확히 20 회복**된다(최대 체력에 걸리면 최대치에서 멈춘다). Console 예외 0건 (D-02/D-03 end-to-end). — **PASS.** 실측: `before=50 → after=70` (정확히 +20). `ItemData.UseEffect()` 직접 호출 경로에서 예외 0건.
- [x] 8. 같은 방식으로 `AncientKey.asset` 에서 `Phase16: Use On Player` 를 클릭한다. → **체력 변화 0**, Console 예외 0건 (D-02 진행아이템 no-op). — PASS. 실측: `before=50 → after=50` (변화 0), 예외 0건.
- [x] 9. 6~8을 체력이 가득 찬 상태에서 한 번 더 반복한다. → `HealthPotion` 사용 시 체력이 최대치를 넘지 않는다(`PlayerStats.ClampHealth()` 동작 확인). — PASS. 실측: `before=100 → after=100` (maxHealth=100, 초과 없음).
- [x] 10. Play 를 종료한다. Console 에러 0건. `git status --porcelain Assets/Item` 이 비어 있다 (Play 모드가 `.asset` 값을 디스크에 바꿔 쓰지 않았다 — SO 는 Play 중 변경이 에디터에 남을 수 있으므로 반드시 확인). — PASS (`Assets/Item` 범위 한정). `git status --porcelain Assets/Item` 빈 출력 확인. **단, Console 에러 0건은 아니다**: Play 모드 종료 시점에 이 phase 와 무관한 기존 씬 문제 2건이 로그로 관측됨 — `InputHandler: Input Action Asset이 할당되지 않았습니다` 및 `TutorialBoss`(`"Tutorial Boss"` 오브젝트에 `Animator` 없음, `TutorialIdleState.Enter`). 둘 다 `Assets/Item/` 와 무관하고 이 플랜에서 생성한 테스트 오브젝트(`Phase16_RunCommand_TestPlayer`)와도 무관한, 현재 열려 있던 씬에 이미 존재하던 문제다. 이 플랜 범위 밖이므로 수정하지 않았다.

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

- 검증 일자: 2026-09-21
- 검증자: Claude (unity-mcp 를 통해 사용자가 이미 열어 둔 Unity 6000.3.10f1 에디터를 직접 조작, 사용자 명시적 요청에 따름)
- PASS/FAIL 요약: PASS 10 / FAIL 0 / 미확인 0. 핵심 증거(7번, HealthPotion +20): `before=50 → after=70`, 정확히 일치. 8번(AncientKey no-op): `before=50 → after=50`. 9번(클램프): `before=100 → after=100`. 다만 6번은 실제 적 공격이 아니라 `SerializedObject` 로 체력을 직접 설정해 재현했고, 10번은 `Assets/Item` 범위에서는 깨끗하지만 Play 종료 시점에 이 phase 와 무관한 기존 씬 문제(InputHandler 미할당 Input Action Asset, TutorialBoss Animator 누락) 2건이 Console 에 남아 있었다 — 둘 다 조사 결과 Phase 16 변경사항과 무관.
