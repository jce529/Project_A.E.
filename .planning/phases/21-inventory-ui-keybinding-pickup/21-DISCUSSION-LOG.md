# Phase 21: 인벤토리 UI + 키 바인딩 + 월드 아이템 획득 피드백 - Discussion Log

> 감사 추적용. 계획/실행 에이전트의 입력은 21-CONTEXT.md다.

**Date:** 2026-11-05
**Phase:** 21-inventory-ui-keybinding-pickup
**Areas discussed:** 토글 키, 아이템 표시 스키마, 열림 상태 동작, 획득 피드백, 아이템 사용, UI 구성 방식

사전 코드 조사로 이미 정해져 재질문하지 않은 것: 슬롯 20개/스택 99, `List<InventorySlot>` 구조,
interact 키 획득, 가득 참 시 월드 유지, `GameState.Inventory` 존재, 사용 중인 키 목록.

---

## 토글 키
| Option | Selected |
|---|---|
| I 키 (Recommended) | |
| Tab 키 | yes |
| B 키 | |

## 아이템 표시 스키마
| Option | Selected |
|---|---|
| icon + displayName 추가 (Recommended) | yes |
| displayName만 추가 | |
| 스키마 변경 없이 id 표시 | |

## 열림 상태 동작
| Option | Selected |
|---|---|
| 일시정지 + HUD 숨김 (Recommended) | yes |
| 실시간 진행 오버레이 | |

## 획득 피드백
| Option | Selected |
|---|---|
| 토스트 + 가득참 메시지 (Recommended) | yes |
| 획득 토스트만 | |
| 월드 플로팅 텍스트 | |

## 아이템 사용
| Option | Selected |
|---|---|
| 슬롯 선택 + 사용 (Recommended) | yes |
| 표시 전용 | |

## UI 구성 방식
| Option | Selected |
|---|---|
| 스크립트 생성 런타임 UI (Recommended) | yes |
| 프리팹/씬 수작업 | |

---

## Claude's Discretion
시각 디자인, 토스트 지속/큐 방식, 사용 제스처(우클릭/더블클릭), 열림 중 입력 차단 방식, 파일 분할.

## 사용자 선택에서 Claude가 파생한 결정 (확인 필요)
- D-05: 인벤토리는 Playing 상태에서만 열림, 열림 중 Esc는 인벤토리가 직접 처리.
- D-09: Progression 아이템은 UI에서 사용 차단(소모 방지).
- D-13: 획득 피드백은 이벤트 방식으로 연결.

## Deferred Ideas
드래그앤드롭, 버리기, 정렬, 툴팁, 퀵슬롯, 사운드, 게임패드.
