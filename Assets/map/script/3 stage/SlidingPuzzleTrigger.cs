using UnityEngine;

public class SlidingPuzzleTrigger : MonoBehaviour
{
    [Header("띄울 슬라이딩 퍼즐 UI 화면")]
    public GameObject puzzleUI;

    [Header("잠겨있는지 여부 (2층은 끄고, 3층은 체크!)")]
    public bool isLocked = false;

    private bool isPlayerNearby = false;

    // ==================================================================================
    // 1. InputHandler 이벤트 연결 (구독)
    // ==================================================================================
    private void OnEnable()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInteractEvent += HandleInteractInput;
            InputHandler.Instance.OnPauseEvent += HandlePauseInput; // ESC(일시정지) 이벤트 추가 구독
        }
    }

    private void OnDisable()
    {
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.OnInteractEvent -= HandleInteractInput;
            InputHandler.Instance.OnPauseEvent -= HandlePauseInput; // 이벤트 해제
        }
    }

    // ==================================================================================
    // 2. 입력 처리 로직
    // ==================================================================================
    private void HandleInteractInput()
    {
        if (isPlayerNearby)
        {
            Interact();
        }
    }

    // ESC 키를 눌렀을 때 실행될 함수
    private void HandlePauseInput()
    {
        // 만약 퍼즐 UI가 켜져 있는 상태라면, ESC를 눌렀을 때 창을 닫습니다.
        if (puzzleUI != null && puzzleUI.activeSelf)
        {
            puzzleUI.SetActive(false);
            Debug.Log("퍼즐 창을 닫습니다.");
        }
    }

    public void Interact()
    {
        if (isLocked)
        {
            Debug.Log("아직 작동하지 않습니다. 먼저 주변 수로를 연결하세요!");
            return;
        }

        puzzleUI.SetActive(true); // 퍼즐 창 띄우기
        Debug.Log("슬라이딩 퍼즐 시작!");
    }

    public void OnPuzzleCleared()
    {
        puzzleUI.SetActive(false); // 팝업창 닫기
        Debug.Log("퍼즐 클리어! 다음 구역 개방!");

        OpengameManager.instance.isMap3Open = true;
        OpengameManager.instance.CheckMap5Condition();
    }

    // ==================================================================================
    // 3. 플레이어 접근 감지
    // ==================================================================================
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("장치에 접근했습니다. 상호작용 키를 누르세요.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}