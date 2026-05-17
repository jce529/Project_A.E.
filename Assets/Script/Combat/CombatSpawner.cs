using UnityEngine;

// GEMINI.md의 '극단적 단순성(Extreme Simplicity)' 원칙을 준수하여 구현된 생성 도구입니다.
// 복잡한 인터페이스나 추상화 없이, 최소한의 로직으로 분신과 투사체를 생성합니다.
public static class CombatSpawner
{
    private static GameObject _projectilePrefab;

    /// <summary>
    /// 지정된 위치와 방향으로 물 투사체를 생성합니다.
    /// </summary>
    public static void SpawnProjectile(Vector3 position, Vector3 direction)
    {
        if (_projectilePrefab == null)
        {
            _projectilePrefab = Resources.Load<GameObject>("WaterSpitProjectile");
        }

        if (_projectilePrefab == null)
        {
            Debug.LogWarning("[CombatSpawner] 'WaterSpitProjectile' 프리팹을 Resources에서 찾을 수 없습니다.");
            return;
        }

        GameObject go = Object.Instantiate(_projectilePrefab, position, Quaternion.identity);
        var proj = go.GetComponent<WaterSpitProjectile>();
        if (proj != null)
        {
            proj.Direction = direction;
        }
    }

    /// <summary>
    /// 보스의 분신(Clone)을 생성하고 필요한 설정을 수행합니다.
    /// </summary>
    public static SpiritController SpawnClone(GameObject prefab, Vector3 position, bool isDummy)
    {
        if (prefab == null) return null;

        GameObject go = Object.Instantiate(prefab, position, Quaternion.identity);
        
        var stats = go.GetComponent<SpiritStats>();
        if (stats != null)
        {
            stats.IsDummy = isDummy;
        }

        var controller = go.GetComponent<SpiritController>();
        if (controller == null)
        {
            Debug.LogWarning($"[CombatSpawner] 생성된 객체({go.name})에 SpiritController가 없습니다.");
        }

        return controller;
    }
}
