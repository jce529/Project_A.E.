using UnityEngine;

public class InteractionPrompt : MonoBehaviour
{
    [Header("UI 설정")]
    public GameObject promptUI;

    [Header("거리 설정")]
    public float interactionDistance = 3f;

    [Header("상호작용 연결")]
    public WaterController waterController;

    private Transform playerTransform;
    private Transform mainCameraTransform;

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        mainCameraTransform = Camera.main?.transform;

        if (promptUI != null)
            promptUI.SetActive(false);
    }

    void Update()
    {
        if (playerTransform == null || promptUI == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= interactionDistance)
        {
            promptUI.SetActive(true);

            // 🎯 F키로 물 회복 상호작용
            if (Input.GetKeyDown(KeyCode.F))
            {
                Interact();
            }
        }
        else
        {
            promptUI.SetActive(false);
        }
    }

    void LateUpdate()
    {
        if (promptUI != null && promptUI.activeSelf && mainCameraTransform != null)
        {
            // UI가 항상 카메라 정면을 향하도록
            promptUI.transform.rotation = mainCameraTransform.rotation;
        }
    }

    void Interact()
    {
        if (waterController != null)
        {
            // ✅ 깨끗한 물 회복
            waterController.RecoveryWater();
        }

        promptUI.SetActive(false);
    }
}
