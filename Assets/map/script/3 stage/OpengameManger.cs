using UnityEngine;

public class OpengameManager : MonoBehaviour
{
    public static OpengameManager instance;

    [Header("각 맵 잠금 해제 상태")]
    public bool isMap2Open = false; // 2번 맵
    public bool isMap3Open = false; // 3번 맵
    public bool isMap4Open = false; // 4번 맵
    public bool isMap5Open = false; // 5번 맵 (1,2,3,4 완료 시)
    public bool isBossMapOpen = false; // 보스방

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 방이 하나 열릴 때마다 이 함수를 불러서 5번 방이 열릴 조건이 되었는지 검사합니다!
    public void CheckMap5Condition()
    {
        // 만약 2번, 3번, 4번 맵이 모두 열려있다면? (1번은 기본 개방이므로)
        if (isMap2Open && isMap3Open && isMap4Open)
        {
            isMap5Open = true; // 5번 맵을 강제로 열어버림!
        }
    }
}