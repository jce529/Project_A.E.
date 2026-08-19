using UnityEngine;

public class PumpManager : MonoBehaviour
{
    public int requiredItems = 10;
    private int currentItems = 0;

    public void SubmitItems(int amount)
    {
        currentItems += amount;
        Debug.Log($"현재 바친 아이템: {currentItems} / {requiredItems}");

        if (currentItems >= requiredItems)
        {
            ActivatePump();
        }
    }

    private void ActivatePump()
    {
        Debug.Log("정화 펌프 작동!");

        // 4번 맵을 열고 조건 체크
        OpengameManager.instance.isMap4Open = true;
        OpengameManager.instance.CheckMap5Condition();
    }
}