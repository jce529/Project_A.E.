# Phase 20: 중앙 집중형 오디오 시스템 및 AudioSource 풀링 - Discussion Log

> **Audit trail only.** Do not use as input to planning, research, or execution agents.
> Decisions are captured in CONTEXT.md — this log preserves the alternatives considered.

**Date:** 2026-09-22
**Phase:** 20-audio-centralization
**Areas discussed:** AudioCue 데이터 구조, AudioSource 풀링 & 동시재생 제한, AudioMixer 및 볼륨 체계 마이그레이션, EnvironmentManager 통합 & 부트스트랩

---

## AudioCue 데이터 구조

| Option | Description | Selected |
|--------|-------------|----------|
| 기본형 (클립+볼륨/피치 랜덤+카테고리) | AudioClip(단일/배열 랜덤), 볼륨/피치 랜덤 범위, 카테고리 | ✓ |
| 기본형 + 쿨다운/우선순위 | 위 필드 + 재트리거 쿨다운, 우선순위 | |
| 기본형 + 쿨다운/우선순위 + 공간음향 옵션 | 위 전부 + Cue별 3D spatial 오버라이드 | |

**User's choice:** 기본형 (클립+볼륨/피치 랜덤+카테고리)
**Notes:** 후속 질문에서 쿨다운(D-03)과 우선순위(D-05), 2D/3D 플래그(D-04)가 결국 개별 필드로 추가됨 — "기본형"은 세부 옵션(공간음향 min/maxDistance 오버라이드) 제외를 의미했음.

### 후속: 카테고리 세분화
| Option | Selected |
|--------|----------|
| BGM/SFX 2분류 | |
| BGM/SFX/UI 3분류 | ✓ |

---

## AudioSource 풀링 & 동시재생 제한

### 풀 크기 관리
| Option | Selected |
|--------|----------|
| 고정 크기 + 부족 시 가장 오래된 재생 강탈 | ✓ |
| 고정 크기 + 동적 확장 | |

### 위치/추적 재생
| Option | Selected |
|--------|----------|
| AudioCue별 2D/3D 플래그 | ✓ |
| 호출 시프(Play(cue, position/transform))로 결정 | |

### 추적 재생 구현
| Option | Selected |
|--------|----------|
| Play(cue, Transform target)로 매 프레임 위치 갱신 | ✓ |
| Play(cue, Vector3 position) 고정 위치만 | |

### 재트리거 쿨다운
단일 옵션으로 제시 불가(선택지 부족) — Claude가 유일하게 타당한 접근으로 판단하여 채택: Cue별 고정 쿨다운(초) 필드, AudioManager가 lastPlayedTime 딕셔너리로 관리.

### 풀 강탈 규칙
| Option | Selected |
|--------|----------|
| 가장 오래된 재생부터 강탈 (우선순위 무시) | |
| 우선순위 낮은 것 중 가장 오래된 것부터 강탈 | ✓ |

---

## AudioMixer 및 볼륨 체계 마이그레이션

### 믹서 그룹 구조
| Option | Selected |
|--------|----------|
| Master 하위에 BGM/SFX/UI 3개 그룹 | ✓ |
| Master/BGM/SFX 2단계 + UI는 SFX 하위 서브그룹 | |

### 볼륨 값 매핑
| Option | Selected |
|--------|----------|
| 기존 0~1 선형값 유지, AudioManager 내부에서 log 변환 | ✓ |
| SaveLoadManager 설정 자체를 dB 기반으로 변경 | |

---

## EnvironmentManager 통합 & 부트스트랩

### Env API 형태
| Option | Selected |
|--------|----------|
| AudioManager.SetEnvironmentState(enum state) | ✓ |
| AudioManager.SetLowPassCutoff(float freq) 직접 수치 전달 | |

### 영속성 확보 방식
| Option | Selected |
|--------|----------|
| AudioManager 자체 싱글톤 부트스트랩 (독립) | |
| PersistentManagers 구조를 Phase 20에서 함께 설계 | ✓ |

### 부트스트랩 범위
| Option | Selected |
|--------|----------|
| AudioManager만 이 구조로 마이그레이션, 프레임워크는 확장 가능하게만 설계 | ✓ |
| InputHandler도 함께 이전해 BUG-007도 이 Phase에서 해결 | |

**Notes:** BUG-007 자체 수정은 명시적으로 범위 제외 — Deferred Ideas에 기록.

---

## Claude's Discretion

- AudioMixer dB 커브 매핑 상수
- 풀 기본 크기(N) 숫자
- PersistentManagers 내부 등록 API 세부 시그니처

## Deferred Ideas

- BUG-007(InputHandler 씬 전환 유실) 완전 수정 — 별도 작업
- Cue별 3D spatial 세부 파라미터(min/maxDistance) 오버라이드 — 향후 확장
- 자동 스폰 몬스터/공격·피격·스킬·보스 패턴 오디오 훅 실제 연결 — 후속 phase
