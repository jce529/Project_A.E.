using UnityEngine;

public class RoomSwitch : MonoBehaviour
{
    [Header("이 스위치를 누르면 몇 번 방이 열리나요? (숫자 입력)")]
    public int mapToUnlock;

    private bool playerInRange = false;
    private bool isAlreadyUsed = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.W) && !isAlreadyUsed)
        {
            isAlreadyUsed = true;

            // 입력한 숫자에 따라 알맞은 방의 잠금을 해제합니다.
            if (mapToUnlock == 2) OpengameManager.instance.isMap2Open = true;
            if (mapToUnlock == 3) OpengameManager.instance.isMap3Open = true;
            if (mapToUnlock == 4) OpengameManager.instance.isMap4Open = true;
            if (mapToUnlock == 99) OpengameManager.instance.isBossMapOpen = true; // 보스방은 편의상 99로 설정

            // 스위치를 누를 때마다 5번 맵 개방 조건이 만족되었는지 확인!
            OpengameManager.instance.CheckMap5Condition();

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