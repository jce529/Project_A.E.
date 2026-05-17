using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("이동 설정")]
    public string sceneToLoad;      // 이동할 씬 이름
    public string targetSpawnName;  // 도착지 스폰 지점 이름

    private bool isPlayerNearby = false;

    private void Update()
    {
        // W키를 눌러 이동 (원하는 키로 변경 가능)
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.W))
        {
            // 1. 이동할 스폰 지점 이름을 매니저에 저장
            GameManager.Instance.NextSpawnPointName = targetSpawnName;

            // 2. 씬 로드
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }
}