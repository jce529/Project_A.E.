using UnityEngine;

public class FloorPuzzleManager : MonoBehaviour
{
    [Header("이 층에 있는 모든 수로들을 배열에 넣으세요")]
    public RotatablePipe[] floorPipes;

    [Header("3층일 경우, 잠금 해제할 중앙 퍼즐 장치를 넣으세요 (1층은 비워둠)")]
    public SlidingPuzzleTrigger centerPuzzleTrigger;

    public void CheckPipes()
    {
        // 배열을 돌며 하나라도 틀린 수로가 있는지 검사합니다.
        foreach (var pipe in floorPipes)
        {
            if (!pipe.IsCorrect()) return;
        }

        Debug.Log("이 층의 수로 퍼즐이 모두 맞춰졌습니다!");

        // 3층 기믹: 수로가 다 맞으면 중앙 슬라이딩 퍼즐 잠금 해제!
        if (centerPuzzleTrigger != null)
        {
            centerPuzzleTrigger.isLocked = false;
            Debug.Log("중앙 슬라이딩 퍼즐 장치가 활성화되었습니다!");
        }
    }
}