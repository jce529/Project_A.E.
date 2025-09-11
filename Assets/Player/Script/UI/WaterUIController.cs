using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class WaterUIController : MonoBehaviour
{
    public GameObject bottleSlot;
    public Transform bottlsSlotParent;
    public Sprite emptyBottleSprite;
    public Sprite corruptedemptyBottleSprite;
    public Sprite waterBottleSprite;
    public Sprite corruptedWaterBottleSprite;

    private List<Image> bottleImages = new List<Image>();
    public WaterController waterController;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitSlot();
    }

    // Update is called once per frame
    void Update()
    {
        updateBottls();
    }

    void InitSlot()
    {
        //현재 물병의 개수만큼 UI를 생성
        for (int i = 0; i < waterController.bottles.Count; i++)
        {
            GameObject newBottleSlot = Instantiate(bottleSlot, bottlsSlotParent);
            Image bottleImage = newBottleSlot.GetComponent<Image>();
            bottleImages.Add(bottleImage);
        }
    }

    void updateBottls()
    {
        //물병의 상태에 따라 UI를 업데이트
        for (int i = 0; i < waterController.bottles.Count; i++)
        {
            if (waterController.bottles[i][0] == 0)
            {
                //물병이 비어있음
                if (waterController.bottles[i][1] == 0)
                {
                    bottleImages[i].sprite = emptyBottleSprite; // 정화된 빈 물병
                }
                else
                {
                    bottleImages[i].sprite = corruptedemptyBottleSprite; // 오염된 빈 물병
                }
            }
            else
            {
                //물병에 물이 있음
                if (waterController.bottles[i][1] == 0)
                {
                    bottleImages[i].sprite = waterBottleSprite; // 정화된 물병
                }
                else
                {
                    bottleImages[i].sprite = corruptedWaterBottleSprite; // 오염된 물병
                }
            }
        }
    }
}
