using UnityEngine;

// 1. PlayerStats 클래스를 상속받습니다.
public class HP : PlayerStats
{
    // 2. Heal 메서드를 override하여 아무것도 하지 않도록 비워둡니다.
    public override void Heal(float health)
    {
        // 힐 기능을 제거했으므로 아무 코드도 작성하지 않습니다.
    }

    // 3. AddHealth 메서드도 override하여 아무것도 하지 않도록 비워둡니다.
    public override void AddHealth()
    {
        // 최대 체력 증가 기능을 제거했으므로 아무 코드도 작성하지 않습니다.
    }

    // 4. TakeDamage 메서드를 override하여 새로운 기능을 추가합니다.
    public override void TakeDamage(float dmg)
    {
        // base.TakeDamage(dmg)를 호출하여 부모 클래스의 데미지 처리 로직을 그대로 실행합니다.
        base.TakeDamage(dmg);

        // 부모 로직 실행 후, 체력이 0 이하인지 확인하여 Die() 메서드를 호출합니다.
        if (Health <= 0)
        {
            Die();
        }
    }

    // 오브젝트 파괴 로직
    public void Die()
    {
        // 이 스크립트가 붙어있는 게임 오브젝트를 파괴합니다.
        Destroy(gameObject);
        Debug.Log(gameObject.name + " has been destroyed.");
    }
}