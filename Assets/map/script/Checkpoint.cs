using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private PlayerRespawn playerRespawn;
    public bool isActiveCheckpoint = false;

    void Update()
    {
        // 범위 안에서 S키를 눌렀을 때
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log(" [" + gameObject.name + "]에서 S키 입력이 감지되었습니다!");

            if (!isActiveCheckpoint)
            {
                // 1. 씬에 있는 모든 체크포인트를 찾아서 끕니다.
                Checkpoint[] allCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                foreach (Checkpoint cp in allCheckpoints)
                {
                    cp.isActiveCheckpoint = false;
                }

                // 2. 이 체크포인트만 켭니다.
                isActiveCheckpoint = true;

                if (playerRespawn != null)
                {
                    playerRespawn.UpdateCheckpoint(this.transform);
                    Debug.Log(" 성공: [" + gameObject.name + "] 위치로 체크포인트가 변경되었습니다!");
                }
                else
                {
                    Debug.LogError(" 실패: PlayerRespawn 스크립트를 찾지 못했습니다.");
                }
            }
            else
            {
                Debug.Log(" [" + gameObject.name + "]은(는) 이미 현재 활성화된 체크포인트입니다.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            playerRespawn = collision.GetComponent<PlayerRespawn>();
            Debug.Log(" 플레이어가 [" + gameObject.name + "] 영역에 들어왔습니다.");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerRespawn = null;
            Debug.Log(" 플레이어가 [" + gameObject.name + "] 영역에서 나갔습니다.");
        }
    }
}