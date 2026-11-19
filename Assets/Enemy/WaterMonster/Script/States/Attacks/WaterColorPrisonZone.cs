using System.Collections.Generic;
using UnityEngine;

public enum PrisonColor { Red, Blue }

public class WaterColorPrisonZone : MonoBehaviour
{
    public static List<WaterColorPrisonZone> ActiveZones = new List<WaterColorPrisonZone>();

    public PrisonColor ZoneColor { get; private set; }
    public bool IsPlayerInside { get; private set; }

    private SpriteRenderer _sr;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        ActiveZones.Add(this);
    }

    void OnDisable()
    {
        ActiveZones.Remove(this);
        IsPlayerInside = false;
    }

    public void SetColor(PrisonColor c)
    {
        ZoneColor = c;
        if (_sr != null)
            _sr.color = c == PrisonColor.Red
                ? new Color(1f, 0.2f, 0.2f, 0.5f)
                : new Color(0.2f, 0.4f, 1f, 0.5f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            IsPlayerInside = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            IsPlayerInside = false;
    }
}
