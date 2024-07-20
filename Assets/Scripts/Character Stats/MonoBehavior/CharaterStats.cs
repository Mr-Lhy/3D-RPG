using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharaterStats : MonoBehaviour
{
    public event Action<int, int> UpdateHealthBarOnAttack;

    public Character_SO templateData;
    public Character_SO charaterData;
    public AttackData_SO attackData;

    [HideInInspector]
    public bool isCritical;

    private void Awake()
    {
        if(templateData != null)
        {
            charaterData = Instantiate(templateData);
        }
    }

    #region Read from Data_SO
    public int MaxHealth
    {
        get{return charaterData != null ? charaterData.maxHealth : 0;}
        set{charaterData.maxHealth = value;}
    }
    public int CurrentHealth
    { 
        get {return charaterData != null ? charaterData.currentHealth : 0;}
        set{charaterData.currentHealth = value;}
    }
    public int BaseDefence
    {
        get{return charaterData != null ? charaterData.baseDefence : 0;}
        set{charaterData.baseDefence = value;}
    }
    public int CurrentDefnece
    {
        get{return charaterData != null ? charaterData.currentDefence : 0;}
        set{charaterData.currentDefence = value;}
    }
    #endregion

    #region Charater Combat

    public void TakeDamage(CharaterStats attacker,CharaterStats defener)
    {
        int damage = Mathf.Max(attacker.CurrentDamage() - defener.CurrentDefnece, 0);
        defener.CurrentHealth = Mathf.Max(defener.CurrentHealth - damage, 0);

        if(attacker.isCritical)
        {
            defener.GetComponent<Animator>().SetTrigger("Hit");
        }
        //update UI
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);
        //æ≠—È÷µupdate
        if(CurrentHealth <= 0)
        {
            attacker.charaterData.UpdateExp(charaterData.killPoint);
        }
    }

    public void TakeDamage(int damage, CharaterStats defener)
    {
        int currentDamage = Mathf.Max(damage-defener.CurrentDefnece, 0);
        defener.CurrentHealth=Mathf.Max(defener.CurrentHealth-damage, 0);
        UpdateHealthBarOnAttack?.Invoke(CurrentHealth, MaxHealth);

        if(defener.CurrentHealth <= 0)
        {
            GameManager.Instance.playerStats.charaterData.UpdateExp(charaterData.killPoint);
        }
    }

    private int CurrentDamage()
    {
        float coreDamage = UnityEngine.Random.Range(attackData.minDamge,attackData.maxDamge);

        if (isCritical)
        {
            coreDamage *= attackData.ctiticalMultiplier;
            Debug.Log("±©ª˜£°" + coreDamage);
        }

        return (int)coreDamage;
    }

    #endregion

}
