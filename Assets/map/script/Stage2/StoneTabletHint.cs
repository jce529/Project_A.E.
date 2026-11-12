using System;
using UnityEngine;

/// <summary>석판. 지정 구역 적이 전멸한 뒤에만 힌트 UI를 열 수 있고, 닫은 뒤 다시 열 수 있다.</summary>
public class StoneTabletHint : MonoBehaviour, IPlayerInteractable
{
    [Tooltip("전멸시켜야 할 구역의 적 HP 목록")]
    [SerializeField] private HP[] areaEnemies;
    [SerializeField] private GameObject hintPanel;

    public bool Unlocked { get; private set; }
    public bool IsOpen { get; private set; }
    public event Action<bool> Changed;

    private int alive;

    private void Start()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
        if (Gimmick2Store.Get(Gimmick2Store.TabletUnlocked)) { Unlocked = true; return; }
        if (areaEnemies == null || areaEnemies.Length == 0) return; // 적 미지정이면 잠금 유지
        foreach (var hp in areaEnemies)
        {
            if (hp == null) continue;
            alive++;
            hp.OnDeath += OnEnemyDeath;
        }
        if (alive == 0) Unlock(); // 이미 전부 파괴됨
    }

    private void OnDestroy()
    {
        if (areaEnemies == null) return;
        foreach (var hp in areaEnemies) if (hp != null) hp.OnDeath -= OnEnemyDeath;
    }

    private void OnEnemyDeath()
    {
        if (--alive <= 0) Unlock();
    }

    private void Unlock()
    {
        if (Unlocked) return;
        Unlocked = true;
        Gimmick2Store.Set(Gimmick2Store.TabletUnlocked, true);
        Changed?.Invoke(true);
    }

    public bool CanInteract(PlayerInteraction player) => Unlocked && !IsOpen;
    public void Interact(PlayerInteraction player) => Open();

    public void Open()
    {
        if (!Unlocked || IsOpen) return;
        IsOpen = true;
        if (hintPanel != null) hintPanel.SetActive(true);
        Gimmick2Store.SetPuzzleInput(true);
        if (InputHandler.Instance != null) InputHandler.Instance.OnPauseEvent += Close;
    }

    /// <summary>닫기 버튼(UnityEvent) 또는 Pause/취소 입력에서 호출.</summary>
    public void Close()
    {
        if (!IsOpen) return;
        IsOpen = false;
        if (hintPanel != null) hintPanel.SetActive(false);
        Gimmick2Store.SetPuzzleInput(false);
        if (InputHandler.Instance != null) InputHandler.Instance.OnPauseEvent -= Close;
    }
}
