---
name: codex-execute
description: This skill should be used when the user asks to hand off implementation/coding to the "codex" CLI, says phrases like "codex로 실행해줘", "실행은 codex한테 넘겨", "execute via codex", "codex에게 위임", or when a GSD phase PLAN.md has just been approved via /gsd:plan-phase and the next step is implementation (instead of running /gsd:execute-phase's internal executor). Governs the split where Claude Code does design/research/verification and codex does the actual code writing.
version: 1.0.0
---

# Codex 실행 위임 (설계는 Claude Code, 실행은 codex)

## 개요

이 프로젝트의 개발 워크플로우는 도구 역할을 둘로 나눈다.

- **Claude Code (나)**: 설계 담당 — 리서치, 계획 수립, `.planning/`에 GSD 산출물 작성, 계획 검증, 실행 결과 리뷰/검증.
- **codex CLI** (`codex`, PATH 등록됨, 확인된 버전 0.154.0): 실행 담당 — 승인된 플랜에 따라 실제 코드를 작성/수정하는 별도의 코딩 에이전트.

이는 CLAUDE.md의 GSD 사이클(`discuss-phase` → `plan-phase` → `execute-phase` → `verify-work`)을 그대로 따르되, **`execute-phase`에서 실제 코딩을 수행하는 부분만 내부 `gsd-executor` 서브에이전트 대신 `codex` CLI가 담당**하도록 바꾼 것이다. 기획/설계는 여전히 Claude Code, 즉 이 프레임워크의 1~4번 원칙(추측 금지, 단순성, 정밀한 변경, 목표 주도적 검증)을 그대로 적용한다.

> **이전 이력:** 이 역할 분담은 원래 `agy` CLI로 수행했었다(`agy-execute` 스킬). 2026-09-21부로 codex CLI로 교체되었다. agy 전용 문법(`--mode accept-edits` 등)은 더 이상 쓰지 않는다.

## 언제 이 스킬을 쓰는가

- 사용자가 "codex로 실행해줘", "실행은 codex한테 넘겨", "codex에게 위임", "execute via codex" 등을 말할 때
- `/gsd:plan-phase`로 `.planning/phases/N-*/N-0X-PLAN.md`가 승인되어 다음 단계가 실제 구현(코딩)일 때
- 사용자가 "이제 실행 단계야"처럼 실제 파일 변경이 필요한 작업을 시작하려 할 때

이 스킬이 적용되는 동안 Claude Code는 **`.planning/` 밖의 구현 코드를 직접 작성하지 않는다.** 오타 수정처럼 사소하거나, 사용자가 "네가 직접 고쳐줘"라고 명시적으로 요청한 경우는 예외다.

## Claude Code가 계속 담당하는 일

1. `/gsd:discuss-phase`, `/gsd:plan-phase`로 `.planning/phases/N-*/` 아래 `N-CONTEXT.md`, `N-RESEARCH.md`, `N-0X-PLAN.md` 작성.
2. 계획에 모호함이나 트레이드오프가 있으면 코드를 짜기 전에 사용자에게 먼저 확인 (CLAUDE.md 1번 원칙).
3. codex 실행이 끝나면 diff 리뷰(`git status`, `git diff`), 빌드/콘솔 에러 확인, `.planning/STATE.md` 갱신, 필요하면 `/gsd:verify-work` 진행.
4. codex가 계획 범위를 벗어난 파일을 건드렸다면 사용자에게 보고 (CLAUDE.md 3번 "정밀한 변경" 원칙은 codex의 산출물에도 그대로 적용).

## 목적별 모델/추론 강도 선택 (MANDATORY)

codex를 부를 때마다 아래 표에서 **작업 목적에 맞는 모델/effort를 명시적으로 골라 `-c model=...` / `-c model_reasoning_effort=...`로 지정한다.** 아무 지정 없이 부르면 `~/.codex/config.toml`의 전역 기본값(`gpt-6-astra`, medium)이 조용히 쓰이므로, phase 실행처럼 목적이 분명한 호출에는 반드시 아래 표를 따른다.

| 목적 | 모델 | Reasoning effort | 비고 |
|------|------|-------------------|------|
| GSD PLAN.md 표준 실행 (기본값) | `gpt-5.6-sol` | `medium` | 대부분의 phase 구현. 사용자가 2026-09-21에 지정한 기본값. |
| 사소한 수정/타이포/1~2줄 버그 (codex에 맡길 정도로 커진 경우) | `gpt-5.6-sol` | `low` | 빠르게 끝내야 하는 저위험 작업. |
| 아키텍처급 판단이 필요한 에스컬레이션 재시도, 복잡한 디버깅 위임 | `gpt-5.6-sol` | `high` | Claude Code가 원인 분석 후 재위임할 때. 일반 실행보다 신중하게 사고해야 하는 경우. |

사용 가능한 모델 이름이 바뀌었는지 확실치 않으면 실행 전에 사용자에게 확인한다 — 존재하지 않는 모델 이름을 조용히 fallback 시키지 않는다.

## codex 실행 방법

설치 경로: `codex` (PATH 등록됨, `C:\Users\chang\AppData\Roaming\npm\codex.cmd`).

### 0. Context Handoff 준비 — 생략 금지

PLAN.md 안의 `<context>` 블록에 있는 `@.planning/STATE.md`, `@...-CONTEXT.md`, `@...-RESEARCH.md`,
`@Assets/.../Xxx.cs` 같은 `@경로` 줄은 **GSD 안에서만 통하는 문법**이다. 원래는 `/gsd:execute-phase`가
`gsd-tools.cjs init execute-phase`로 이 줄들을 미리 펼쳐서 `<files_to_read>` 블록을 만들고,
gsd-executor(`.claude/agents/gsd-executor.md` 22~24행)가 그걸 보고 실제로 Read 하도록 강제하는 구조다.
codex는 이 문법을 모른다 — PLAN.md 원문을 그냥 `cat` 해서 넘기면 `@경로` 줄은 codex 입장에서 의미 없는
텍스트일 뿐이고, STATE.md/CONTEXT.md/RESEARCH.md/실제 소스 파일 내용은 전달되지 않는다.

그래서 codex를 부르기 **전에 Claude Code가 직접** 이 역할을 대신한다:

1. PLAN.md의 `<context>` 블록에서 `@`로 시작하는 줄을 모두 추출한다.
2. 각 경로를 실제로 Read 한다. 이때 파일을 전체 그대로 붙이지 않는다 — **PLAN.md가 이미
   `<interfaces>` 블록에서 하고 있는 것과 같은 방식으로, 관련 있는 public 메서드 시그니처/필드/
   앵커 문자열만 뽑아서 붙인다** (예: 12-01-PLAN.md의 `CameraController.cs` 429줄 전체 대신
   관련 메서드 4~5개만 발췌한 것과 동일한 원칙). 단, `<interfaces>`/`<encoding_note>`/
   `<verification_gate_note>`처럼 앵커 문자열·바이트 카운트·주의사항이 담긴 블록 자체는 요약하지
   말고 원문 그대로 유지한다 — 이 프로젝트는 CP949/UTF-8 인코딩 훼손 이력이 있어 문구를 함부로
   줄이면 검증 게이트가 깨진다. `STATE.md`/`RESEARCH.md`처럼 긴 문서는 이번 태스크와 무관한
   과거 phase 서술을 빼고 현재 phase 관련 절만 남긴다.
3. codex 프롬프트 맨 앞에 "다음 파일들을 먼저 Read 해서 참고할 것" 형태로 파일 경로 목록을 명시하고,
   PLAN.md 본문 뒤에 이어 붙인다.

### 1. 승인된 플랜 파일 확인

```
.planning/phases/<N>-<phase-name>/<N>-0X-PLAN.md
```

프론트매터의 `files_modified:` 목록을 그대로 적어둔다 — 아래 3번에서 pathspec으로 그대로 쓴다.

### 2. git 안전 규율 (반드시 프롬프트에 포함)

이 저장소는 이미 pathspec 없는 `git commit`으로 무관한 파일이 같이 커밋된 사고(Phase 11 Plan 3)를
겪었고, PLAN.md들이 그 재발 방지 규칙을 명시적으로 담고 있다. codex는 이 규칙을 모르니 매번 프롬프트에
넣어준다:

```
- git add / git commit 시 파일을 절대 와일드카드나 -A/. 로 넣지 말고, 아래 파일 목록만 pathspec으로
  명시할 것: <files_modified 목록>
- 이 목록 밖의 파일(예: 이미 워킹트리에 있는 다른 미커밋 변경)은 절대 스테이징/커밋하지 말 것
- 태스크 하나 = 커밋 하나. 커밋 메시지는 conventional commits 형식(feat/fix/refactor/docs 등)
```

### 2-1. 컴파일 게이트 (커밋 전 필수)

이 저장소에는 `.editorconfig`/린터 설정이 없어서 `dotnet format` 같은 자동 스타일 수정기는 쓰지 않는다
(설정 없이 돌리면 관련 없는 코드까지 재포맷하고, 이 프로젝트는 인코딩 훼손 이력까지 있어 위험하다).
대신 **컴파일만 확인하는 안전한 게이트**를 태스크 커밋 직전에 넣는다 — 파일을 고치지 않고 문법/타입
오류만 걸러내므로 부작용이 없다:

```
각 태스크의 코드 편집이 끝나면 커밋 전에 다음으로 컴파일 여부만 확인할 것 (스타일 수정 아님):
dotnet build "Projeect_A.E.sln" --nologo -v quiet
컴파일 에러가 있으면 그 태스크를 커밋하지 말고 먼저 고칠 것.
```

### 3. 비대화형(1회성) 실행 — 기본 권장

`--approve-for-me` 는 agy의 `--mode accept-edits`에 대응한다 — workspace-write 샌드박스로 파일 편집은
자동 승인하되, 그 밖의 위험한 동작은 자동 리뷰를 거친다. `-s`(`--sandbox`)와 `--approve-for-me`는
동시에 쓸 수 없다(codex CLI가 거부함) — `--approve-for-me`만 쓴다.

```bash
codex exec \
  -m gpt-5.6-sol \
  -c model_reasoning_effort=medium \
  --approve-for-me \
  -C "C:\Users\chang\Downloads\Projeect_A.E (2) (1)\Projeect_A.E" \
  "다음 파일들을 먼저 Read 해서 참고할 것: <0번에서 추출한 @경로 목록>

$(cat ".planning/phases/<N>-<phase-name>/<N>-0X-PLAN.md")

위 계획을 그대로 실행해줘. 계획에 없는 리팩토링/추상화는 추가하지 말고, 계획에 명시된 파일만 수정해줘.

git 규율: git add/commit 시 -A 나 . 을 쓰지 말고 반드시 아래 pathspec만 사용할 것: <files_modified 목록>
이 목록 밖의 파일은 절대 커밋하지 말 것. 태스크 하나 = 커밋 하나.

컴파일 게이트: 각 태스크 커밋 전에 'dotnet build \"Projeect_A.E.sln\" --nologo -v quiet' 로 컴파일만
확인할 것 (스타일 수정 아님). 실패하면 커밋하지 말고 먼저 고칠 것.

에러 대응: 단순 문법/타입/빌드 에러는 태스크당 최대 3회까지 스스로 고쳐서 계속 진행할 것.
3회를 넘겨도 안 풀리거나, 새 테이블/스키마 추가·라이브러리 교체·기존 구조 변경처럼 계획 범위를
벗어나는 판단이 필요하면 그 자리에서 멈추고 무엇이 막혔는지 보고할 것 (직접 결정해서 밀어붙이지 말 것).
보고할 때 에러 로그는 전체를 붙이지 말고 마지막 10줄만 포함할 것."
```

`-C`로 프로젝트 루트를 명시한다 — codex는 어느 작업 디렉터리에서 호출되든 이 프로젝트를 대상으로
동작해야 한다.

완전 무인 실행이 필요하면 `--dangerously-bypass-approvals-and-sandbox`로 대체할 수 있지만,
**사용자 승인 없이 이 플래그를 임의로 켜지 않는다** — 비가역적일 수 있는 작업이므로 처음 실행하거나
이 플래그를 쓸 때는 먼저 사용자에게 확인한다.

### 4. 이어서 진행 (같은 세션 컨텍스트 유지)

```bash
codex exec resume --last "다음 태스크 계속 진행해줘"
```

### 5. 코드 리뷰 목적 (선택)

계획 실행 자체가 아니라 codex에게 별도로 diff 리뷰를 맡기고 싶을 때는 `codex review`를 쓴다 — 이 경우
표의 "아키텍처급 판단" 행에 준해 `high` effort를 권장한다.

## codex가 막혔을 때 (에러 에스컬레이션)

- **단순 문법/타이포/빌드 에러**: codex가 직접 고치도록 둔다 (위 프롬프트의 "3회 시도" 규칙).
- **로직/아키텍처급 문제** (계획에 없는 구조 변경이 필요해 보임, 계획의 전제가 틀렸음을 발견함,
  3회 넘게 같은 에러가 반복됨): codex를 더 붙잡고 있지 말고, codex가 보고한 에러 로그만 가지고
  Claude Code로 돌아온다. Claude Code가 원인을 분석해 PLAN.md를 갱신(또는 사용자에게 트레이드오프
  제시)한 뒤, 갱신된 내용으로 codex를 다시 호출한다 — 이때는 위 표의 `high` effort로 재위임한다.
  (GSD 자체의 `deviation_rules` Rule 4 "아키텍처 변경은 STOP"과 동일한 기준.)

## 실행 후 체크리스트

- [ ] `git status` / `git diff`로 codex가 만든 변경 범위가 플랜과 일치하는지 확인
- [ ] 플랜에 없는 파일이 건드려졌다면 사용자에게 보고
- [ ] 빌드/테스트 통과 확인 (Unity 프로젝트이므로 콘솔 에러 등 확인)
- [ ] `.planning/STATE.md` 및 해당 PLAN.md 체크리스트 갱신
- [ ] 필요하면 `/gsd:verify-work`로 UAT 진행

## 경계

- 기본 워크플로우는 "Claude Code = 설계·검증, codex = 구현"이다.
- 사소한 수정(오타, 한두 줄)이나 사용자가 명시적으로 요청한 경우는 예외로 Claude Code가 직접 고친다.
- codex 실행, 특히 `--dangerously-bypass-approvals-and-sandbox`를 쓰는 무인 실행은 비가역적일 수 있으므로 사용자 확인 없이 먼저 켜지 않는다.
- 존재를 확신할 수 없는 모델 이름을 임의로 지어내 쓰지 않는다 — 목적별 표에 없는 새 목적이 생기면 사용자에게 먼저 확인한다.
