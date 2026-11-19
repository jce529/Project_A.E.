# Quick: 세이브 포인트 / ESC 저장 슬롯 선택

1. 공통 런타임 저장 선택창: 3개 슬롯의 진행 정보, 빈 슬롯 저장, 기존 슬롯 덮어쓰기 확인, 취소 및 실패 안내.
2. Checkpoint.Interact와 GameSettingsPanel.OnSaveProgressBtnClick을 연결한다. 성공한 슬롯을 CurrentSlot으로 유지하고, 취소 시 저장/체크포인트 변경 없이 이전 게임 상태로 돌아간다.
3. 컴파일 및 가능한 비파괴 검증 후 변경 파일만 커밋한다. STATE 기존 수정은 보존한다.

씬마다 Inspector 연결을 추가하지 않도록 런타임 Canvas로 생성한다. 기존 메인메뉴의 Load/NewGame 동작은 유지한다. 체크포인트 회복 및 ESC 저장의 기존 위치 정책을 그대로 사용한다.
