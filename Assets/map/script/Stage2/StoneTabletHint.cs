using System;
using UnityEngine;

/// <summary>석판. 지정 구역 적이 전멸한 뒤에만 힌트 UI를 열 수 있고, 닫은 뒤 다시 열 수 있다.</summary>
public class StoneTabletHint : MonoBehaviour, IPlayerInteractable
{
    [Tooltip("전멸시켜야 할 구역의 적 HP 목록")]
    [SerializeField] private HP[] areaEnemies;
    [SerializeField] private GameObject hintPanel;
    [Header("Gimmick keys (2-2 석판은 s2.b.* 로 변경)")]
    [SerializeField] private string unlockedKey = Gimmick2Store.TabletUnlocked;
    [SerializeField] private string readKey = Gimmick2Store.TabletRead;
    [Tooltip("비어있지 않으면 이 플래그가 true 일 때만 열 수 있음 (예: 2-2 는 s2.b.waterfall.stopped)")]
    [SerializeField] private string requiredFlagKey = "";

    public bool Unlocked { get; private set; }
    public bool IsOpen { get; private set; }
    public event Action<bool> Changed;

    private int alive;

    private void Start()
    {
        if (hintPanel != null) hintPanel.SetActive(false);
        if (Gimmick2Store.Get(unlockedKey)) { Unlocked = true; return; }
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
        Gimmick2Store.Set(unlockedKey, true);
        Changed?.Invoke(true);
    }

    private bool FlagOk() => string.IsNullOrWhiteSpace(requiredFlagKey) || Gimmick2Store.Get(requiredFlagKey);

    public bool CanInteract(PlayerInteraction player) => Unlocked && !IsOpen && FlagOk();
    public void Interact(PlayerInteraction player) => Open();

    public void Open()
    {
        if (!Unlocked || IsOpen || !FlagOk()) return;
        IsOpen = true;
        Gimmick2Store.Set(readKey, true);
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
