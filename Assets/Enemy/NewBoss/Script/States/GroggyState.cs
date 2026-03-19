using UnityEngine;

public class GroggyState : IBossState
{
    private float _recoveryTime = 5f;
    private float _timer;

    public void Enter(BossController boss)
    {
        _timer = 0f;
        //애니메이션 : 그로기 모션
        //베리어 해제 이팩트
    }

    public void Execute(BossController boss)
    {
        _timer += Time.deltaTime;
        if (_timer >= _recoveryTime)
        {
            boss.Stats.RestoreWater();
            boss.ChangeState(new CombatState());
        }
    }

    public void Exit(BossController boss)
    {
        //그로기 상태에서 나올 때 필요한 정리 작업 수행
    }
}
