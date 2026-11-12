using UnityEngine;

// Phase 24: 플레이어가 트리거(Collider2D, isTrigger)에 들어오면 진행 플래그를 세운다.
// 시작/하층/상층/징검다리 끝/폭포 끝/물가르기/보상방 진입 지점에 배치한다.
// 기믹 완료 이벤트가 있으면 이 컴포넌트 대신 Report()를 UnityEvent 에 연결해도 된다.
[RequireComponent(typeof(Collider2D))]
public class Stage1Trigger : MonoBehaviour
{
    [Tooltip("세울 플래그 키 (예: s1.upper.entered). Stage1Progress 상수 참고")]
    public string key;
    [Tooltip("이 키들이 모두 true 일 때만 동작 (순서 보장). 비워두면 무조건")]
    public string[] requires;
    public bool oneShot = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) Report();
    }

    public void Report()
    {
        if (string.IsNullOrEmpty(key) || !Stage1Progress.AllSet(requires)) return;
        Stage1Progress.Set(key, true);
        if (oneShot) enabled = false;
    }
}
