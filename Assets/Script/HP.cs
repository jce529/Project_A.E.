using UnityEditor;

using UnityEngine;

public class HP : MonoBehaviour
{
    public float maxHealth;
    private float currentHealth;
    GameObject thisObject;
    public void setMaxHP(float HP)
    {
        maxHealth = HP;
    }
    public void heal(int amount)
    {
        if (currentHealth + amount < maxHealth)
            currentHealth = +amount;
        else currentHealth = maxHealth;
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