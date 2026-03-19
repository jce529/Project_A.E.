using UnityEngine;
using TMPro;

public class TutorialTrigger : MonoBehaviour
{
    [SerializeField]
    protected GameObject tutorialUI; 

    [SerializeField]
    [TextArea(3, 10)]
    protected string tutorialText; 

    // 자식 클래스가 이 함수를 재정의(override) 할 수 있도록 virtual 키워드 추가
    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialUI.SetActive(true);
            tutorialUI.GetComponentInChildren<TextMeshProUGUI>().text = tutorialText;
        }
    }

    protected void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            tutorialUI.SetActive(false);
        }
    }
}