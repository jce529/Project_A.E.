using UnityEngine;

public class PureWater : MonoBehaviour
{
    public WaterController waterController;
    private bool PlayerInRange = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = true;
            Debug.Log("Player entered the corrupted water area.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInRange = false;
            Debug.Log("Player exited the corrupted water area.");
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (PlayerInRange && Input.GetKeyDown(KeyCode.Q))
        {
            if (waterController.corruptedwaterCounter() + waterController.waterCounter() < waterController.bottles.Count)
            {
                waterController.RecoveryWater();
                Debug.Log("Corrupted water recovered.");
            }
            else
            {
                Debug.Log("No more bottles available to recover corrupted water.");
            }
        }
    }
}