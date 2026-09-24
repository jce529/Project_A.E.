# Phase 20: 중앙 집중형 오디오 시스템 및 AudioSource 풀링 - Context

**Gathered:** 2026-09-22
**Status:** Ready for planning

<domain>
## Phase Boundary

자동 생성되는 영속 `AudioManager`가 BGM/SFX/UI 재생 권한과 `AudioMixer`, 데이터 기반 `AudioCue`(ScriptableObject), 위치·추적 재생, `AudioSource` 풀, 동시 재생 제한·재트리거 쿨다운·강탈 규칙을 중앙에서 소유한다. `EnvironmentManager`는 BGM `AudioSource`/`AudioLowPassFilter`를 직접 조작하지 않고 환경 상태 판정 결과만 `AudioManager`에 요청한다. `AudioManager`의 영속성 확보를 위해 확장 가능한 `PersistentManagers` 부트스트랩 프레임워크를 도입하되, 이번 phase에서는 `AudioManager`만 이 구조로 마이그레이션한다.

**범위 밖:** BUG-007(InputHandler 씬 전환 유실) 자체의 수정 — `PersistentManagers` 프레임워크는 확장 가능하게 설계하지만 InputHandler를 지금 이전하지는 않는다. 이는 별도 버그 수정 작업으로 남긴다.

</domain>

<decisions>
## Implementation Decisions

### AudioCue (ScriptableObject)
- **D-01:** 기본형 필드 구성 — `AudioClip`(단일 또는 배열에서 랜덤 선택), 볼륨 랜덤 범위, 피치 랜덤 범위, 카테고리.
- **D-02:** 카테고리는 BGM/SFX/UI 3분류.
- **D-03:** 재트리거 쿨다운은 Cue별 고정 필드(초 단위)로 가짐. `AudioManager`가 Cue별 `lastPlayedTime`을 딕셔너리로 관리하고, 쿨다운 내 재요청은 무시한다.
- **D-04:** 위치/추적 재생은 Cue별 2D/3D 플래그(`spatialBlend` 고정값)로 결정 — BGM은 항상 2D, 필요한 SFX는 3D로 설정.
- **D-05:** 우선순위는 Cue에 필드로 보관하되(향후 확장 대비), 이번 phase의 강탈 로직에서 실제로 사용됨(D-08 참고).

### AudioSource 풀링 & 동시재생
- **D-06:** 풀은 고정 크기로 시작 시 미리 생성. 동적 확장 없음.
- **D-07:** 추적 재생은 `Play(cue, Transform target)` 형태로 호출하고, `AudioManager`가 대상 Transform을 보관하며 매 프레임 AudioSource의 position을 갱신. 고정 위치 재생은 `Play(cue, Vector3 position)`으로 별도 제공.
- **D-08:** 풀이 가득 찬 상태에서 새 재생 요청이 오면, 현재 재생 중인 것 중 **우선순위가 낮은 것들 중 가장 오래된 것부터** 강탈(steal)한다. 요청의 우선순위가 모든 현재 재생보다 낮으면 재생 자체를 무시(풀 미할당).

### AudioMixer 및 볼륨 마이그레이션
- **D-09:** Mixer 그룹 구조: `Master` 하위에 `BGM`/`SFX`/`UI` 3개 그룹. AudioCue 카테고리(D-02)와 1:1 대응.
- **D-10:** `SaveLoadManager.CurrentSettings`의 기존 0~1 선형 볼륨 값 스키마는 그대로 유지한다. `AudioManager`가 `SetBGMVolume(float)` / `SetSFXVolume(float)` 등 기존 시그니처를 유지하면서 내부적으로 `Mathf.Log10` 변환 후 `AudioMixer.SetFloat`(dB)에 적용한다. 외부(SoundSettingsPanel, SaveLoadManager)는 변경 없음.
- **D-11:** UI 카테고리를 위한 볼륨 파라미터/필드도 BGM/SFX와 동일한 패턴으로 추가 필요 (예: `UiVolume`).

### EnvironmentManager 통합 & 부트스트랩
- **D-12:** `EnvironmentManager`는 `AudioManager.SetEnvironmentState(EnvironmentState state)` 형태의 API를 호출한다. 상태 판정(Alive/Neutral/Withered)은 `EnvironmentManager`가 계속 담당하고, cutoff 수치 계산과 BGM 재생/필터 적용은 `AudioManager` 내부로 이전.
- **D-13:** `PersistentManagers` 부트스트랩 프레임워크를 신설하여 `AudioManager`의 `DontDestroyOnLoad`를 여기서 관리한다. 프레임워크는 향후 다른 매니저(InputHandler 등)도 등록 가능하도록 확장 가능하게 설계하되, **이번 phase에서는 AudioManager만 실제로 마이그레이션**한다. InputHandler(BUG-007)는 이 phase의 범위가 아니다.

