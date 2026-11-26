# 모델 라우팅 로그

`jev-model-routing` 스킬 §9 의 관측 기록. 작업(plan 또는 task)이 끝날 때마다 **한 줄**을 아래 표 끝에 추가한다.
배정을 *결정한* 증거는 plan frontmatter 의 `routing:` 블록이고, 이 로그는 *결과*(1차 통과, 에스컬레이션, 폴백)를 남긴다.

기록 규칙
- 모델·채널은 실제로 호출한 값을 적는다. Codex 는 실행 로그 첫머리 `model:` 줄과 요청 모델이 같은지 확인하고 `model확인=OK|불일치`를 비고에 적는다.
- Jev 를 쓰지 못했으면 `classifier: fallback`, 쓴 경우 `jev_ask`로 얻은 복잡도/위험도를 적는다.
- 한도 초과 폴백은 `codex 한도 → <argon|claude 모델> 폴백(행 ①~④), 해제 시각 <시각>` 형식으로 비고에 적는다.
- 위임 없이 직접 작성한 경우도 `직접(claude)`로 한 줄 남긴다(라우팅 미적용 사유 포함).
- 이 파일 외 STATE/ROADMAP 은 총괄만 수정한다.

| 날짜 | 작업 ID | 복잡도/위험도 | 티어 | 모델(채널) | 1차 통과 | 에스컬레이션 횟수 | 검증자 지적 수 | routing_miss | 비고 |
|---|---|---|---|---|---|---|---|---|---|
| 2026-11-12 | 23-01 | 미기록 | - | claude (직접) | - | 0 | - | - | 세션이 `jev_ask`·plan `routing:` 블록을 남기지 않음(소급 기록, 라우팅 적용 여부 확인 불가) |
| 2026-11-12 | 24-01 | 미기록 | - | claude (직접) | - | 0 | - | - | 세션 보고: Codex/Argon 위임 없음, 스킬 파일 미열람 |
| 2026-11-12 | 25-01 | 미기록 | - | claude (직접) | - | 0 | - | - | 세션 보고: 위임 없음, 스킬 파일 미열람 |
| 2026-11-12 | 26-01 | 미기록 | - | claude (sonnet, 직접) | - | 0 | - | - | 세션 보고: Codex 없이 sonnet 직접 작성, 스킬 파일 미열람 |
| 2026-11-12 | 27 | 미기록 | - | claude (직접) | - | 0 | - | - | `routing:` 블록 미확인 |
| 2026-11-12 | 28-01 | 기록됨(plan frontmatter) | - | claude | - | 0 | - | - | 28-01-PLAN.md 에만 `routing:` 블록 존재(유일한 정식 기록) |
| 2026-11-19 | quick-stage12-whitebox/스크립트 | multi_file/low | T3 | codex gpt-6-sol (medium) | N (1차 무출력) | 1 | - | - | classifier: jev(P1=0.42 경계, P2=0.12, P3=0.64, P4=0.53), 1차 호출은 탐색만 하고 파일 미작성 → 같은 모델 재시도로 성공, model확인=OK, 컴파일 오류 0, Reach 사다리 연결 1곳은 sonnet 이 패치 |
| 2026-11-19 | quick-stage12-whitebox/SPEC 검수 | - | T3 | gemini-4-argon-medium (agy plan) | Y | 0 | 2 | - | 검수 전용, 실제 결함 2건 반영, model확인=해당없음(agy) |
| 2026-11-19 | quick-stage12-whitebox/PDF 읽기·씬 작업·캡처 | - | T3 | claude sonnet (직접) | - | 0 | - | - | 비전·Unity CLI 다단계, 에디터 크래시 1회 후 재기동 |
