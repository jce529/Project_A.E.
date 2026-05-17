using UnityEngine;
using UnityEngine.SceneManagement;

public class SignpostPortal : MonoBehaviour
{
    [Header("이동 설정")]
    public string nextSceneName;
    public string spawnPointName;

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.W))
        {
            // 정적 변수에 목표 지점 이름을 미리 저장
            PlayerSpawner.targetSpawnPointName = spawnPointName;
            SceneManager.LoadScene(nextSceneName);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }
}