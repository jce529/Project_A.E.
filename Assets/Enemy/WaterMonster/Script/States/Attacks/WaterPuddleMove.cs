using WaterMonster.Phase2;

public class WaterPuddleMove : IAttackStrategy
{
    public float Cooldown => 1f;
    public string AnimationName => WaterMonsterPatterns.PuddleMove;

    public void ExecuteAttack(BossController boss)
    {
        if (boss is not WaterMonsterController wmc || wmc.Target == null) return;
        var system = WaterMonsterPuddleSystem.Instance;
        if (system == null || !system.TrySelectMove(wmc.transform.position, wmc.Target.position,
                out WaterPuddle entry, out WaterPuddle exit)) return;

        boss.ChangeState(new WaterTeleportState(entry, exit));
    }
}