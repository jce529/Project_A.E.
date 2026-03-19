using UnityEngine;

// 이 스크립트를 ClearPanel, InventoryPanel 등 각 패널 오브젝트에 붙이세요.
public class UIPanelProperties : MonoBehaviour
{
    [Header("이 패널이 열릴 때 전환될 게임 상태")]
    public GameStateManager.GameState targetState = GameStateManager.GameState.Paused;
}