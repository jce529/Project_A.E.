using UnityEngine;

// 키 설정 화면 전체(패널)를 관리하는 스크립트
public class KeyBindingMenuController : MonoBehaviour
{
    // 뒤로가기 버튼 클릭 시 호출
    public void OnBackBtnClick()
    {
        // UIManager를 통해 현재 패널 닫기
        UIManager.Instance.PopPanel();

        // (선택사항) 창을 닫을 때 저장 확실히 한 번 더 하기
        if (InputHandler.Instance != null)
        {
            InputHandler.Instance.SaveBindingOverrides();
        }
    }

    // 나중에 '기본값으로 초기화' 버튼이 생긴다면 여기에 추가
    /*
    public void OnResetAllBindingsClick()
    {
        InputHandler.Instance.ResetAllBindings();
        // 화면에 있는 모든 RebindButton들에게 UI 갱신하라고 신호 보내기 필요
    }
    */
}