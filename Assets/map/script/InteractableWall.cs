using UnityEngine;

public class InteractableWall : MonoBehaviour
{
    public GameObject wallTilemap;

    public void UnlockWall()
    {
        if (wallTilemap != null)
        {
            wallTilemap.SetActive(false);
        }

        gameObject.SetActive(false);
    }
}