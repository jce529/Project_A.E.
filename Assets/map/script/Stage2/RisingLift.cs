using System;
using UnityEngine;

/// <summary>상승 장치. lowPoint↔highPoint 사이를 이동. 보통 WaterWheel 이 제어한다.</summary>
public class RisingLift : MonoBehaviour
{
    [SerializeField] private Transform platform;
    [SerializeField] private Transform lowPoint;
    [SerializeField] private Transform highPoint;
    [SerializeField] private float speed = 2f;

    public bool Raised { get; private set; }
    public event Action<bool> Changed;

    private void Awake()
    {
        if (platform == null) platform = transform;
    }

    private void Update()
    {
        Transform target = Raised ? highPoint : lowPoint;
        if (target == null) return;
        platform.position = Vector3.MoveTowards(platform.position, target.position, speed * Time.deltaTime);
    }

    /// <summary>fire=false 면 로드 복원(즉시 위치 이동, 이벤트 없음).</summary>
    public void SetRaised(bool raised, bool fire)
    {
        if (Raised == raised) return;
        Raised = raised;
        Gimmick2Store.Set(Gimmick2Store.LiftRaised, raised);
        if (!fire)
        {
            Transform t = raised ? highPoint : lowPoint;
            if (t != null) platform.position = t.position;
            return;
        }
        Changed?.Invoke(raised);
    }
}
