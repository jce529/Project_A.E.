public interface IAttackStrategy
{
    void ExecuteAttack(BossController boss);
    float Cooldown { get; }
    string AnimationName { get; } // 애니메이션 확인용 (유지)

    // [삭제됨] float WaterCost { get; } 
}