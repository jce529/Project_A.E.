using UnityEngine;

public class PrisonDebuff : MonoBehaviour
{
    float _dotDamage = 3f;
    float _dotInterval = 1f;
    int _mashRequirement = 15;

    public float MashProgress => (float)_mashProgress / _mashRequirement;

    private int _mashProgress = 0;
    private float _dotTimer = 0f;
    private PlayerController _pc;
    private PlayerStats _ps;

    void Awake()
    {
        _pc = GetComponent<PlayerController>();
        _ps = GetComponent<PlayerStats>();

        if (_pc != null)
        {
            _pc.IsImprisoned = true;

            var pa = GetComponent<PlayerAttack>();
            if (pa != null) pa.enabled = false;

            var pab = GetComponent<PlayerAttackBase>();
            if (pab != null) pab.enabled = false;
        }

        if (InputHandler.Instance != null)
            InputHandler.Instance.OnJumpEvent += OnMash;
    }

    void Update()
    {
        _dotTimer += Time.deltaTime;
        if (_dotTimer >= _dotInterval)
        {
            _dotTimer -= _dotInterval;
            if (_ps != null) _ps.TakeDamage(_dotDamage);
        }
    }

    private void OnMash()
    {
        _mashProgress++;
        if (_mashProgress >= _mashRequirement)
            Release();
    }

    private void Release()
    {
        if (_pc != null) _pc.IsImprisoned = false;

        var pa = GetComponent<PlayerAttack>();
        if (pa != null) pa.enabled = true;

        var pab = GetComponent<PlayerAttackBase>();
        if (pab != null) pab.enabled = true;

        Destroy(this);
    }

    void OnDestroy()
    {
        if (InputHandler.Instance != null)
            InputHandler.Instance.OnJumpEvent -= OnMash;

        if (_pc != null) _pc.IsImprisoned = false;

        var pa = GetComponent<PlayerAttack>();
        if (pa != null) pa.enabled = true;

        var pab = GetComponent<PlayerAttackBase>();
        if (pab != null) pab.enabled = true;
    }
}
