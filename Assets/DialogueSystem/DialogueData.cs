using System;
using System.Collections.Generic;

[Serializable]
public class ChoiceData
{
    public string choiceText;
    public int nextNodeIndex;
    public string conditionID; 
}

[Serializable]
public class DialogueNodeData
{
    public string speakerName;
    public string dialogueText;
    public List<ChoiceData> choices;

    public bool isEnd;
    public int jumpToIndex = -1; 
    public string actionID; 
}

[Serializable]
public class DialogueWrapper
{
    public List<DialogueNodeData> dialogues;
}