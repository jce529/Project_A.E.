using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

// 개별 키 설정 버튼에 부착하는 스크립트
public class RebindButton : MonoBehaviour
{
    [Header("설정")]
    public string actionName; // 예: "Jump", "Fire"

    [Tooltip("바꾸려는 키의 순서 (0:주 키, 1:보조 키 등)")]
    public int bindingIndex = 0;

    [Header("UI 연결")]
    public TMP_Text bindingText; // 현재 키를 보여줄 텍스트 (예: "Space")

    private InputAction targetAction;
    private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

    void Start()
    {
        // InputHandler가 초기화되지 않았다면 중단
        if (InputHandler.Instance == null) return;

        // 이름으로 액션 찾기
        targetAction = InputHandler.Instance.GetAction(actionName);

        // 시작하자마자 현재 설정된 키 표시
        UpdateUIText();
    }

    // 버튼 클릭 시 호출 (리바인딩 시작)
    public void OnClickStartRebinding()
    {
        if (targetAction == null)
        {
            Debug.LogError($"[RebindButton] '{actionName}' 액션을 찾을 수 없습니다.");
            return;
        }

        targetAction.Disable();
        bindingText.text = "입력 대기중..."; // Press any key...

        rebindingOperation = targetAction.PerformInteractiveRebinding(bindingIndex)
    // 마우스 전체를 끄는 대신, '마우스 위치'와 '마우스 이동'만 제외
                             .WithControlsExcluding("<Pointer>/position")
                             .WithControlsExcluding("<Pointer>/delta")
                             .OnMatchWaitForAnother(0.1f)
                             .OnComplete(operation => FinishRebinding())
                             .Start();
    }

    private void FinishRebinding()
    {
        rebindingOperation.Dispose(); // 메모리 정리
        targetAction.Enable();        // 액션 다시 활성화
        UpdateUIText();               // UI 갱신

        // 변경사항 저장 요청
        InputHandler.Instance.SaveBindingOverrides();
    }

    private void UpdateUIText()
    {
        if (targetAction == null) return;

        // 현재 바인딩된 키 이름을 사람이 읽기 쉬운 문자열로 변환
        string keyName = InputControlPath.ToHumanReadableString(
            targetAction.bindings[bindingIndex].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        bindingText.text = keyName;
    }
}