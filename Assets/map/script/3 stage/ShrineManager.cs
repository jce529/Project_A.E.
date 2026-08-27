using UnityEngine;

public class ShrineManager : MonoBehaviour
{
    public GameObject[] flowerMonsters; // 스폰할 몬스터 배열
    public GameObject nextMapPortal;    // 다음 맵 포탈 (맵 내부에 있는 경우)
    private int monstersKilled = 0;
    private bool isFirstInteractionDone = false;

    public void InteractWithShrine()
    {
        if (!isFirstInteractionDone)
        {
            SpawnMonsters();
            isFirstInteractionDone = true;
        }
        else if (monstersKilled >= 3)
        {
            if (nextMapPortal != null) nextMapPortal.SetActive(true);

            // 기존에 만드신 매니저를 정확한 이름과 소문자 instance로 호출합니다!
            OpengameManager.instance.isMap2Open = true;
            OpengameManager.instance.CheckMap5Condition();

        }
    }

    private void SpawnMonsters()
    {
        foreach (var monster in flowerMonsters)
        {
            monster.SetActive(true);
        }
    }

    public void OnMonsterDied()
    {
        monstersKilled++;
    }
}