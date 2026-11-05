using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

const float RowStep = 44f;
var report = new StringBuilder();
if (EditorApplication.isPlayingOrWillChangePlaymode) throw new System.Exception("Run in Edit mode.");
if (SceneManager.GetActiveScene().isDirty) throw new System.Exception("Active scene has unsaved changes - aborting.");
var panelType = System.Type.GetType("ControlsSettingsPanel, Assembly-CSharp");

foreach (var path in new[] { "Assets/Scenes/1 stage.unity", "Assets/Scenes/Tutorial Map.unity" })
{
    var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    var panel = Resources.FindObjectsOfTypeAll(panelType).Cast<Component>()
        .Single(c => c.gameObject.scene == scene);
    var so = new SerializedObject(panel);
    var entries = so.FindProperty("entries");

    SerializedProperty actionEntry = null;
    bool exists = false;
    for (int i = 0; i < entries.arraySize; i++)
    {
        var e = entries.GetArrayElementAtIndex(i);
        string name = e.FindPropertyRelative("actionName").stringValue;
        if (name == "Inventory") exists = true;
        if (name == "Action") actionEntry = e;
    }
    if (exists) { report.AppendLine(path + ": Inventory entry already present - skipped"); continue; }

    var srcButton = (Button)actionEntry.FindPropertyRelative("rebindButton").objectReferenceValue;
    var srcRow = (RectTransform)srcButton.transform.parent;
    var content = (RectTransform)srcRow.parent;
    var resetRow = (RectTransform)((Button)so.FindProperty("resetButton").objectReferenceValue).transform;
    while (resetRow.parent != content) resetRow = (RectTransform)resetRow.parent;

    // Precondition: the Action row must be the lowest key row (only the Reset row below it).
    foreach (RectTransform child in content)
        if (child != srcRow && child != resetRow && child.anchoredPosition.y < srcRow.anchoredPosition.y)
            throw new System.Exception(path + ": row '" + child.name + "' sits below the Action row - layout differs, stop.");

    var newRow = (RectTransform)Object.Instantiate(srcRow.gameObject, content).transform;
    newRow.name = "InventoryRow";
    newRow.SetSiblingIndex(srcRow.GetSiblingIndex() + 1);
    newRow.anchoredPosition = srcRow.anchoredPosition - new Vector2(0f, RowStep);
    newRow.Find("ActionLabel").GetComponent<TMP_Text>().text = "Inventory";
    var newButton = newRow.Find("KeyBtn").GetComponent<Button>();
    var newKeyText = newRow.Find("KeyBtn/KeyText").GetComponent<TMP_Text>();
    newKeyText.text = "Tab";
    if (newButton.onClick.GetPersistentEventCount() != 0)
        throw new System.Exception(path + ": cloned KeyBtn has persistent onClick listeners - inspect before continuing.");

    resetRow.anchoredPosition -= new Vector2(0f, RowStep);
    content.sizeDelta += new Vector2(0f, RowStep);

    int n = entries.arraySize;
    entries.InsertArrayElementAtIndex(n);
    var added = entries.GetArrayElementAtIndex(n);
    added.FindPropertyRelative("actionName").stringValue = "Inventory";
    added.FindPropertyRelative("bindingIndex").intValue = 0;
    added.FindPropertyRelative("rebindButton").objectReferenceValue = newButton;
    added.FindPropertyRelative("keyLabel").objectReferenceValue = newKeyText;
    so.ApplyModifiedPropertiesWithoutUndo();

    EditorSceneManager.MarkSceneDirty(scene);
    EditorSceneManager.SaveScene(scene);
    report.AppendLine($"{path}: row y={newRow.anchoredPosition.y}, reset y={resetRow.anchoredPosition.y}, content h={content.sizeDelta.y}, entries={entries.arraySize}");
}
return report.ToString();
