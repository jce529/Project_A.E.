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
            hp.TakeDamage(dmg);
        }
    }
}
