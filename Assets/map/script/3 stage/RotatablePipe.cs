using UnityEngine;

public class RotatablePipe : MonoBehaviour
{
    public int correctRotationIndex; // 정답 방향 (0=기본, 1=90도, 2=180도, 3=270도)
    private int currentIndex = 0;

    // 스위치를 누르면 호출되어 스스로 90도 회전합니다.
    public void RotatePipe()
    {
        currentIndex = (currentIndex + 1) % 4;
        transform.Rotate(0, 0, 90f);
    }

    // 현재 정답 방향인지 확인합니다.
    public bool IsCorrect()
    {
        return currentIndex == correctRotationIndex;
    }
}