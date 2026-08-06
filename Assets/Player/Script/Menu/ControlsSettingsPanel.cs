using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;

// 일시정지 > 컨트롤 탭: 키 리바인딩 UI
// Inspector에서 각 RebindEntry를 채워 버튼과 액션을 연결합니다.
//
// Move 컴포짓 바인딩 인덱스 참고:
//   0 = 컴포짓 헤더 (리바인딩 불가)
//   1 = up (W), 2 = down (S), 3 = left (A), 4 = right (D)
// 단일 버튼 액션은 bindingIndex = 0
// BasicAttack은 0 = 마우스 좌클릭, 1 = 키보드 F
public class ControlsSettingsPanel : MonoBehaviour
{
    [System.Serializable]
    public class RebindEntry
    {
        public string   actionName;    // InputAction 이름 (예: "Jump", "Move")
        public int      bindingIndex;  // action.bindings[n] 인덱스
        public Button   rebindButton;  // 클릭하면 리바인딩 시작
        public TMP_Text keyLabel;      // 현재 키 이름을 표시하는 텍스트
    }

    [Header("리바인드 항목 목록")]
    public List<RebindEntry> entries = new List<RebindEntry>();

    [Header("리바인딩 대기 오버레이")]
    public GameObject listeningOverlay; // "아무 키나 누르세요" 안내 패널

    [Header("버튼")]
    public Button resetButton;

    private InputActionRebindingExtensions.RebindingOperation _rebindOp;

    private void Start()
    {
        foreach (var entry in entries)
        {
            var captured = entry;
            entry.rebindButton?.onClick.AddListener(() => StartRebind(captured));
        }
        resetButton?.onClick.AddListener(ResetAllBindings);
    }

    private void OnEnable() => RefreshAllLabels();

    private void OnDisable()
    {
        _rebindOp?.Cancel();
        _rebindOp?.Dispose();
        _rebindOp = null;
    }

    // ── 리바인딩 시작 ──────────────────────────────────────────────
    private void StartRebind(RebindEntry entry)
    {
        var action = GetAction(entry.actionName);
        if (action == null) return;

        action.Disable();
        listeningOverlay?.SetActive(true);

        _rebindOp = action
            .PerformInteractiveRebinding(entry.bindingIndex)
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .OnMatchWaitForAnother(0.1f)
            .OnComplete(_ => FinishRebind(entry, action))
            .OnCancel(_  => CancelRebind(action))
            .Start();
    }

    private void FinishRebind(RebindEntry entry, InputAction action)
    {
        CleanupOp();
        action.Enable();
        listeningOverlay?.SetActive(false);
        RefreshLabel(entry);
        InputHandler.Instance?.SaveBindingOverrides();
    }

    private void CancelRebind(InputAction action)
    {
        CleanupOp();
        action.Enable();
        listeningOverlay?.SetActive(false);
    }

    private void CleanupOp()
    {
        _rebindOp?.Dispose();
        _rebindOp = null;
    }

    // ── 기본값 초기화 ──────────────────────────────────────────────
    private void ResetAllBindings()
    {
        foreach (var entry in entries)
            GetAction(entry.actionName)?.RemoveAllBindingOverrides();
        InputHandler.Instance?.SaveBindingOverrides();
        RefreshAllLabels();
    }

    // ── 라벨 갱신 ─────────────────────────────────────────────────
    private void RefreshAllLabels()
    {
        foreach (var entry in entries)
            RefreshLabel(entry);
    }

    private void RefreshLabel(RebindEntry entry)
    {
        if (entry.keyLabel == null) return;
        var action = GetAction(entry.actionName);
        entry.keyLabel.text = action != null
            ? action.GetBindingDisplayString(entry.bindingIndex)
            : "?";
    }

    private InputAction GetAction(string actionName)
        => InputHandler.Instance?.GetAction(actionName);
}
