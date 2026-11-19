//밟으면 사라지는 발판에 넣는 코드
using System.Collections;
using UnityEngine;

public class FloatingPlatform : MonoBehaviour
{
    [Header("징검다리 기본 설정")]
    [Tooltip("부유물이 사라지기까지의 총 제한 시간 (초)")]
    public float lifetime = 15f;
    [Tooltip("사라지기 전 경고(흔들림)를 시작할 남은 시간 (초)")]
    public float warningTime = 3f;
    [Tooltip("흔들림 강도")]
    public float shakeIntensity = 0.1f;

    [Header("리스폰 설정")]
    [Tooltip("체크하면 일정 시간 후 부유물이 다시 리스폰됩니다.")]
    public bool canRespawn = true;
    [Tooltip("사라진 후 다시 나타나기까지 걸리는 시간 (초)")]
    public float respawnTime = 5f;

    private bool timerStarted = false;
    private bool isWarning = false;
    private float currentTimer = 0f;
    private Vector3 originalPosition;

    private SpriteRenderer spriteRenderer;
    private Collider2D platformCollider;
    private Coroutine shakeCoroutine;

    void Start()
    {
        // 초기 위치 및 컴포넌트 캐싱
        originalPosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        platformCollider = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (timerStarted)
        {
            currentTimer += Time.deltaTime;

            // 1. 경고 시간 도달 시 흔들림 시작
            if (!isWarning && currentTimer >= (lifetime - warningTime))
            {
                isWarning = true;
                shakeCoroutine = StartCoroutine(ShakeRoutine());
            }

            // 2. 제한 시간이 지나면 부유물 사라짐 처리
            if (currentTimer >= lifetime)
            {
                Disappear();
            }
        }
    }

    private void Disappear()
    {
        // 상태 값 초기화
        timerStarted = false;
        isWarning = false;
        currentTimer = 0f;

        // 진행 중인 흔들림 코루틴 중단 및 위치 복구
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
        transform.position = originalPosition;

        // 리스폰 여부에 따른 처리
        if (canRespawn)
        {
            StartCoroutine(RespawnRoutine());
        }
        else
        {
            // 리스폰 옵션이 꺼져있으면 오브젝트 완전 파괴
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnRoutine()
    {
        // 부유물 숨기기 (이미지 및 충돌체 비활성화)
        SetPlatformVisible(false);

        // 지정된 리스폰 대기 시간만큼 대기
        yield return new WaitForSeconds(respawnTime);

        // 다시 나타나기 (이미지 및 충돌체 재활성화)
        SetPlatformVisible(true);
    }

    // 부유물의 가시성 및 충돌 여부 제어
    private void SetPlatformVisible(bool visible)
    {
        if (spriteRenderer != null) spriteRenderer.enabled = visible;
        if (platformCollider != null) platformCollider.enabled = visible;

        // ※ 만약 자식 오브젝트로 스프라이트나 콜라이더가 더 들어있다면 아래 주석을 해제해 사용하세요:
        // foreach (var sr in GetComponentsInChildren<SpriteRenderer>()) sr.enabled = visible;
        // foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = visible;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 부유물이 활성화되어 있고, 플레이어가 위에서 밟았을 때만 작동
        if (!timerStarted && collision.gameObject.CompareTag("Player"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y < -0.5f)
                {
                    timerStarted = true;
                    Debug.Log(gameObject.name + " : 타이머 시작!");
                    break;
                }
            }
        }
    }

    private IEnumerator ShakeRoutine()
    {
        while (timerStarted && currentTimer < lifetime)
        {
            float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = new Vector3(originalPosition.x + offsetX, originalPosition.y, originalPosition.z);
            yield return null;
        }
    }
}