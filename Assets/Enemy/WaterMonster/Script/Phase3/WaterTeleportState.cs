using System.Collections;
using UnityEngine;
using WaterMonster.Phase2;

/// <summary>
/// WaterTeleportState — IBossState implementation.
/// The boss enters the nearest puddle, sinks, and re-forms at another puddle (puddle move pattern).
/// D-04: No HP cost. D-06: Entry/exit puddles come from WaterMonsterPuddleSystem.TrySelectMove.
/// </summary>
public class WaterTeleportState : IBossState
{
    private const float TeleportDelay = 0.25f;       // Delay between disappearance and reappearance (D-09)
    private const float EnterTimeout = 2f;           // Max time to walk to the entry puddle
    private const float EnterReachX = 0.5f;          // x distance treated as "inside" the entry puddle

    private WaterPuddle _entry;
    private WaterPuddle _exit;
    private Coroutine _routine;

    public WaterTeleportState() { }

    public WaterTeleportState(WaterPuddle entry, WaterPuddle exit)
    {
        _entry = entry;
        _exit = exit;
    }

    public void Enter(BossController boss)
    {
        if (_entry == null || _exit == null)
        {
            var system = WaterMonsterPuddleSystem.Instance;
            if (system != null && boss.Target != null)
            {
                system.TrySelectMove(boss.transform.position, boss.Target.position, out _entry, out _exit);
            }
        }

        // Record first so an empty target still starts the 8s cooldown (no per-frame re-entry loop)
        if (boss is WaterMonsterController wmc)
        {
            wmc.RecordTeleportTime();
        }

        if (_entry == null || _exit == null)
        {
            boss.ChangeState(new WaterMonsterCombatState());
            return;
        }

        WaterMonsterPatterns.TrySetTrigger(boss.Anim, WaterMonsterPatterns.PuddleMove);

        // Start teleport coroutine via boss (IBossState is not a MonoBehaviour)
        _routine = boss.StartCoroutine(TeleportSequence(boss, _entry, _exit));
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

        boss.StopMove();

        // Restore SpriteRenderer (in case it was disabled during VFX)
        var sr = boss.GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = true;
    }

    private IEnumerator TeleportSequence(BossController boss, WaterPuddle entry, WaterPuddle exit)
    {
        var sr = boss.GetComponent<SpriteRenderer>();

        // Walk to the entry puddle (bounded wait)
        float elapsed = 0f;
        while (elapsed < EnterTimeout && entry != null && entry.gameObject.activeInHierarchy
            && Mathf.Abs(boss.transform.position.x - entry.transform.position.x) > EnterReachX)
        {
            boss.MoveTo(entry.transform.position);
            elapsed += Time.deltaTime;
            yield return null;
        }
        boss.StopMove();

        // Sink into the puddle
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(TeleportDelay);

        // Re-form at the exit puddle (keep ground height); stay put if it vanished meanwhile
        if (exit != null && exit.gameObject.activeInHierarchy)
        {
            Vector3 pos = boss.transform.position;
            boss.transform.position = new Vector3(exit.transform.position.x, pos.y, pos.z);
        }

        if (sr != null) sr.enabled = true;

        yield return new WaitForSeconds(TeleportDelay);

        _routine = null;

        // D-10: Return to WaterMonsterCombatState immediately after arrival
        // Note: Use WaterMonsterCombatState directly to avoid swap loops
        boss.ChangeState(new WaterMonsterCombatState());
    }
}