### Claude's Discretion
- AudioMixer 에셋의 정확한 dB 커브 매핑 상수, 풀 기본 크기(N) 숫자, `PersistentManagers`의 내부 등록 API 세부 시그니처는 연구/계획 단계에서 Claude가 결정.

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

### 기존 오디오 관련 코드 (교체/리팩터 대상)
- `Assets/Script/AudioManager.cs` — 현재 싱글톤 BGM/SFX 볼륨 관리자. `BgmVolume`/`SfxVolume` 필드와 `SetBGMVolume`/`SetSFXVolume`/`PlaySFX` 시그니처는 유지해야 함 (D-10).
- `Assets/Script/EnvironmentManager.cs` — 현재 `bgmSource`/`AudioLowPassFilter`를 직접 조작 중. `ApplyEnvironmentEffects`/`ChangeBGMCutoff`의 오디오 관련 로직을 `AudioManager.SetEnvironmentState`로 이전 (D-12). 배경색 변경(`backgroundRenderers`) 로직은 그대로 유지.
- `Assets/Player/Script/Menu/SoundSettingsPanel.cs` — `AudioManager.Instance` 호출부. 시그니처 유지되면 변경 불필요할 가능성 높음, 연구 단계에서 확인 필요.
- `SaveLoadManager.CurrentSettings` (BgmVolume/SfxVolume 필드) — 볼륨 저장 스키마. 변경하지 않음 (D-10).

### 관련 미해결 이슈
- `.planning/phases/15-load-timing-and-load-scope/bugs/BUG-007-inputhandler-lost-on-scene-transition.md` — `PersistentManagers` 프레임워크와 소프트 연계되는 미해결 버그. 이번 phase에서 InputHandler는 마이그레이션하지 않음 (D-13). 프레임워크 설계 시 이 버그의 요구사항(루트 오브젝트 `DontDestroyOnLoad`, 중복 인스턴스 처리)을 참고해 확장 가능하게 설계.

### 로드맵
- `.planning/ROADMAP.md` Phase 20 섹션 — 원본 목표/의존성 서술.

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- 없음 — 이 프로젝트에 기존 AudioSource 풀링, AudioMixer, ScriptableObject 기반 Cue 시스템 없음. Phase 20이 이 인프라의 최초 도입.
- 기존 `BossController`/`BossStatsSystem`의 이벤트 패턴(`OnDamageTaken` 등)은 향후 보스 공격/피격 오디오 훅 연결 시 참고 가능 (이번 phase 범위는 아니지만 향후 확장 지점).

### Established Patterns
- 싱글톤 패턴: `Instance` static 프로퍼티 + `Awake()`에서 중복 시 `Destroy(gameObject)` — 기존 `AudioManager.cs`가 이 패턴 사용 중. `PersistentManagers` 설계 시 이 관례와 일관성 유지 권장.
- 설정 저장: 볼륨은 게임플레이 값이 아니라 `SaveLoadManager.CurrentSettings`를 통해 메모리/영속 저장 — 이 경로는 건드리지 않음.

### Integration Points
- `EnvironmentManager` → `AudioManager` (신규 `SetEnvironmentState` 호출)
- `SoundSettingsPanel` → `AudioManager` (기존 볼륨 API, 변경 없음)
- 씬 부트스트랩: 현재 `AudioManager`가 씬에 배치된 GameObject로 존재하는 것으로 추정 — `PersistentManagers` 도입 시 이 배치 방식이 바뀔 수 있음 (연구 단계에서 씬 파일 확인 필요).

</code_context>

<specifics>
## Specific Ideas

없음 — 논의 전반이 구조적 결정(D-01~D-13)으로 구체화됨.

</specifics>

<deferred>
## Deferred Ideas

- **BUG-007 (InputHandler 씬 전환 유실) 완전 수정** — `PersistentManagers` 프레임워크는 확장 가능하게 설계하지만, InputHandler를 이 프레임워크로 옮기고 버그를 해결하는 작업은 별도로 진행. (사용자가 명시적으로 범위 제외 결정)
- **Cue별 3D spatial 세부 파라미터(minDistance/maxDistance) 오버라이드** — 기본형 AudioCue에는 포함하지 않기로 결정. 필요해지면 향후 확장.
- **자동 스폰 몬스터/공격·피격·스킬·보스 패턴 오디오 훅 실제 연결** — 로드맵 목표에 언급된 "수용" 대상이지만, 이번 phase는 인프라(AudioManager/AudioCue/풀/믹서)만 구축. 실제 훅 연결은 각 기능이 구현되는 후속 phase의 몫.

</deferred>

---

*Phase: 20-audio-centralization*
*Context gathered: 2026-09-22*
