# 오디오 (중앙 집중형 AudioManager)

**브랜치 주의**: 이 문서가 기술하는 내용은 `20-audio-centralization` 브랜치에만 존재한다. 이 SSOT 스냅샷을 생성한 `주창은` 브랜치(및 그 기준 `ec03fd9`)에는 `AudioManager`/`Master.mixer`가 아직 없다 — Phase 17/18/19(아이템)와 달리 이 phase는 아직 `주창은`에 병합되지 않았다. 병합 전까지 이 문서는 "예정된 다음 상태"를 기록한 것으로 취급할 것.

## 책임

`AudioManager`가 BGM/SFX/UI 재생 권한, `AudioMixer` 파라미터, `AudioSource` 풀, 환경 상태에 따른 BGM 필터링을 중앙에서 소유한다. `EnvironmentManager`는 더 이상 BGM `AudioSource`나 로우패스 필터를 직접 조작하지 않고 `AudioManager.SetEnvironmentState(...)` 호출로만 음향 상태 변경을 요청한다.

## 런타임 구조

| 컴포넌트/에셋 | 위치 | 내용 |
|---|---|---|
| `AudioManager : MonoBehaviour` | `Assets/Script/AudioManager.cs`, `PersistentManagers`(root, `DontDestroyOnLoad`)에 부착 | `mixer`(AudioMixer), `bgmGroup`/`sfxGroup`/`uiGroup`, `bgmSource`+`bgmLowPass`, 고정 16개 `PooledSource_0..15` 풀, `currentEnvironmentState` |
| `Master.mixer` | `Assets/Audio/Resources/Master.mixer` | Master/BGM/SFX/UI 그룹, 노출 파라미터 `BGMVolume`/`SFXVolume`(선형→dB, `Mathf.Max(linear,0.0001f)`로 −80dB 하한 클램프) |
| `AudioCue : ScriptableObject` | `Assets/Audio/Script/AudioCue.cs` | 데이터 정의만 존재 — 실제 `.asset` 인스턴스나 `Play(...)` 호출부는 아직 없음(향후 작업) |
| `PersistentManagers` | 부트스트랩 시 생성되는 root GameObject | `AudioManager` + `SaveLoadManager`를 자식으로 소유하는 `DontDestroyOnLoad` 컨테이너 |

## 동작과 실패 경로

- `SetBGMVolume(float)`/`SetSFXVolume(float)` 시그니처는 Phase 20 이전과 동일하게 유지된다(`SoundSettingsPanel.cs` 무변경) — 내부적으로 선형 값을 dB로 변환해 `mixer.SetFloat`를 호출할 뿐, 실제 값은 여전히 `SaveLoadManager.CurrentSettings.BgmVolume`/`SfxVolume`에 저장된다. `AudioManager`는 재생만 소유하고 영속화는 `SaveLoadManager`가 그대로 담당한다([save-and-settings.md](./save-and-settings.md) 참고).
- `SetEnvironmentState(EnvironmentState)`(`None`/`Alive`/`Neutral`/`Withered`)는 `bgmLowPass.cutoffFrequency`만 바꾸고 `bgmSource`를 멈추거나 재시작하지 않는다 — 상태 전환 중에도 BGM 재생 위치(`bgmSource.time`)가 유지된다(실측: Alive=22000Hz, Neutral=5000Hz, Withered=1000Hz).
- `AudioSource` 풀은 `Instantiate` 없이 고정 16슬롯을 미리 생성해두고 재사용한다(`Instantiate` 호출 0건 — 정적 grep 게이트로 보장).
- `AudioManager`는 씬 전환 후에도 동일 인스턴스가 유지되며(`PersistentManagers` 자식), 씬 전환 전에 설정한 BGM/SFX 값이 그대로 보존된다(Unity CLI 실측 확인).

## 제약

- 11개 씬(`.unity`)에 `EnvironmentManager`의 죽은 `bgmSource`/`lowPassFilter` 직렬화 잔재가 남아있다 — C# 필드는 제거됐고 런타임에 무시되며, 각 씬을 다음에 저장할 때 Unity가 자동으로 제거한다. 의도적으로 손으로 정리하지 않았다.
- `AudioCue` 에셋 인스턴스와 실제 `Play(...)` 호출부(몬스터/공격/피격/보스 사운드)가 전혀 없다 — 풀/쿨다운/우선순위 스틸 로직은 컴파일만 됐을 뿐 게임플레이 Play 모드 커버리지가 없다.
- `InputHandler`는 아직 `PersistentManagers` 밖에 있다(BUG-007 미해결, 이 phase 범위 밖).

## 근거

- `Assets/Script/AudioManager.cs`
- `Assets/Script/PersistentManagers.cs`
- `Assets/Audio/Script/AudioCue.cs`
- `Assets/Audio/Resources/Master.mixer`
- `Assets/Player/Script/Menu/SoundSettingsPanel.cs`(무변경 확인)
- `Assets/Audio/Check.md`(정적 회귀 13항목 PASS + Play 모드 체크리스트 10항목)

## 검증

`partial` — `20-audio-centralization` 브랜치에서 Unity CLI(`unity command eval`, reflection)로 Play 모드 실측: 구조(PersistentManagers/풀/중복 없음), 볼륨→dB 변환, 환경 상태 전환 시 필터+BGM 연속재생, 씬 전환 후 인스턴스/볼륨값 유지, 슬라이더 UI 콜백 경로까지 10개 항목 전부 프로그램적으로 통과(2026-09-24). 사람이 직접 듣고 확인하는 절차(스피커 음량 체감, 배경 색상 육안 확인)는 아직 대체되지 않았다 — `Assets/Audio/Check.md` Task 3(`checkpoint:human-verify`)는 미완료 상태로 남아있다.
