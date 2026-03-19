using UnityEngine;

/// <summary>
/// RootSpike 프리팹의 루트에 부착.
/// 프리팹 구조:
///   RootSpike (루트, 이 스크립트 부착)
///   ├── Pillar    (기둥 SpriteRenderer, Pivot: Center)
///   └── SpikeHead (뾰족 끝 SpriteRenderer, Rotation Z: 45, Pivot: Center)
///
/// RootSpikeStrategy에서 SetHeight(spikeHeight) 호출 시
/// 바닥~플레이어 전체 높이만큼 Pillar를 늘리고
/// SpikeHead를 그 위에 올려 조합합니다.
/// </summary>
public class RootSpikeVisual : MonoBehaviour
{
    [Header("자식 오브젝트 연결 (Inspector에서 직접 할당)")]
    public Transform pillar;    // Pillar 자식 오브젝트
    public Transform spikeHead; // SpikeHead 자식 오브젝트

    [Header("가시 끝 고정 크기")]
    [Tooltip("SpikeHead의 고정 높이. Pillar는 전체 높이에서 이 값을 뺀 만큼 늘어납니다.")]
    public float spikeHeadHeight = 0.8f;

    /// <summary>
    /// 바닥~플레이어 전체 높이를 받아 Pillar + SpikeHead 크기/위치를 설정합니다.
    /// 루트 오브젝트는 중심 Y(바닥 + 높이/2)에 위치해야 합니다.
    /// </summary>
    public void SetHeight(float totalHeight)
    {
        if (pillar == null || spikeHead == null)
        {
            Debug.LogWarning("[RootSpikeVisual] Pillar 또는 SpikeHead가 연결되지 않았습니다!");
            return;
        }

        // 기둥 높이 = 전체 높이 - 가시 끝 높이
        float pillarHeight = Mathf.Max(totalHeight - spikeHeadHeight, 0.1f);

        // ── Pillar ────────────────────────────────────────────────────
        // Pivot이 Center이므로, 루트 중심 기준으로 아래쪽 절반에 위치
        // 루트는 centerY(바닥 + totalHeight/2)에 있으므로
        // Pillar 중심 = 루트 기준 -(totalHeight/2) + pillarHeight/2
        float pillarLocalY = -(totalHeight / 2f) + pillarHeight / 2f;
        pillar.localPosition = new Vector3(0f, pillarLocalY, 0f);
        pillar.localScale = new Vector3(pillar.localScale.x, pillarHeight, 1f);

        // ── SpikeHead ─────────────────────────────────────────────────
        // 기둥 꼭대기 위에 딱 붙음
        // 기둥 꼭대기 Y(루트 기준) = pillarLocalY + pillarHeight/2
        float pillarTopLocalY = pillarLocalY + pillarHeight / 2f;
        float spikeHeadLocalY = pillarTopLocalY + spikeHeadHeight / 2f;
        spikeHead.localPosition = new Vector3(0f, spikeHeadLocalY, 0f);
        // SpikeHead 스케일은 Inspector에서 설정한 값 유지 (가로 = 세로 = 고정)
    }
}