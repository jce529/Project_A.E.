using System.Collections;
using UnityEngine;

public static class WaterMonsterPatterns
{
    public const string Geyser = "Attack_Geyser";
    public const string Jump = "Attack_Jump";
    public const string Burst = "Attack_Burst";
    public const string Prison = "Attack_Prison";
    public const string PrisonAoe = "Attack_PrisonAoe";
    public const string PuddleSpawn = "Attack_PuddleSpawn";
    public const string PuddleMove = "Attack_PuddleMove";
    public const string PuddleExplode = "Attack_PuddleExplode";
    public const string ColorPrison = "Attack_ColorPrison";
    public const string Wave = "Attack_Wave";
    public const string Groggy = "Groggy";
    public const string PhaseTransition = "PhaseTransition";

    public static readonly string[] AllTriggers =
    {
        Geyser, Jump, Burst, Prison, PrisonAoe, PuddleSpawn,
        PuddleMove, PuddleExplode, ColorPrison, Wave, Groggy, PhaseTransition
    };

    public static bool TrySetTrigger(Animator anim, string trigger)
    {
        if (anim == null || anim.runtimeAnimatorController == null || !anim.isActiveAndEnabled)
            return false;

        foreach (AnimatorControllerParameter parameter in anim.parameters)
        {
            if (parameter.name == trigger && parameter.type == AnimatorControllerParameterType.Trigger)
            {
                anim.SetTrigger(trigger);
                return true;
            }
        }

        return false;
    }

    public static Coroutine StartPattern(BossController boss, IEnumerator routine)
    {
        return boss is WaterMonsterController wmc ? wmc.RunPattern(routine) : boss.StartCoroutine(routine);
    }
}
