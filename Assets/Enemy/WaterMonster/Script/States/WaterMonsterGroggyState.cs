using UnityEngine;

public class WaterMonsterGroggyState : IBossState
{
    private readonly int _targetPhase;
    private readonly bool _empowerOnly;
    private float _timer;
    private bool _transitionStarted;

    public WaterMonsterGroggyState(int targetPhase, bool empowerOnly)
    {
        _targetPhase = targetPhase;
        _empowerOnly = empowerOnly;
    }

    public void Enter(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc))
        {
            boss.ChangeState(new CombatState());
            return;
        }

        boss.StopMove();
        WaterMonsterPatterns.TrySetTrigger(boss.Anim, WaterMonsterPatterns.Groggy);
        wmc.WaterStats.IncomingDamageMultiplier = wmc.GroggyDamageMultiplier;
        _timer = 0f;
    }

    public void Execute(BossController boss)
    {
        if (!(boss is WaterMonsterController wmc)) return;

        _timer += Time.deltaTime;
        if (!_transitionStarted && _timer >= wmc.GroggyDuration)
        {
            _transitionStarted = true;
            WaterMonsterPatterns.TrySetTrigger(boss.Anim, WaterMonsterPatterns.PhaseTransition);
            wmc.WaterStats.IncomingDamageMultiplier = 1f;
        }

        if (_timer >= wmc.GroggyDuration + wmc.PhaseTransitionDuration)
        {
            if (_empowerOnly) wmc.ActivateEnrage();
            else wmc.CompletePhaseTransition(_targetPhase);
            boss.ChangeState(new WaterMonsterCombatState());
        }
    }

    public void Exit(BossController boss)
    {
        if (boss is WaterMonsterController wmc && wmc.WaterStats != null)
            wmc.WaterStats.IncomingDamageMultiplier = 1f;
    }
}
