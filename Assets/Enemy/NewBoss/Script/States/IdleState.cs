using System.Collections.Specialized;
using UnityEngine;

public class IdleState : IBossState
{
    public void Enter(BossController boss)
    {
        // 보스가 대기 상태에 들어갈 때 필요한 초기화 작업 수행
    }

    public void Execute(BossController boss)
    {
        //컷씬 종료 혹은 플레이어 인식 시 추적 상태로 전환
        if (boss.TargetFound)
        {
            boss.ChangeState(new ChaseState());
        }
    }

    public void Exit(BossController boss)
    {
        // 보스가 대기 상태에서 나올 때 필요한 정리 작업 수행
    }

}

