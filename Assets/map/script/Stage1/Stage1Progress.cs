using System;
using UnityEngine;

// Phase 24: 1스테이지 진행 플래그의 단일 창구.
// 저장은 Phase 23 헬퍼(SaveLoadManager.SetGimmick/GetGimmick)에 위임한다. 키 규칙: "s1.{area}.{name}".
// 전금재 기믹 계약: 기믹은 완료/실패 시점에 Stage1Progress.Set(...) 또는
// Stage1Trigger / Stage1SlideResult(UnityEvent 연결)를 호출하면 된다. 기믹 내부는 건드리지 않는다.
public static class Stage1Progress
{
    public const string LowerEntered   = "s1.lower.entered";   // 하층 진입
    public const string PlatformDone   = "s1.platform.done";   // 물 상승 장판 통과
    public const string UpperEntered   = "s1.upper.entered";   // 상층 진입
    public const string SlideSuccess   = "s1.slide.success";   // 워터 슬라이드 성공
    public const string SteppingDone   = "s1.stepping.done";   // 징검다리 통과
    public const string WaterfallDone  = "s1.waterfall.done";  // 폭포 통과
    public const string SplitEntered   = "s1.split.entered";   // 물가르기 구간 진입
    public const string RewardEntered  = "s1.reward.entered";  // 보상방 진입

    // 진행 순서. 가장 뒤의 true 플래그가 현재 단계.
    public static readonly string[] Order =
    {
        LowerEntered, PlatformDone, UpperEntered, SlideSuccess,
        SteppingDone, WaterfallDone, SplitEntered, RewardEntered
    };

    // 플래그 값이 실제로 바뀐 경우에만 발행된다.
    public static event Action<string, bool> OnFlagChanged;

    public static bool Get(string key)
    {
        var slm = SaveLoadManager.Instance;
        return slm != null && slm.GetGimmick(key, false);
    }

    public static void Set(string key, bool value)
    {
        var slm = SaveLoadManager.Instance;
        if (slm == null) { Debug.LogWarning("[Stage1Progress] SaveLoadManager 없음: " + key); return; }
        if (slm.GetGimmick(key, false) == value) return;
        slm.SetGimmick(key, value);
        OnFlagChanged?.Invoke(key, value);
    }

    // 모든 prerequisite 키가 true 인가 (null/빈 배열이면 true).
    public static bool AllSet(string[] keys)
    {
        if (keys == null) return true;
        foreach (var k in keys) if (!string.IsNullOrEmpty(k) && !Get(k)) return false;
        return true;
    }

    // 현재 단계 인덱스 (-1 = 시작 전). 디버그/UI용.
    public static int CurrentStep()
    {
        for (int i = Order.Length - 1; i >= 0; i--) if (Get(Order[i])) return i;
        return -1;
    }

    // 슬라이드 실패: 하층으로 돌아가므로 장판·상층·슬라이드 진행을 되돌려 구간을 다시 풀게 한다.
    public static void ResetToLower()
    {
        Set(SlideSuccess, false);
        Set(UpperEntered, false);
        Set(PlatformDone, false);
    }
}
