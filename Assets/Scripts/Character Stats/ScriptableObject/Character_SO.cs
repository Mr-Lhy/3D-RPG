using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Data",menuName ="Character Stats/Data")]
public class Character_SO : ScriptableObject
{
    [Header("Stats Info")]
    public int maxHealth;
    public int currentHealth;
    public int baseDefence;
    public int currentDefence;

    [Header("Kill")]
    public int killPoint;

    [Header("Level")]
    public int currentLevel;
    public int maxLevel;
    public int baseExp;
    public int currentExp;
    public float levelBuff;

    public float LevelMultiplier 
    {
        get { return 1+(currentLevel -1)*levelBuff; }
    }

    public void UpdateExp(int point)
    {
        currentExp += point;

        if (currentExp > baseExp)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        while (currentExp >= baseExp && currentLevel < maxLevel)
        {
            currentExp -= baseExp; // ʹ�ó����ľ���ֵ��Ϊ�µȼ������
            currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);

            baseExp = (int)(baseExp * LevelMultiplier); // ������һ�������������µ� baseExp
            maxHealth = (int)(maxHealth * LevelMultiplier); // ������һ�������������µ� maxHealth
            currentHealth = maxHealth; // ���õ�ǰ����ֵΪ���ֵ

            // �ǵ�������ط�����������Եĸ���
        }

        if (currentLevel >= maxLevel)
        {
            currentExp = 0; // ����ﵽ���ȼ����������þ���ֵ
        }

        Debug.Log("LEVEL UP! Current Level: " + currentLevel + ", Max Health: " + maxHealth);
    }
}
