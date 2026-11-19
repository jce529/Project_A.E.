using UnityEngine;

public class SlidingPuzzleTrigger : MonoBehaviour, IPlayerInteractable
{
    [Header("띄울 슬라이딩 퍼즐 UI 화면")]
    public GameObject puzzleUI;

    [Header("잠겨있는지 여부 (2층은 끄고, 3층은 체크!)")]
    public bool isLocked = false;

    private InputHandler subscribedInput;
    private void Start() => SubscribePause();
    private void OnEnable() => SubscribePause();
    private void SubscribePause()
    {
        if (subscribedInput == InputHandler.Instance) return;
        OnDisable();
        subscribedInput = InputHandler.Instance;
        if (subscribedInput != null) subscribedInput.OnPauseEvent += HandlePauseInput;
    }
    private void OnDisable()
    {
        if (subscribedInput != null) subscribedInput.OnPauseEvent -= HandlePauseInput;
        subscribedInput = null;
    }

    public bool CanInteract(PlayerInteraction player) => !isLocked && puzzleUI != null;
    public void Interact(PlayerInteraction player) => Interact();

    private void HandlePauseInput()
    {
        // 만약 퍼즐 UI가 켜져 있는 상태라면, ESC를 눌렀을 때 창을 닫습니다.
        if (puzzleUI != null && puzzleUI.activeSelf)
        {
            puzzleUI.SetActive(false);
        }
    }

    public void Interact()
    {
        if (isLocked || puzzleUI == null)
        {
            return;
        }

        puzzleUI.SetActive(true); // 퍼즐 창 띄우기
    }

    public void OnPuzzleCleared()
    {
        puzzleUI.SetActive(false); // 팝업창 닫기

        OpengameManager.instance.isMap3Open = true;
        OpengameManager.instance.CheckMap5Condition();
    }

}
