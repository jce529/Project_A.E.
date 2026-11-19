using UnityEngine;

public class MapManager : MonoBehaviour
{
    [Header("메인 허브의 닫혀있는 문(포탈)들")]
    public GameObject map2Door; // 2번 방 포탈
    public GameObject map3Door; // 3번 방 포탈
    public GameObject map4Door; // 4번 방 포탈
    public GameObject map5Door; // 5번 방 포탈
    public GameObject bossDoor; // 보스방 포탈

    void Start()
    {
        // 메인 맵이 켜질 때마다 OpengameManager를 확인합니다.
        if (OpengameManager.instance != null)
        {
            // 각 문이 연결되어 있다면, 관리자의 true/false 상태에 따라 문을 켜고 끕니다.
            if (map2Door != null) map2Door.SetActive(OpengameManager.instance.isMap2Open);
            if (map3Door != null) map3Door.SetActive(OpengameManager.instance.isMap3Open);
            if (map4Door != null) map4Door.SetActive(OpengameManager.instance.isMap4Open);
            if (map5Door != null) map5Door.SetActive(OpengameManager.instance.isMap5Open);
            if (bossDoor != null) bossDoor.SetActive(OpengameManager.instance.isBossMapOpen);
        }
    }
}
