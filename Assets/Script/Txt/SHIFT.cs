using UnityEngine;
using TMPro;

public class SHITF : MonoBehaviour
{
    // [SerializeField]는 유니티 에디터에서 연결할 수 있게 해줍니다.
    [SerializeField]
    private GameObject infoUI; // 텍스트를 보여줄 UI 오브젝트

    // 보여주고 싶은 메시지 내용
    private string message = "Shift 키를 길게 누르면 달릴 수 있습니다.";

    // 트리거에 들어갔을 때
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            infoUI.SetActive(true); // UI를 켜고
            infoUI.GetComponentInChildren<TextMeshProUGUI>().text = message; // 메시지를 설정
        }
    }

    // 트리거에서 나갔을 때
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            infoUI.SetActive(false); // UI를 끈다
        }
    }
}