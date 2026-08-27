using UnityEngine;

public class PumpManager : MonoBehaviour
{
    public int requiredItems = 10;
    private int currentItems = 0;

    public void SubmitItems(int amount)
    {
        currentItems += amount;

        if (currentItems >= requiredItems)
        {
            ActivatePump();
        }
    }

    private void ActivatePump()
    {

        // 4번 맵을 열고 조건 체크
        OpengameManager.instance.isMap4Open = true;
        OpengameManager.instance.CheckMap5Condition();
    }
}