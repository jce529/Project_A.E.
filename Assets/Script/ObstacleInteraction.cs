using UnityEngine;

public class ObstacleInteraction : MonoBehaviour
{
    private bool playerInRange = false;
    private GameObject player;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.gameObject;
            Debug.Log($"{gameObject.name} 근처에 플레이어가 들어왔습니다!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            player = null;
            Debug.Log($"{gameObject.name} 범위에서 플레이어가 벗어났습니다.");
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            Destroy(gameObject);
            Debug.Log($"{gameObject.name} 파괴됨!");
        }
    }
}
