using System;
using UnityEngine;

/// <summary>가고일=잡몹 나무. 플레이어가 트리거 구역에 2초 연속 체류하면 변신. 2초 전 이탈 시 타이머 초기화.
/// 이 오브젝트에 isTrigger Collider2D 필요.</summary>
[RequireComponent(typeof(Collider2D))]
public class TreeMonsterTransform : MonoBehaviour
{
    [SerializeField] private float dwellSeconds = 2f;
    [SerializeField] private string playerTag = "Player";
    [Tooltip("변신 전 나무 오브젝트(변신 시 비활성)")]
    [SerializeField] private GameObject treeObject;
    [Tooltip("변신 후 몬스터 오브젝트(변신 시 활성)")]
    [SerializeField] private GameObject monsterObject;

    public bool Transformed { get; private set; }
    public float Timer { get; private set; }
    public event Action<bool> Changed;

    private bool playerInside;

    private void Start()
    {
        if (Gimmick2Store.Get(Gimmick2Store.TreeTransformed)) Apply(false);
        else if (monsterObject != null) monsterObject.SetActive(false);
    }

    private void Update()
    {
        if (Transformed || !playerInside) return;
        Timer += Time.deltaTime;
        if (Timer >= dwellSeconds) Apply(true);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = true;
        Timer = 0f;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInside = false;
        Timer = 0f;
    }

    private void Apply(bool fire)
    {
        Transformed = true;
        if (treeObject != null) treeObject.SetActive(false);
        if (monsterObject != null) monsterObject.SetActive(true);
        if (!fire) return;
        Gimmick2Store.Set(Gimmick2Store.TreeTransformed, true);
        Changed?.Invoke(true);
    }
}
