using System.Collections;
using UnityEngine;
using WaterMonster.Phase2;

/// <summary>
/// WaterTeleportState — IBossState implementation.
/// The boss teleports to an indestructible puddle as a positioning pattern.
/// D-07: Selects target puddle inversely proportional to distance from player.
/// D-11: REQ-WM-02 HP cost applied.
/// D-15: Selects target + HP cost + starts VFX coroutine in Enter, returns to CombatState upon completion.
/// </summary>
public class WaterTeleportState : IBossState
{
    private const float HpCostPercent = 0.05f;       // Teleport HP cost (5% of MaxHealth)
    private const float CloseRangeThreshold = 3.0f;  // Reuse CombatState's dist <= 3.0f threshold (D-08)
    private const float TeleportDelay = 0.25f;       // Delay between disappearance and reappearance (D-09)

    private Coroutine _routine;

    public void Enter(BossController boss)
    {
        // D-11, REQ-WM-02: Deduct HP cost
        if (boss.Stats is WaterMonsterStats wms)
        {
            wms.SpendHpCost(wms.MaxHealth * HpCostPercent);
        }

        // D-07: Select teleport target
        WaterPuddle target = SelectTeleportTarget(boss);
        if (target == null)
        {
            // If no target found, return to CombatState immediately
            boss.ChangeState(new WaterMonsterCombatState());
            return;
        }

        // Record teleport time for cooldown (stored in WaterMonsterController)
        if (boss is WaterMonsterController wmc)
        {
            wmc.RecordTeleportTime();
        }

        // Start teleport coroutine via boss (IBossState is not a MonoBehaviour)
        _routine = boss.StartCoroutine(TeleportSequence(boss, target));
    }

    public void Execute(BossController boss)
    {
        // Coroutine handles everything
    }

    public void Exit(BossController boss)
    {
        // Pitfall 2: Clean up coroutine if state is forced to change
        if (_routine != null)
        {
            boss.StopCoroutine(_routine);
            _routine = null;
        }
        
        // Restore SpriteRenderer (in case it was disabled during VFX)
        var sr = boss.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    private IEnumerator TeleportSequence(BossController boss, WaterPuddle target)
    {
        var sr = boss.GetComponent<SpriteRenderer>();

        // D-09: Disappearance VFX — simple SpriteRenderer.enabled = false
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(TeleportDelay);

        // Move position
        boss.transform.position = target.transform.position;

        // D-09: Reappearance VFX — simple SpriteRenderer.enabled = true
        if (sr != null) sr.enabled = true;

        yield return new WaitForSeconds(TeleportDelay);

        _routine = null;

        // D-10: Return to WaterMonsterCombatState immediately after arrival
        // Note: Use WaterMonsterCombatState directly to avoid swap loops
        boss.ChangeState(new WaterMonsterCombatState());
    }

    /// <summary>
    /// D-07: Select target puddle inversely proportional to distance from player.
    /// Close (dist <= 3.0f) -> Farthest puddle from player
    /// Far (dist > 3.0f) -> Nearest puddle to player
    /// </summary>
    private WaterPuddle SelectTeleportTarget(BossController boss)
    {
        if (PuddleStackManager.Instance == null) return null;

        var puddles = PuddleStackManager.Instance.IndestructiblePuddles;
        if (puddles == null || puddles.Count == 0) return null;

        float distToPlayer = Vector2.Distance(boss.transform.position, boss.Target.position);
        bool isClose = distToPlayer <= CloseRangeThreshold;

        WaterPuddle selected = null;
        float bestDist = isClose ? float.MinValue : float.MaxValue;

        foreach (var puddle in puddles)
        {
            if (puddle == null || !puddle.gameObject.activeSelf) continue;

            float d = Vector2.Distance(boss.Target.position, puddle.transform.position);

            if (isClose)
            {
                // If player is close, move to the farthest puddle
                if (d > bestDist) { bestDist = d; selected = puddle; }
            }
            else
            {
                // If player is far, move to the nearest puddle
                if (d < bestDist) { bestDist = d; selected = puddle; }
            }
        }

        return selected;
    }
}
