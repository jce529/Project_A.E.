using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button loadGameButton;

    private void Start()
    {
        if (loadGameButton != null)
        {
            loadGameButton.interactable = SaveLoadManager.Instance != null && SaveLoadManager.Instance.HasSaveFile();
        }
    }

    public void OnClickStart()
    {

        Debug.Log("게임 시작 버튼 클릭됨");
        //스타트버튼 클릭시 게임 시작
        SceneManager.LoadScene("InGame");
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
