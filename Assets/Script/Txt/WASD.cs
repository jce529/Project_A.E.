using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요합니다.

public class WASD : MonoBehaviour
{
    // 튜토리얼 UI 오브젝트를 연결할 변수
    [SerializeField]
    private GameObject tutorialUI;

    // UI에 표시될 튜토리얼 텍스트 (인스펙터 창에서 직접 수정)
    [SerializeField]
    [TextArea(3, 5)] // 인스펙터 창에서 여러 줄로 편하게 입력 가능
    private string tutorialMessage = "W, A, S, D 키를 눌러 이동할 수 있습니다.";

    // 트리거 영역에 플레이어가 들어왔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 들어온 오브젝트가 "Player" 태그를 가졌는지 확인
        if (other.CompareTag("Player"))
        {
            // 튜토리얼 UI를 활성화
            tutorialUI.SetActive(true);

            // UI 안의 TextMeshPro 컴포넌트를 찾아 텍스트를 설정
            tutorialUI.GetComponentInChildren<TextMeshProUGUI>().text = tutorialMessage;
        }
    }

    // 트리거 영역에서 플레이어가 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 튜토리얼 UI를 비활성화
            tutorialUI.SetActive(false);
        }
    }
}