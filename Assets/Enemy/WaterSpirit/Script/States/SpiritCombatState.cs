using UnityEngine;

public class SpiritCombatState : CombatState
{
    private static readonly System.Func<IAttackStrategy>[] _pattern = new System.Func<IAttackStrategy>[]
    {
        () => new SpiritCharge(),        // 1. 돌진 (텔레포트 → 관통 돌진)
        () => new SpiritExhaustion(),    // 2. 지침 (취약, 정지)
        () => new SpiritWakeRepel(),     // 3. 깨어나면서 밀쳐내기
        () => new SpiritFarProjectile(), // 4. 순간이동(2배 거리) + 원거리 투사체
        () => new SpiritCharge(),        // 5. 순간이동 + 돌진
    };

    private int _patternIndex;

    public override void Enter(BossController boss)
    {
        base.Enter(boss);
        _patternIndex = 0;
        Debug.Log("[SpiritCombatState] 정령 보스 전투 상태 진입");
    }

    protected override bool ShouldTransitionToGroggy(BossController boss) => false;

    protected override IAttackStrategy SelectAttackStrategy(BossController boss, float dist)
    {
        if (!(boss is SpiritController)) return null;

        IAttackStrategy attack = _pattern[_patternIndex]();
        Debug.Log($"[SpiritCombatState] 패턴 {_patternIndex + 1}/{_pattern.Length}: {attack.GetType().Name}");
        _patternIndex = (_patternIndex + 1) % _pattern.Length;
        return attack;
    }
}
