using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Color loadGameDisabledColor = new Color(0.05f, 0.05f, 0.05f, 1f);

    private void Start()
    {
        if (loadGameButton != null)
        {
            bool hasSave = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSaveFile();
            loadGameButton.interactable = hasSave;

            var loadGameText = loadGameButton.GetComponentInChildren<TMP_Text>();
            if (loadGameText != null && !hasSave)
            {
                loadGameText.color = loadGameDisabledColor;
            }
        }
    }

    public void OnClickStart()
    {

        Debug.Log("���� ���� ��ư Ŭ����");
        //��ŸƮ��ư Ŭ���� ���� ����
        SceneManager.LoadScene("Tutorial Map");
    }

    public void OnClickLoad()
    {
        if (SaveLoadManager.Instance == null || !SaveLoadManager.Instance.HasSaveFile())
        {
            Debug.LogWarning("[MainMenuUI] OnClickLoad: no save file found.");
            return;
        }

        SaveLoadManager.Instance.LoadGame();
    }
}
