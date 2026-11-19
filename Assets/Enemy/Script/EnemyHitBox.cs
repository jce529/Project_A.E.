using UnityEngine;

/// <summary>
/// 보스나 적의 자식 오브젝트(HitBox)에 부착하여, 
/// 해당 부위가 맞았을 때 부모의 Stats로 데미지를 전달하는 브릿지 클래스입니다.
/// </summary>
public class EnemyHitBox : MonoBehaviour
{
    [SerializeField] private BossStatsSystem _ownerStats;

    private void Awake()
    {
        // 수동 할당되지 않았다면 부모에서 찾음
        if (_ownerStats == null)
            _ownerStats = GetComponentInParent<BossStatsSystem>();
            
        if (_ownerStats == null)
            Debug.LogWarning($"[EnemyHitBox] {gameObject.name}의 부모에게서 BossStatsSystem을 찾을 수 없습니다.");
    }

    public void TakeDamage(DamageInfo info)
    {
        if (_ownerStats != null)
        {
            _ownerStats.TakeDamageInfo(info);
        }
    }
}
