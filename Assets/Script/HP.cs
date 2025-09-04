using UnityEditor;
using UnityEngine;

public class HP : MonoBehaviour
{
    public float maxHealth;
    private float currentHealth;
    GameObject thisObject;

    private void Start()
    {
        currentHealth = maxHealth;
    }


    public void setHP(float HP)
    {
        maxHealth = HP;
    }
    public void takeDmg(float dmg)
    {
        currentHealth -= dmg;
        if (currentHealth <= 0)
        {
            die();
        }
    }

    public void die()
    {
        Destroy(thisObject);
    }
}
