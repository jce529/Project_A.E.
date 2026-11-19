using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [Header("WaterUI")]
    public GameObject WaterIconPrefab; // 순수한 물 아이콘 프리팹
    public Transform watePanel; // 물 아이콘을 배치할 패널
    public Sprite pureSprite; // 순수한 물 아이콘 스프라이트
    public Sprite corruptedSprite; // 오염된 물 아이콘 스프라이트

    private Image[] waterIcons; // 물 아이콘을 저장할 배열

    public void InitWaterIcon(int maxCount)
    {
        waterIcons = new Image[maxCount]; // 물 아이콘 배열 초기화

        for (int i = 0; i < maxCount; ++i)
        {
            GameObject PureWatericon = Instantiate(WaterIconPrefab, watePanel); // 순수한 물 아이콘 생성

            waterIcons[i] = PureWatericon.GetComponent<Image>(); // 아이콘의 Image 컴포넌트 가져오기
        }

    }



    public void updateWater(int currentWater, int corruptedCount)
    {
        for (int i = 0; i < waterIcons.Length; ++i)
        {
            if (i < currentWater)
            {
                if (i >= currentWater - corruptedCount)
                {
                    waterIcons[i].sprite = pureSprite; // 순수한 물 아이콘 설정
                }
                else
                {
                    waterIcons[i].sprite = corruptedSprite; // 오염된 물 아이콘 설정
                }
            }
        }
    }
}