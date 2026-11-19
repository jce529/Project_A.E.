using UnityEngine;
using TMPro;

public class OverwriteConfirmPanel : MonoBehaviour
{
    public const string TitleCopy   = "슬롯 덮어쓰기";
    public const string BodyCopy    = "이 슬롯을 덮어쓰고 새 게임을 시작하시겠습니까?";
    public const string ConfirmCopy = "덮어쓰고 시작";
    public const string CancelCopy  = "취소";

    [Header("Texts")]
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public TMP_Text confirmButtonText;
    public TMP_Text cancelButtonText;

    private int _pendingSlot = -1;
    private System.Action<int> _onConfirm;

    private void OnEnable()
    {
        if (titleText != null) titleText.text = TitleCopy;
        if (bodyText != null) bodyText.text = BodyCopy;
        if (confirmButtonText != null) confirmButtonText.text = ConfirmCopy;
        if (cancelButtonText != null) cancelButtonText.text = CancelCopy;
    }

    public void Open(int slot, System.Action<int> onConfirm)
    {
        _pendingSlot = slot;
        _onConfirm = onConfirm;
        gameObject.SetActive(true);
    }

    public void OnClickConfirm()
    {
        int slot = _pendingSlot;
        System.Action<int> callback = _onConfirm;
        Close();
        if (slot >= 0 && callback != null) callback(slot);
    }

    public void OnClickCancel()
    {
        Close();
    }

    private void Close()
    {
        _pendingSlot = -1;
        _onConfirm = null;
        gameObject.SetActive(false);
    }
}
