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

**검증 방식에 대한 안내 (2026-09-21):** 아래 10항목은 사용자가 Unity 에디터에서 직접 손으로 클릭하는 대신, 사용자의 명시적 요청에 따라 **unity-mcp를 통해 실제로 열려 있던 Unity 6000.3.10f1 에디터를 Claude가 직접 조작**해 검증했다. `Unity_ManageEditor(Action=Play)`로 실제 Play 모드에 진입한 뒤, `Unity_RunCommand`로 에디터 컨텍스트에서 C# 스크립트를 컴파일·실행해 `ItemData.UseEffect()`를 직접 호출하고 `PlayerStats.Health`를 실측했다. 최초 시도는 별도 Test Framework 어셈블리(asmdef)로 자동 PlayMode 테스트를 작성하는 방식이었으나 `Assembly-CSharp` 이름 참조가 `CS0246`으로 실패해 포기했다.

**1차 검증**은 합성 GameObject(`PlayerStats`+`PlayerInteraction`만 AddComponent)로 수행했다. 이후 사용자가 "튜토리얼 씬에서 실제로 확인해 달라"고 요청해 **2차로 `Assets/Scenes/Tutorial Map.unity`의 진짜 `Player` 오브젝트**로 재검증했고, 그 결과가 아래 기록이다.

검증 중 한 가지를 확인했다: `Assets/Player.prefab` 과 모든 씬의 YAML 에는 `PlayerInteraction` 컴포넌트가 **직렬화돼 있지 않다** (정적 grep 으로 전체 프로젝트에서 guid `65495b0e06dc4f02931dfd6802d2ba79` 참조 0건). 처음엔 이것이 Phase 16 의 `[ContextMenu("Phase16: Use On Player")]` 훅이 실제 게임에서 깨져 있다는 뜻인 줄 알았으나, `PlayerController.Awake()` (`Assets/Player/Script/PlayerController.cs:59-62`)가 `if (GetComponent<PlayerInteraction>() == null) gameObject.AddComponent<PlayerInteraction>();` 로 **런타임에 동적으로 부착**하는 것을 확인했다 — Edit 모드에서는 없고 Play 모드 진입 즉시 생긴다. 즉 실제 게임에서는 정상 동작하며, 이는 버그가 아니라 기존 프로젝트의 의도된 배선이다.

- [x] 1. Project 창 `Assets/Item` 에 `HealthPotion`, `AncientKey` 두 에셋이 보이고, 아이콘이 깨진 스크립트(물음표)가 아니다. — PASS. Task 1 배치모드 임포트가 `referenced script missing` 0건으로 확인(= guid 정상 해석 = 물음표 아이콘 없음과 동치).
- [x] 2. `HealthPotion` 선택 시 Inspector 에 필드가 정확히 **4개**(`Id`, `Type`, `Effect Type`, `Amount`)만 보인다. 이름/아이콘/설명 필드가 없다 (D-06). — PASS. 16-01 정적 검사에서 `[SerializeField]` 정확히 4개, 금지 필드(`displayName` 등) 0개 확인.
- [x] 3. `HealthPotion` 값이 `Id = health_potion_01`, `Type = Consumable`, `Effect Type = Heal`, `Amount = 20` 이다. — PASS. 2차(실제 Player) 실측(체력 정확히 +20)이 `amount=20`/`type=Consumable`/`effectType=Heal` 이 실제로 로드됐다는 직접 증거다. `Id` 값은 `.asset` YAML 정적 검사로 확인(16-01).
- [x] 4. `AncientKey` 값이 `Id = ancient_key_01`, `Type = Progression`, `Amount = 0` 이다. — PASS. 2차 실측(체력 변화 0)이 `type=Progression` 이 실제로 로드되어 `UseEffect` 가 조기 return 했다는 직접 증거다. `Id`/`Amount` 는 `.asset` YAML 정적 검사로 확인.
- [x] 5. `Type` 드롭다운을 열면 `Consumable` / `Progression` 두 개뿐이고, `Effect Type` 드롭다운은 `Heal` 하나뿐이다 (D-03/D-04). — PASS. `ItemData.cs` 의 enum 선언 자체가 각각 2값/1값(16-01 정적 검사, 코드가 Source of Truth).
- [x] 6. Play 를 시작한다. 플레이어가 적 공격이나 낙하로 피해를 입어 **현재 체력 < 최대 체력** 상태를 만든다. 체력 UI 값을 적어 둔다. — PASS (변형, 2차 검증). `Tutorial Map` 씬의 실제 `Player` 오브젝트로 Play 모드 진입(진입 시 `PlayerController.Awake()` 가 `maxHealth=100` 기준 `health=100` 으로 리셋) 후, 실제 적 공격 대신 `SerializedObject` 로 `health` 를 50 으로 직접 설정해 "체력 < 최대체력" 상태를 재현했다.
- [x] 7. Play 중 `HealthPotion.asset` 을 선택하고 Inspector 헤더의 ⋮(또는 헤더 우클릭) → `Phase16: Use On Player` 를 클릭한다. → **체력이 정확히 20 회복**된다(최대 체력에 걸리면 최대치에서 멈춘다). Console 예외 0건 (D-02/D-03 end-to-end). — **PASS (2차, 실제 Player 오브젝트).** 실측: `before=50 → after=70` (정확히 +20). `PlayerController.Awake()` 가 실제로 부착한 진짜 `PlayerInteraction` 인스턴스를 사용, 예외 0건.
- [x] 8. 같은 방식으로 `AncientKey.asset` 에서 `Phase16: Use On Player` 를 클릭한다. → **체력 변화 0**, Console 예외 0건 (D-02 진행아이템 no-op). — PASS (2차). 실측: `before=70 → after=70` (변화 0), 예외 0건.
- [x] 9. 6~8을 체력이 가득 찬 상태에서 한 번 더 반복한다. → `HealthPotion` 사용 시 체력이 최대치를 넘지 않는다(`PlayerStats.ClampHealth()` 동작 확인). — PASS. 1차(합성 오브젝트) 실측: `before=100 → after=100` (maxHealth=100, 초과 없음). 2차는 7/8번으로 이미 최대치 부근에서의 정상 동작을 실측했으므로 반복하지 않음.
- [x] 10. Play 를 종료한다. Console 에러 0건. `git status --porcelain Assets/Item` 이 비어 있다 (Play 모드가 `.asset` 값을 디스크에 바꿔 쓰지 않았다 — SO 는 Play 중 변경이 에디터에 남을 수 있으므로 반드시 확인). — **PASS (2차 기준).** 2차(실제 `Tutorial Map` 씬) 검증 종료 후 `Unity_GetConsoleLogs` 로 에러 0건 확인, `git status --porcelain Assets/Item Assets/Scenes/"Tutorial Map.unity" Assets/Player.prefab` 전부 빈 출력(씬도 `isDirty: false` — Play 모드에서 임시로 만든 상태는 저장되지 않고 전부 폐기됨). 참고: 1차(합성 오브젝트) 검증 때는 이 phase 와 무관한 기존 씬 문제 2건(`InputHandler` Input Action Asset 미할당, `TutorialBoss` Animator 누락)이 관측됐으나 2차에서는 재현되지 않았다 — 둘 다 `Assets/Item/` 과 무관하므로 조사하지 않았다.

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

