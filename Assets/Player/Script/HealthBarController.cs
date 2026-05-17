/*
 *  Author: ariel oliveira [o.arielg@gmail.com]
 */

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    private GameObject[] heartContainers;
    private Image[] heartFills;

    public Transform heartsParent;
    public GameObject heartContainerPrefab;

    [Header("하트 단위 조정")]
    public int healthPerHeart;

    private IEnumerator Start()
    {
        yield return new WaitUntil(() => PlayerStats.Instance != null);
        // Should I use lists? Maybe :)
        int heartCount = Mathf.CeilToInt(PlayerStats.Instance.MaxTotalHealth / (float)healthPerHeart);

        heartContainers = new GameObject[heartCount];
        heartFills = new Image[heartCount];

        PlayerStats.Instance.onHealthChangedCallback += UpdateHeartsHUD;
        InstantiateHeartContainers();
        UpdateHeartsHUD();
    }

    public void UpdateHeartsHUD()
    {
        SetHeartContainers();
        SetFilledHearts();
    }

    void SetHeartContainers()
    {
        int heartCount = Mathf.CeilToInt(PlayerStats.Instance.MaxHealth / (float)healthPerHeart);

        for (int i = 0; i < heartContainers.Length; i++)
        {
            if (i < PlayerStats.Instance.MaxHealth)
            {
                heartContainers[i].SetActive(i < heartCount);
            }
        }
    }

    void SetFilledHearts()
    {
        float currentHealth = PlayerStats.Instance.Health;

        for (int i = 0; i < heartFills.Length; i++)
        {
            // 이 하트가 담당하는 체력 범위
            float heartStart = i * healthPerHeart;
            float heartEnd = (i + 1) * healthPerHeart;

            // 현재 체력이 이 하트 범위 안에서 차지하는 비율
            float fillAmount = Mathf.Clamp01((currentHealth - heartStart) / healthPerHeart);

            heartFills[i].fillAmount = fillAmount;
        }
    }

    void InstantiateHeartContainers()
    {
        for (int i = 0; i < heartContainers.Length; i++)
        {
            GameObject temp = Instantiate(heartContainerPrefab, heartsParent);
            heartContainers[i] = temp;
            heartFills[i] = temp.transform.Find("HeartFill").GetComponent<Image>();
        }
    }
}
