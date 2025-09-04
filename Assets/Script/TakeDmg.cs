using UnityEngine;

public class GiveDmg : MonoBehaviour
{
    GameObject Target;
    public HP hp;
    
    public void DealtoTarget(GameObject target, float dmg)
    {
        Target = target;
        hp = Target.GetComponent<HP>();
        if (hp != null)
        {
            hp.takeDmg(dmg);
            Debug.Log("공격 성공! 데미지: " + dmg);
        }
    }
}
