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

        Debug.Log("보스를 처치하여 벽이 열렸습니다!");
        gameObject.SetActive(false);
    }
}