using UnityEngine;
using TMPro; // TextMeshPro 사용 필수
using UnityEngine.UI;
using System.Collections.Generic;

public class DialogueView : MonoBehaviour
{
    [Header("UI 요소 연결")]
    public GameObject dialoguePanel; // 대화창 전체 배경 패널
    public TextMeshProUGUI nameText; // 이름 출력용 
    public TextMeshProUGUI dialogueText; // 대화 내용 출력용 

    [Header("선택지 요소 연결")]
    public Transform choiceContainer; // 선택지 버튼들이 생성될 부모 (예: Vertical Layout Group)
    public GameObject choiceButtonPrefab; // 미리 만들어둔 선택지 버튼 프리팹

    private List<GameObject> activeChoiceButtons = new List<GameObject>();

    void OnEnable()
    {
        // 매니저의 이벤트를 구독하여 UI를 제어합니다.
        DialogueManager.OnDialogueStarted += ShowPanel;
        DialogueManager.OnDialogueEnded += HidePanel;
        DialogueManager.OnLineUpdated += UpdateDialogueUI;
    }

    void OnDisable()
    {
        // 씬이 넘어가거나 오브젝트가 꺼질 때 메모리 누수 방지를 위해 구독 해제
        DialogueManager.OnDialogueStarted -= ShowPanel;
        DialogueManager.OnDialogueEnded -= HidePanel;
        DialogueManager.OnLineUpdated -= UpdateDialogueUI;
    }

    private void ShowPanel() => dialoguePanel.SetActive(true);
    private void HidePanel() => dialoguePanel.SetActive(false);

    // 매니저가 새 대사 데이터를 던져줄 때마다 실행됨
    private void UpdateDialogueUI(DialogueNodeData nodeData)
    {
        nameText.text = nodeData.speakerName;
        dialogueText.text = nodeData.dialogueText;

        ClearChoiceButtons();

        if (nodeData.choices != null && nodeData.choices.Count > 0)
        {
            foreach (var choice in nodeData.choices)
            {
                GameObject btnObj = Instantiate(choiceButtonPrefab, choiceContainer);

                // 생성된 버튼의 텍스트 변경 (버튼 하위의 TextMeshProUGUI 찾기)
                btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;

                // 버튼 클릭 이벤트에 매니저의 선택지 클릭 기능 연결
                Button btn = btnObj.GetComponent<Button>();
                btn.onClick.AddListener(() =>
                {
                    FindObjectOfType<DialogueManager>().SelectChoice(choice.nextNodeIndex);
                });

                activeChoiceButtons.Add(btnObj);
            }
        }
    }

    private void ClearChoiceButtons()
    {
        foreach (var btn in activeChoiceButtons)
        {
            Destroy(btn);
        }
        activeChoiceButtons.Clear();
    }
}