using UnityEngine;
using UnityEngine.UI; // 이것과 다른 시스템 라이브러리가 충돌한 것입니다.

public class BossHealthBarController : MonoBehaviour
{
    public GameObject healthBarPrefab;
    public Transform uiParentCanvas;

    // BossController를 참조하도록 변경 (기존 boss 스크립트 삭제 반영)
    public BossController bossController;

    private BossStatsSystem bossStats;
    private GameObject barInstance;

    // [에러 수정 부분] 충돌 방지를 위해 'UnityEngine.UI.Image'라고 풀네임을 적어줍니다.
    private UnityEngine.UI.Image fillImage;

    void Start()
    {
        // 1. BossController 연결 확인
        if (bossController != null)
        {
            // BossController 안에 있는 Stats 시스템을 가져옴
            bossStats = bossController.Stats;
        }

        // 2. 체력바 UI 생성 (꺼둔 상태로 시작)
        if (healthBarPrefab != null && uiParentCanvas != null)
        {
            barInstance = Instantiate(healthBarPrefab, uiParentCanvas);

            // "Fill"이라는 이름의 자식 오브젝트에서 Image 컴포넌트 찾기
            // 여기서도 명시적으로 UnityEngine.UI.Image 사용
            fillImage = barInstance.transform.Find("Fill")?.GetComponent<UnityEngine.UI.Image>();

            barInstance.SetActive(false);
        }
    }

    void Update()
    {
        // 필수 요소들이 없으면 실행 안 함
        if (bossController == null || bossStats == null || barInstance == null) return;

        // 1. 보스가 플레이어를 감지했는지 확인 (TargetFound)
        bool show = bossController.TargetFound;

        // 감지 여부에 따라 체력바 켜기/끄기
        if (barInstance.activeSelf != show)
        {
            barInstance.SetActive(show);
        }

        // 2. 체력바가 켜져 있다면 체력 갱신
        if (show && fillImage != null)
        {
            // BossStatsSystem에 MaxHealth와 CurrentHealth(혹은 _currentHealth)를 
            // 가져올 수 있는 프로퍼티가 있어야 합니다. (public getter 필요)

            // 만약 BossStatsSystem 변수가 private이라 접근이 안 된다면
            // BossStatsSystem.cs에 'public float CurrentHealth => _currentHealth;' 등을 추가해야 합니다.

            // 임시 예시 코드 (실제 변수명에 맞춰 수정 필요):
            float hpPercent = bossStats.CurrentHealth / bossStats.MaxHealth;
            fillImage.fillAmount = hpPercent;
        }
    }
}