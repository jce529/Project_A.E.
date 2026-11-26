//조건부 사라지는 수문에 넣는 코드
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class InteractableWaterGate : MonoBehaviour
{
    [Header("수문 설정")]
    [Tooltip("이 수문의 고유 ID (저장/불러오기에 사용). 맵마다 겹치지 않게 설정하세요!")]
    public string gateID = "WaterGate_01";

    [Header("제거 시 효과 (선택 사항)")]
    [Tooltip("수문이 열릴 때 재생할 파티클 효과 프리팹 (필요 시 연결)")]
    public GameObject openEffectPrefab;
    [Tooltip("수문이 열릴 때(제거될 때) 추가로 실행할 외부 이벤트들")]
    public UnityEvent onGateOpened;

    private bool isOpened = false;

    void Start()
    {
        // 1. 게임 시작 시, 이 수문이 과거에 이미 제거되었는지 저장 데이터 확인
        CheckSavedState();
    }

    private void CheckSavedState()
    {
        // (예시) PlayerPrefs를 사용한 간단한 상태 저장 확인
        // 현업에서는 별도의 세이브/로드 매니저 클래스(GameManager.SaveData 등)를 참조하는 것이 좋습니다.
        int savedState = PlayerPrefs.GetInt(gateID + "_Opened", 0);

        if (savedState == 1)
        {
            // 이미 이전에 열린(제거된) 수문이라면 게임 시작과 동시에 즉시 삭제
            isOpened = true;
            gameObject.SetActive(false); // 또는 Destroy(gameObject);
        }
    }

    /// <summary>
    /// 외부(버튼, 대화창 시스템, 특정 아이템 획득 시)에서 이 함수를 호출하여 수문을 엽니다.
    /// </summary>
    public void OpenGate()
    {
        // 이미 열린 상태라면 중복 실행 방지
        if (isOpened) return;

        isOpened = true;
        Debug.Log($"[시스템] 수문 '{gateID}'가 제거되었습니다. 보물상자로 가는 길이 열렸습니다!");

        // 2. 수문 제거 상태를 영구적으로 저장 (재생성 방지)
        PlayerPrefs.SetInt(gateID + "_Opened", 1);
        PlayerPrefs.Save();

        // 3. (선택) 시각적 효과 연출
        if (openEffectPrefab != null)
        {
            Instantiate(openEffectPrefab, transform.position, Quaternion.identity);
        }

        // 4. (선택) 추가 이벤트 실행 (예: 퀘스트 목표 달성 갱신 등)
        onGateOpened?.Invoke();

        // 5. 완료 기준 충족: 수문 오브젝트 완전히 비활성화(또는 파괴)하여 상호작용 및 재생성 차단
        gameObject.SetActive(false);
        // Destroy(gameObject)를 써도 무방하나, 추후 복구나 연출 지연이 필요할 땐 SetActive(false)가 안전합니다.
    }

    // 개발/테스트용 편의 기능: 인스펙터에서 저장된 기록 초기화 (우클릭 -> Reset Gate Data)
    [ContextMenu("개발용: 이 수문의 세이브 데이터 초기화")]
    public void ResetSaveData()
    {
        PlayerPrefs.DeleteKey(gateID + "_Opened");
        Debug.Log($"[테스트] 수문 '{gateID}'의 삭제 기록이 초기화되었습니다. 다시 켜집니다.");
    }
}