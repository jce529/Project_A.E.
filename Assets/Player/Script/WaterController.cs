using UnityEngine;

public class WaterController : MonoBehaviour
{
    public System.Collections.Generic.List<int[]> bottles = new System.Collections.Generic.List<int[]>();
    public bool PureWaterMod = false;
    public bool CorruptedWaterMod = false;

    public void AddBottle(int amout1)
    {
        // 각 물병은 int 2칸짜리 배열
        for (int i = 0; i < amout1; i++)
        {
            int[] bottle_State = new int[2]; // [0]:0 = 빈 물통, 1 = 물이 들어있는 물통, [1]: 0 = 정화됨, 1 = 오염됨
            bottles.Add(bottle_State); // 리스트에 추가
            bottles[i][0] = 1; // 물병이 물로 차있음
        }
    }

    public void UseBottle(int amount2)
    {

        for (int i = 0; i < amount2; i++)
        {
                bottles[waterCounter() + corruptedwaterCounter() - 1][0] = 0; // 물병에서 물을 사용함
        }
        
    }

    public void RecoveryWater()
    {
        for (int i = 0; i < bottles.Count; i++)
        {
            if (bottles[i][0] == 0) // 내용물이 없는 물병을 찾음
            {
                bottles[i][0] = 1; // 물병에 물을 채움
                return; // 물병을 채운 후 함수 종료
            }
        }
    }

    public void RecoveryCorruptedWater()
    {
        for (int i = 0; i < bottles.Count; i++)
        {
            if (bottles[i][0] == 0) // 내용물이 없는 물병을 찾음
            {
                bottles[i][0] = 1; // 물병에 물을 채우고
                bottles[i][1] = 1; // 상태를 오염된 물로 설정
                return; // 오염된 물병을 채운 후 함수 종료
            }
        }
    }

    public int waterCounter()
    {
        int count1 = 0;
        for (int i = 0; i < bottles.Count; i++)
        {
            if (bottles[i][0] == 1 && bottles[i][1] == 0) // 물이 있는 물병을 찾음
            {
                count1++;
            }
        }
        return count1; // 물이 있는 물병의 개수를 반환
    }
    public int corruptedwaterCounter()
    {
        int count2 = 0;
        for (int i = 0; i < bottles.Count; i++)
        {
            if (bottles[i][0] == 1 && bottles[i][1] == 1) //오염된 물이 있는 물병을 찾음
            {
                count2++;
            }
        }
        return count2; // 오염된 물이 있는 물병의 개수를 반환
    }
    void Start()
    {
        AddBottle(5); // 물병을 5개 추가
    }

    void Update()
    {
        waterCounter();
        corruptedwaterCounter();
        if (corruptedwaterCounter() == 0) PureWaterMod = true; // 오염된 물병이 없으면 순수한 물 모드 활성화
        else PureWaterMod = false; // 오염된 물병이 있으면 순수한 물 모드 비활성화 

        if (waterCounter() == 0) CorruptedWaterMod = true; // 물병이 없으면 오염된 물 모드 활성화
        else CorruptedWaterMod = false; // 물병이 있으면 오염된 물 모드 비활성화


    }
}
