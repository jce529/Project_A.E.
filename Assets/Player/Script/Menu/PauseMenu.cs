using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("연결할 UI")]
    public GameObject settingPanel; // 바로 다음 단계인 '설정 패널'을 직접 연결

    // [계속하기] 버튼
    public void OnResumeBtnClick()
    {
        UIManager.Instance.CloseAll();
    }

    // [설정] 버튼
    public void OnSettingsBtnClick()
    {
        if (settingPanel != null)
        {
            // UIManager에게 "내 다음 단계인 설정창을 열어줘" 요청
            UIManager.Instance.PushPanel(settingPanel);
        }
        else
        {
            Debug.LogError("PauseMenu: SettingPanel이 연결되지 않았습니다.");
        }
    }

    // [종료] 버튼
    public void OnQuitBtnClick()
    {
        Debug.Log("게임 종료");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}