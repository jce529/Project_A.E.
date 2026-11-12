using System;
using UnityEngine;

/// <summary>망원경. 상호작용 시 정적 관측 화면(이미지 패널)을 열고 닫는다. 한 번이라도 보면 Viewed 저장.</summary>
public class Telescope : MonoBehaviour, IPlayerInteractable
{
    [Tooltip("정적 관측 화면(이미지) 패널")]
    [SerializeField] private GameObject viewPanel;

    public bool Viewed { get; private set; }
    public bool IsViewing { get; private set; }
    public event Action<bool> Changed;

    private void Start()
    {
        if (viewPanel != null) viewPanel.SetActive(false);
        Viewed = Gimmick2Store.Get(Gimmick2Store.TelescopeViewed);
    }

    public bool CanInteract(PlayerInteraction player) => !IsViewing;
    public void Interact(PlayerInteraction player) => Open();

    public void Open()
    {
        if (IsViewing) return;
        IsViewing = true;
        if (viewPanel != null) viewPanel.SetActive(true);
        Gimmick2Store.SetPuzzleInput(true);
        if (InputHandler.Instance != null) InputHandler.Instance.OnPauseEvent += Close;
        if (!Viewed)
        {
            Viewed = true;
            Gimmick2Store.Set(Gimmick2Store.TelescopeViewed, true);
            Changed?.Invoke(true);
        }
    }

    /// <summary>닫기 버튼(UnityEvent) 또는 Pause/취소 입력에서 호출.</summary>
    public void Close()
    {
        if (!IsViewing) return;
        IsViewing = false;
        if (viewPanel != null) viewPanel.SetActive(false);
        Gimmick2Store.SetPuzzleInput(false);
        if (InputHandler.Instance != null) InputHandler.Instance.OnPauseEvent -= Close;
    }
}
