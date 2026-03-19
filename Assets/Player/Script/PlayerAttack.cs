using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.InputSystem; // New Input System 사용을 위해 추가

/// <summary>
/// 플레이어의 공격 로직 + 입력 감지를 통합한 클래스.
/// 이제 PlayerAttackBase를 상속받아 InputHandler의 이벤트를 받아 처리합니다.
/// </summary>
public class PlayerAttack : PlayerAttackBase // <--- [변경] 상속 클래스 변경
{
    [Header("공격 기본 설정")]
    public float damage = 10f;
    public GameObject attackBox;
    public float attackDistance = 1.5f;
    public float attackDuration = 0.2f;

    [Header("E 스킬 (섬광참) 설정")]
    public float teleportDistance = 3f;
    public GameObject slashHitboxPrefab;

    [Header("R 스킬 (파동참) 설정")]
    public float radius = 2.5f;
    public GameObject waveEffectPrefab;

    [Header("기타 컴포넌트")]
    public WaterController waterController;
    public PlayerStats playerStats;

    void Start()
    {
        if (waterController == null) UnityEngine.Debug.LogError("[PlayerAttack] WaterController가 연결되지 않았습니다! Inspector를 확인하세요.");
        if (playerStats == null) UnityEngine.Debug.LogError("[PlayerAttack] PlayerStats가 연결되지 않았습니다! Inspector를 확인하세요.");

        UnityEngine.Debug.Log("PlayerAttack 초기화 완료.");
    }
    /// <summary>
    /// 기본 공격
    /// </summary>
    protected override void OnBasicAttack()
    {
        // [Tip] New Input System만 활성화된 경우 Input.mousePosition 대신 아래 코드를 사용하세요.
        // Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        // Vector3 mousePos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
        UnityEngine.Debug.Log("기본공격 입력");

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        Vector2 spawnPos = (Vector2)transform.position + direction * attackDistance;

        GameObject atkBox = Instantiate(attackBox, spawnPos, Quaternion.identity);
        atkBox.transform.right = direction;

        StartCoroutine(DisableAfterTime(atkBox, attackDuration));
    }

    /// <summary>
    /// E 스킬 - 섬광참
    /// </summary>
    protected override void OnSkillE()
    {
        UnityEngine.Debug.Log("스킬1 입력");

        if (waterController == null || playerStats == null) return;
        // 물이 부족하면 실행하지 않음 (조건 유지)
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1)
        {
            UnityEngine.Debug.Log("물 부족");
            return;
        }

        waterController.UseBottle(1);

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        // 마우스가 오른쪽인지 왼쪽인지 판단
        Vector2 direction = (mousePos.x > transform.position.x) ? Vector2.right : Vector2.left;

        Vector2 targetPos = new Vector2(
            transform.position.x + (direction.x * teleportDistance),
            transform.position.y
        );
        Vector3 slashSpawnPos = transform.position + new Vector3(0, 0.5f, 0);

        GameObject slash = Instantiate(slashHitboxPrefab, slashSpawnPos, Quaternion.identity);
        slash.transform.right = direction;

        Destroy(slash, 0.1f);

        transform.position = targetPos;
    }

    /// <summary>
    /// R 스킬 - 파동참
    /// </summary>
    protected override void OnSkillR()
    {
        UnityEngine.Debug.Log("스킬2 입력");
        if (waterController == null || playerStats == null) return;
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 2)
        {
            UnityEngine.Debug.Log("물 부족");
            return;
        }

        waterController.UseBottle(2);

        GameObject wave = Instantiate(waveEffectPrefab, transform.position, Quaternion.identity);
        Destroy(wave, 1.0f);
    }

    /// <summary>
    /// 1번 키 - 힐
    /// </summary>
    protected override void OnHeal()
    {
        UnityEngine.Debug.Log("회복 입력");
        if (waterController == null || playerStats == null) return;
        if (waterController.waterCounter() + waterController.corruptedwaterCounter() < 1)
        {
            UnityEngine.Debug.Log("물 부족");
            return;
        }

        waterController.UseBottle(1);
        playerStats.Heal(1);
    }

    private IEnumerator DisableAfterTime(GameObject obj, float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(obj);
    }
}