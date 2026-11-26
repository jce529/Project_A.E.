using System.Collections;
using UnityEngine;

public class BossHorn : MonoBehaviour
{
    [Header("Settings")]
    public float hornRange = 3.5f;
    public float hornCooldown = 2.5f;
    public float telegraphTime = 0.4f;
    public float attackDuration = 0.2f;
    public float recoveryTime = 0.5f;

    public Color telegraphColor = new Color(1f, 0.6f, 0f);

    [Header("Hitbox")]
    public GameObject hornHitbox; // 💡 유니티 에디터에서 뿔치기 힛박스를 연결할 빈칸

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;

        // 💡 시작할 때 힛박스가 켜져있다면 안전하게 꺼둡니다.
        if (hornHitbox != null) hornHitbox.SetActive(false);
    }

    public IEnumerator HornRoutine()
    {
        // 1. 준비 동작 (선딜레이)
        if (spriteRenderer != null) spriteRenderer.color = telegraphColor;
        yield return new WaitForSeconds(telegraphTime);

        // 2. 공격 판정 시작
        if (spriteRenderer != null) spriteRenderer.color = Color.red;

        // 💡 이 순간에 힛박스를 켭니다! (플레이어가 이 네모 안에 있으면 OnTriggerEnter2D 발동)
        if (hornHitbox != null) hornHitbox.SetActive(true);

        yield return new WaitForSeconds(attackDuration);

        // 3. 공격 판정 종료
        // 💡 공격 판정 시간이 지나면 힛박스를 다시 끕니다!
        if (hornHitbox != null) hornHitbox.SetActive(false);

        // 4. 후딜레이 및 상태 복구
        if (spriteRenderer != null) spriteRenderer.color = originalColor;
        yield return new WaitForSeconds(recoveryTime);
    }
}