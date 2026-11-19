using UnityEngine;
using TMPro;

public class DOUBLESPACE
    : MonoBehaviour
{
    // 유니티 에디터에서 연결할 UI 오브젝트
    [SerializeField]
    private GameObject infoUI;

    // 보여줄 메시지
    private string message = "공중에서 스페이스 키를 한 번 더 누르면 더블 점프를 할 수 있습니다.";

    // 트리거에 들어갔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            infoUI.SetActive(true); // UI 켜기
            infoUI.GetComponentInChildren<TextMeshProUGUI>().text = message; // 메시지 설정
        }
    }

    // 트리거에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            infoUI.SetActive(false); // UI 끄기
        }
    }
}