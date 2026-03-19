using UnityEngine;

// 보스의 상태를 정의하는 인터페이스
public interface IBossState
{
    void Enter(BossController boss);
    void Execute(BossController boss);
    void Exit(BossController boss);
}
