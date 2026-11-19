using UnityEngine;

public class SettingMenuController : MonoBehaviour
{
    [Header("다음 단계 UI 연결")]
    // 기존: keybindingPanel -> 변경: controlPanel
    public GameObject controlPanel;

    // [설정] -> [컨트롤] 버튼 클릭 시
    public void OnControlsBtnClick()
    {
        if (controlPanel != null)
        {
            // 이제 키바인딩이 아니라 '컨트롤 메뉴'를 엽니다.
            UIManager.Instance.PushPanel(controlPanel);
            Debug.Log("컨트롤 메뉴 진입");
        }
        else
        {
            Debug.LogError("SettingMenu: ControlPanel이 연결되지 않았습니다!");
        }
    }

    // 뒤로가기
    public void OnBackBtnClick()
    {
        UIManager.Instance.PopPanel();
    }

    // 기타 버튼들...
    public void OnGeneralBtnClick() { Debug.Log("일반 설정"); }
    public void OnSoundBtnClick() { Debug.Log("소리 설정"); }
    public void OnGraphicBtnClick() { Debug.Log("그래픽 설정"); }
}