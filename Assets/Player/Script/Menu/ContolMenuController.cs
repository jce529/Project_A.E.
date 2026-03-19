using UnityEngine;
using UnityEngine.UI; // 슬라이더, 토글 사용을 위해 필수

public class ControlMenuController : MonoBehaviour
{
    [Header("다음 단계 UI 연결")]
    public GameObject keybindingPanel; // [키 설정] 버튼 누르면 열릴 패널

    private void Start()
    {
        // (선택사항) 게임 시작 시, 저장된 설정값이 있다면 UI에 반영
        // float savedSens = PlayerPrefs.GetFloat("MouseSensitivity", 1.0f);
        // sensitivitySlider.value = savedSens;
    }

    // =========================================================
    // 1. [키 바인딩] 화면 진입
    // =========================================================
    public void OnKeyBindingsBtnClick()
    {
        if (keybindingPanel != null)
        {
            UIManager.Instance.PushPanel(keybindingPanel);
            Debug.Log("키 설정 화면으로 이동");
        }
        else
        {
            Debug.LogError("ControlMenu: KeyBindingPanel이 연결되지 않았습니다.");
        }
    }

    // =========================================================
    // 2. [감도] 조절 (슬라이더)
    // =========================================================
    // 슬라이더의 OnValueChanged 이벤트에 연결
    public void OnSensitibityBtnClick()
    {
        Debug.Log("감도 버튼 클릭됨");
    }

    // =========================================================
    // 3. [조작 방식] 변경 (토글)
    // =========================================================
    // 토글의 OnValueChanged 이벤트에 연결
    // isOn이 true면 '한 번 눌러서 달리기(Toggle)', false면 '누른 채로 달리기(Hold)'
    public void OnGamePlayBtnClick()
    {
        Debug.Log("조작방식 버튼 클릭됨");
    }

    // =========================================================
    // 4. 뒤로가기
    // =========================================================
    public void OnBackBtnClick()
    {
        UIManager.Instance.PopPanel();
    }
}