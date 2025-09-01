using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.XR;

public class boss : MonoBehaviour
{
    public Transform playerTransform;
    public GameObject mudHandPrefab;
   
    public float attackInterval; //공격 간격
    public float summon0ffset = 1f; // 플레이어로부터 손 소환 위치 오프셋
    public int maxHealth = 100;
    private int currentHealth;
    public float moveSpeed;
    public float stoppingDistance; // 플레이어 거리 측정 후 멈춤
    private Animator animator;
    private Rigidbody2D rb;
    
    public LayerMask isLayer;
    public float cooltime;
    private float currenttime;
    public float distance;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("플레이어를 찾을 수 없습니다. player 태그를 확인하세요.");
            }
        }
        StartCoroutine(AttackRoutine());
    }
    void Update()
    {
        if (playerTransform != null && currentHealth > 0)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer > stoppingDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
            }
        }
    }
    IEnumerator AttackRoutine()
    {
        while (currentHealth > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            if (playerTransform != null)
            {
                SummonMudHand();
            }

        }
    }
    void SummonMudHand()
    {
        //Vector3 summonPosition = new Vector3(playerTransform.position.x, playerTransform.position.y - summon0ffset, 10);
        float groundY = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).y - 1f;
        Vector3 summonPosition = new Vector3(playerTransform.position.x, groundY, 0);
        Instantiate(mudHandPrefab, summonPosition, Quaternion.identity);
    }
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("보스 체력: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
        
    }
    void Die()
    {
        Debug.Log("보스 사망!");
        Destroy(gameObject, 2f);
       

    }
    
}
