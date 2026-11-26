using System;
using System.Collections.Generic;
using UnityEngine;

public enum SpiritFx { Windup, Charge, Move, Repel, Flash, Hit, Death, Groggy, DropStart, DropHit }

// 물의 정령 이펙트 스프라이트 시트를 일회성/루프 오브젝트로 재생한다.
// Entries 는 Tools/Spirit Boss/Wire Water Spirit Effects 메뉴로 채운다.
public class SpiritEffects : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public SpiritFx Id;
        public Sprite[] Stage1;
        public Sprite[] Stage2;
        public float Fps = 12f;
        public float Scale = 1f / 3f; // 이펙트 PPU(100) → 보스 PPU(600) 보정(1/6)의 2배
    }

    public List<Entry> Entries = new List<Entry>();

    private SpiritController _spirit;
    private SpriteRenderer _bossRenderer;

    private void Awake()
    {
        _spirit = GetComponent<SpiritController>();
        _bossRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // follow 가 있으면 그 자식으로 붙는다. loop 이펙트는 호출자가 반환값을 Destroy 해야 한다.
    public GameObject Play(SpiritFx id, Vector3 pos, float rotZ = 0f, float sizeMul = 1f, bool loop = false, Transform follow = null)
    {
        Entry e = Entries.Find(x => x.Id == id);
        if (e == null) return null;

        bool stage2 = _spirit != null && (_spirit.IsStage2 || _spirit.IsDummy);
        Sprite[] frames = stage2 && e.Stage2 != null && e.Stage2.Length > 0 ? e.Stage2 : e.Stage1;
        if (frames == null || frames.Length == 0) return null;

        var go = new GameObject("Fx_" + id);
        go.transform.position = pos;
        go.transform.rotation = Quaternion.Euler(0f, 0f, rotZ);
        go.transform.localScale = Vector3.one * e.Scale * sizeMul;
        if (follow != null) go.transform.SetParent(follow, true);

        var sr = go.AddComponent<SpriteRenderer>();
        if (_bossRenderer != null)
        {
            sr.sortingLayerID = _bossRenderer.sortingLayerID;
            sr.sortingOrder = _bossRenderer.sortingOrder + 1;
        }

        go.AddComponent<SpiritEffectPlayer>().Init(sr, frames, e.Fps, loop);
        return go;
    }
}

public class SpiritEffectPlayer : MonoBehaviour
{
    private SpriteRenderer _sr;
    private Sprite[] _frames;
    private float _fps;
    private bool _loop;
    private float _t;

    public void Init(SpriteRenderer sr, Sprite[] frames, float fps, bool loop)
    {
        _sr = sr;
        _frames = frames;
        _fps = fps;
        _loop = loop;
        _sr.sprite = frames[0];
    }

    private void Update()
    {
        _t += Time.deltaTime * _fps;
        int i = (int)_t;
        if (i >= _frames.Length)
        {
            if (!_loop) { Destroy(gameObject); return; }
            i %= _frames.Length;
        }
        _sr.sprite = _frames[i];
    }
}
