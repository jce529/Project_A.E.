using UnityEngine;

public class GameSystemTester : MonoBehaviour
{
    void OnEnable()
    {
        // 대화 매니저가 쏘는 '액션 신호' 방송을 구독!
        DialogueManager.OnDialogueAction += HandleDialogueAction;
    }

    void OnDisable()
    {
        DialogueManager.OnDialogueAction -= HandleDialogueAction;
    }

    // 신호탄(actionID)이 날아오면 이 함수가 실행됨
    private void HandleDialogueAction(string actionID)
    {
        Debug.Log($"[본 게임 수신] 대화창으로부터 신호를 받았습니다: {actionID}");

        // 넘어온 문자열에 따라 실제 체력 증감 로직을 흉내 냄
        if (actionID == "Heal_50")
        {
            Debug.Log("💚 띠링! 플레이어의 체력을 50 회복합니다!");
        }
        else if (actionID == "Damage_10")
        {
            Debug.Log("💔 윽! 플레이어의 체력이 10 깎였습니다!");
        }
    }
}