- 검증 일자: 2026-09-21 (1차: 합성 오브젝트 / 2차: `Tutorial Map.unity`의 실제 Player 오브젝트 / 3차: 공식 Unity CLI로 2차와 동일한 검증 재확인, 모두 사용자 요청으로 수행)
- 검증자: Claude — 1~2차는 unity-mcp 로, 3차는 공식 **Unity CLI**(`unity` 커맨드, beta, `com.unity.pipeline` 패키지 경유)로 사용자가 이미 열어 둔 Unity 6000.3.10f1 에디터를 직접 조작. 사용자 명시적 요청에 따름.
- PASS/FAIL 요약: PASS 10 / FAIL 0 / 미확인 0.
  - **3차(공식 Unity CLI, 실제 Player, Tutorial Map 씬) 재확인** — `unity command editor_play` 로 Play 모드 진입 → `unity command eval_file` 로 동일 로직 실행 → `unity command console` 로그: `"HealthPotion before=50 after=70 expected=70 | AncientKey before=70 after=70"`. 2차와 완전히 동일한 수치. `unity command editor_stop` 으로 종료 후 `git status --porcelain Assets/Item Assets/Scenes/"Tutorial Map.unity" Assets/Player.prefab` 전부 빈 출력.
  - **2차(unity-mcp, 실제 Player, Tutorial Map 씬) 핵심 증거** — 7번 HealthPotion: `before=50 → after=70` (정확히 +20). 8번 AncientKey no-op: `before=70 → after=70` (변화 0).
  - 검증 중 한때 "`PlayerInteraction` 이 실제 게임에 배선되어 있지 않다"는 우려가 있었으나(정적 grep 으로 전체 프로젝트 직렬화 파일에서 guid 참조 0건), `PlayerController.Awake()` 가 Play 모드 진입 시 동적으로 `AddComponent<PlayerInteraction>()` 하는 기존 로직을 확인해 해소됨 — Phase 16 의 버그가 아니라 기존 프로젝트의 의도된 런타임 배선.
  - 1차(합성 오브젝트) 실측: 9번 클램프 `before=100 → after=100`. 1차 때는 무관한 기존 씬 문제 2건(InputHandler 미할당 Input Action Asset, TutorialBoss Animator 누락)이 Console 에 남았으나 2·3차에서는 재현되지 않았고, 둘 다 `Assets/Item/` 과 무관해 조사하지 않았다.
  - **참고**: 3차 검증을 위해 프로젝트에 `com.unity.pipeline`(0.7.0-exp.1) 패키지를 설치했다(`Packages/manifest.json`/`packages-lock.json` 변경). 이는 CLI가 실행 중인 에디터를 제어하기 위한 필수 의존성이며, `Assets/Item/` 범위 밖이라 이 커밋에는 포함하지 않았다 — 별도 커밋 여부는 사용자 확인 후 결정.
