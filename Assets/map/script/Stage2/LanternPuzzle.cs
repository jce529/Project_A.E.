using System;
using UnityEngine;

/// <summary>석등 3개의 ON/OFF 패턴이 정답과 일치하면 해결 플래그(s2.a.lantern.solved) 저장.</summary>
public class LanternPuzzle : MonoBehaviour
{
    [SerializeField] private LanternSwitch[] lanterns = new LanternSwitch[3];
    [Tooltip("lanterns 와 같은 순서의 정답 ON/OFF 패턴")]
    [SerializeField] private bool[] answer = { true, false, true };

    public bool Solved { get; private set; }
    public event Action<bool> Changed;

    private void Start()
    {
        foreach (var l in lanterns) if (l != null) l.Toggled += OnToggled;
        if (Gimmick2Store.Get(Gimmick2Store.LanternSolved)) ApplySolved(false);
    }

    private void OnDestroy()
    {
        foreach (var l in lanterns) if (l != null) l.Toggled -= OnToggled;
    }

    private void OnToggled(bool _)
    {
        if (!Solved && Matches()) ApplySolved(true);
    }

    private bool Matches()
    {
        if (lanterns.Length == 0 || answer.Length < lanterns.Length) return false;
        for (int i = 0; i < lanterns.Length; i++)
            if (lanterns[i] == null || lanterns[i].IsOn != answer[i]) return false;
        return true;
    }

    private void ApplySolved(bool fire)
    {
        Solved = true;
        for (int i = 0; i < lanterns.Length && i < answer.Length; i++)
        {
            if (lanterns[i] == null) continue;
            lanterns[i].SetOnSilently(answer[i]);
            lanterns[i].Locked = true;
        }
        if (!fire) return;
        Gimmick2Store.Set(Gimmick2Store.LanternSolved, true);
        Changed?.Invoke(true);
    }
}
