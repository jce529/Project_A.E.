using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    // 다음에 플레이어가 나타나야 할 스폰 지점의 이름
    public string NextSpawnPointName { get; set; }

    private void Awake()
    {
        // 싱글톤 설정: 씬이 바뀌어도 이 오브젝트는 하나만 유지됨
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}