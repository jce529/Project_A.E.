using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    // 씬이 바뀌어도 유지되도록 static 유지
    public static string targetSpawnPointName = "";

    void Start()
    {
        ApplySpawn();
    }

    public void ApplySpawn()
    {
        if (!string.IsNullOrEmpty(targetSpawnPointName))
        {
            // Find만으로 못 찾는 경우가 많으므로, 모든 게임 오브젝트 중 이름을 대조합니다.
            GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
            GameObject targetPoint = null;

            foreach (GameObject obj in allObjects)
            {
                if (obj.name == targetSpawnPointName)
                {
                    targetPoint = obj;
                    break;
                }
            }

            if (targetPoint != null)
            {
                // 1. 플레이어 위치를 포탈 스폰 위치로 강제 이동
                transform.position = targetPoint.transform.position;

                // 2. PlayerRespawn 컴포넌트와 연동하여 체크포인트 갱신
                PlayerRespawn respawn = GetComponent<PlayerRespawn>();
                if (respawn != null)
                {
                    respawn.SyncStartPosition(targetPoint.transform);
                }


                // 이동 후 이름 초기화 (다음 번엔 기본 위치에서 시작할 수 있도록)
                targetSpawnPointName = "";
            }
            else
            {
                Debug.LogWarning($"스폰 실패: '{targetSpawnPointName}' 이름을 가진 오브젝트를 씬에서 찾을 수 없습니다.");
            }
        }
    }
}