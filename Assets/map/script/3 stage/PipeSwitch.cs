using UnityEngine;

public class PipeSwitch : MonoBehaviour
{
    [Header("이 장치가 돌릴 수로를 끌어다 넣으세요")]
    public RotatablePipe targetPipe;

    [Header("이 층을 관리하는 퍼즐 매니저를 넣으세요")]
    public FloorPuzzleManager puzzleManager;

    // 플레이어가 장치 앞에서 상호작용 키를 누르면 실행되는 함수
    public void InteractSwitch()
    {
        if (targetPipe != null)
        {
            targetPipe.RotatePipe();      // 수로를 90도 돌리고
            puzzleManager.CheckPipes();   // 층 전체 수로가 정답인지 확인!
        }
    }
}