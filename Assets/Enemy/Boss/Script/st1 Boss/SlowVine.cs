using System.Collections;
using UnityEngine;

public class SlowVine : MonoBehaviour
{
    [Header("슬로우 설정")]
    [Range(0.1f, 1f)]
    public float slowRatio = 0.5f; // 0.5 = 속도 절반(50%), 0.8 = 속도 80%

    // 속도가 느려진 플레이어를 기억해두기 위한 변수
    private PlayerController affectedPlayer;

    // 1️⃣ 덩쿨에 닿기 시작할 때 (속도 감소)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();

            // 아직 슬로우를 걸지 않은 플레이어라면
            if (controller != null && affectedPlayer == null)
            {
                affectedPlayer = controller;

                // 설정한 비율(slowRatio)을 곱해서 속도를 줄임 (예: 3 * 0.5 = 1.5)
                affectedPlayer.defaultSpeed *= slowRatio;
                affectedPlayer.runSpeed *= slowRatio;
            }
        }
    }

    // 2️⃣ 덩쿨에서 벗어날 때 (속도 복구)
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController controller = other.GetComponent<PlayerController>();

            // 덩쿨을 벗어난 대상이 우리가 느려지게 만들었던 그 플레이어가 맞다면
            if (controller != null && controller == affectedPlayer)
            {
                RestoreSpeed();
            }
        }
    }

    // 3️⃣ 덩쿨 자체가 시간이 다 되어 사라질 때 (안전장치)
    private void OnDestroy()
    {
        // 플레이어가 아직 덩쿨 안에 있는데 덩쿨이 사라지면 영원히 느려지는 버그 방지
        RestoreSpeed();
    }

    // 속도를 원래대로 되돌리는 함수
    private void RestoreSpeed()
    {
        if (affectedPlayer != null)
        {
            // 줄였던 비율만큼 다시 나누어서 원상복구 (예: 1.5 / 0.5 = 3)[cite: 2]
            affectedPlayer.defaultSpeed /= slowRatio;
            affectedPlayer.runSpeed /= slowRatio;

            // 플레이어 기억 지우기
            affectedPlayer = null;
        }
    }
}
