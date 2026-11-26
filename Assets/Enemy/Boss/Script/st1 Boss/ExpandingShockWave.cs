using UnityEngine;

public class ExpandingShockwave : MonoBehaviour
{
    [Header("확장 설정")]
    public float expandSpeed = 30f;   // 커지는 속도
    public float maxScale = 15f;     // 최대 크기
    public float lifeTime = 2f;      // 최대 생존 시간 (안전장치)

    void Start()
    {
        // 생성될 때 스케일을 0이나 아주 작게 시작하게 만들 수도 있습니다.
        // transform.localScale = Vector3.zero;

        // 일정 시간 뒤 파괴 (메모리 누수 방지)
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // 매 프레임마다 xyz 스케일을 동일하게 키움
        transform.localScale += Vector3.one * (expandSpeed * Time.deltaTime);

        // 최대 크기에 도달하면 스스로 파괴
        if (transform.localScale.x >= maxScale)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("확장형 충격파 적중! 데미지 적용");

            // 여기에 플레이어 HP 깎기 코드를 넣으시면 됩니다.
            // HP hp = other.GetComponent<HP>();
            // if(hp != null) hp.TakeDamage(10);
        }
    }
}