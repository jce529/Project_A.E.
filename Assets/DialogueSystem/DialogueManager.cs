using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    [Header("데이터 파일 연결")]
    public TextAsset dialogueJsonData;

    public static event Action OnDialogueStarted;
    public static event Action OnDialogueEnded;
    public static event Action<DialogueNodeData> OnLineUpdated;

    public static event Action<string> OnDialogueAction;

    private List<DialogueNodeData> dialogueList;
    private DialogueNodeData currentNode;
    public bool IsDialogueActive => currentNode != null;
    private int dialogueStartedFrame = -1;

    void Awake()
    {
        LoadDialogueData();
    }

    void Start()
    {
        //StartDialogue(0);
    }

    void Update()
    {
        if (Time.frameCount != dialogueStartedFrame && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            NextLine();
        }
    }

    private void LoadDialogueData()
    {
        if (dialogueJsonData != null)
        {
            DialogueWrapper wrapper = JsonUtility.FromJson<DialogueWrapper>(dialogueJsonData.text);
            dialogueList = wrapper.dialogues;
            Debug.Log($"[시스템] 대화 데이터 로드 완료! 총 {dialogueList.Count}개의 대사");
        }
        else
        {
            Debug.LogError("[시스템] JSON 데이터 파일이 연결되지 않았습니다!");
        }
    }

    public void StartDialogue(int nodeIndex)
    {
        if (dialogueList == null || nodeIndex < 0 || nodeIndex >= dialogueList.Count) return;

        currentNode = dialogueList[nodeIndex];
        dialogueStartedFrame = Time.frameCount;

        Time.timeScale = 0f;

        OnDialogueStarted?.Invoke();
        OnLineUpdated?.Invoke(currentNode);
        if (!string.IsNullOrEmpty(currentNode.actionID))
        {
            OnDialogueAction?.Invoke(currentNode.actionID);
        }
    }

    public void NextLine()
    {
        if (currentNode == null) return;

        if (currentNode.choices != null && currentNode.choices.Count > 0)
        {
            return;
        }
        if (currentNode.isEnd)
        {
            EndDialogue();
            return;
        }

        int nextIndex = currentNode.jumpToIndex != -1 ? currentNode.jumpToIndex : dialogueList.IndexOf(currentNode) + 1;

        if (nextIndex < dialogueList.Count)
        {
            StartDialogue(nextIndex);
        }
        else
        {
            EndDialogue();
        }
    }

    public void SelectChoice(int nextNodeIndex)
    {
        StartDialogue(nextNodeIndex);
    }

    public void EndDialogue()
    {
        currentNode = null;
        OnDialogueEnded?.Invoke();
        Time.timeScale = 1f;
        Debug.Log("[시스템] 대화 종료");
    }
}