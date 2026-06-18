using UnityEngine;
using UnityEngine.UI;

// 일시정지 패널 내 탭 전환을 담당하는 컨트롤러
public class PauseMenuTabController : MonoBehaviour
{
    [Header("탭 버튼")]
    public Button tabGame;
    public Button tabSound;
    public Button tabControls;
    public Button tabGraphics;

    [Header("콘텐츠 패널")]
    public GameObject panelGame;
    public GameObject panelSound;
    public GameObject panelControls;
    public GameObject panelGraphics;

    [Header("탭 색상")]
    public Color selectedColor = new Color(0.15f, 0.45f, 0.75f, 1f);
    public Color normalColor   = new Color(0.08f, 0.10f, 0.14f, 0.9f);

    private void OnEnable()
    {
        // 패널이 열릴 때마다 첫 번째 탭(게임)으로 초기화
        ShowTab(tabGame, panelGame);
    }

    // 버튼 OnClick에 연결
    public void OnTabGameClick()     => ShowTab(tabGame,     panelGame);
    public void OnTabSoundClick()    => ShowTab(tabSound,    panelSound);
    public void OnTabControlsClick() => ShowTab(tabControls, panelControls);
    public void OnTabGraphicsClick() => ShowTab(tabGraphics, panelGraphics);

    private void ShowTab(Button activeTab, GameObject activePanel)
    {
        SetPanel(panelGame,     activePanel == panelGame);
        SetPanel(panelSound,    activePanel == panelSound);
        SetPanel(panelControls, activePanel == panelControls);
        SetPanel(panelGraphics, activePanel == panelGraphics);

        SetTabColor(tabGame,     activeTab == tabGame);
        SetTabColor(tabSound,    activeTab == tabSound);
        SetTabColor(tabControls, activeTab == tabControls);
        SetTabColor(tabGraphics, activeTab == tabGraphics);
    }

    private void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    private void SetTabColor(Button btn, bool selected)
    {
        if (btn == null) return;
        var img = btn.GetComponent<Image>();
        if (img != null) img.color = selected ? selectedColor : normalColor;
    }
}
