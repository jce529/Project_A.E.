using UnityEngine;

// Phase 24: 워터 슬라이드 결과 어댑터. 전금재 슬라이드 기믹의 성공/실패 지점에서
// OnSuccess() / OnFail() 을 호출(UnityEvent 또는 Stage1 트리거)한다.
//  - 성공: s1.slide.success = true
//  - 실패: 진행 플래그를 하층 기준으로 되돌리고, 하층 체크포인트로 리스폰시킨다.
public class Stage1SlideResult : MonoBehaviour
{
    [Tooltip("하층 체크포인트(씬의 Checkpoint 오브젝트 Transform)")]
    public Transform lowerCheckpoint;
    public Transform player;   // 비워두면 Player 태그로 찾는다

    public void OnSuccess()
    {
        if (!Stage1Progress.Get(Stage1Progress.UpperEntered)) return;
        Stage1Progress.Set(Stage1Progress.SlideSuccess, true);
    }

    public void OnFail()
    {
        Stage1Progress.ResetToLower();

        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }
        if (player == null) return;

        var respawn = player.GetComponent<PlayerRespawn>();
        if (respawn == null) { Debug.LogError("[Stage1SlideResult] PlayerRespawn 없음"); return; }
        if (lowerCheckpoint != null) respawn.UpdateCheckpoint(lowerCheckpoint);
        respawn.RespawnPosition();
    }
}
