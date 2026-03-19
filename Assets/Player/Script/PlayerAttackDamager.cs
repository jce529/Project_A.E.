using UnityEngine;

/// <summary>
/// Damager를 상속받아, 플레이어의 공격(무기, 주먹 등)이 적에게 닿았을 때 데미지를 주는 클래스.
/// 이 스크립트는 플레이어의 공격 판정을 위한 히트박스(Hitbox) 게임 오브젝트에 부착합니다.
/// </summary>
public class PlayerAttackDamager : Damager
{
    [Header("플레이어 공격 설정")]
    [Tooltip("이 공격이 적에게 입힐 데미지 양")]
    public PlayerAttack playerAttack;



    /// <summary>
    /// 부모 클래스(Damager)의 추상 메서드를 구현합니다.
    /// 히트박스가 유효한 타겟(HP 컴포넌트가 있고, 지정된 레이어에 속한)과 닿으면 이 메서드가 호출됩니다.
    /// </summary>
    /// <param name="targetHP">데미지를 받을 대상의 HP 컴포넌트 (이 경우 적의 HP)</param>
    protected override void ApplyDamageEffect(HP targetHP)
    {
        // 대상(적)에게 설정된 만큼의 공격 데미지를 입힙니다.
        targetHP.TakeDamage(playerAttack.damage);

        // (선택 사항) 여기에 추가적인 효과를 넣을 수 있습니다.
        // 예: 타격 이펙트 생성, 사운드 재생 등
        // Instantiate(hitEffectPrefab, targetHP.transform.position, Quaternion.identity);
        // SoundManager.Instance.PlaySound("HitSound");
    }
}