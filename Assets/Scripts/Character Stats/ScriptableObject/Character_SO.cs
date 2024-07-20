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
            currentExp -= baseExp; // 使用超出的经验值作为新等级的起点
            currentLevel = Mathf.Clamp(currentLevel + 1, 0, maxLevel);

            baseExp = (int)(baseExp * LevelMultiplier); // 假设有一个方法来计算新的 baseExp
            maxHealth = (int)(maxHealth * LevelMultiplier); // 假设有一个方法来计算新的 maxHealth
            currentHealth = maxHealth; // 重置当前生命值为最大值

            // 记得在这个地方添加其他属性的更新
        }

        if (currentLevel >= maxLevel)
        {
            currentExp = 0; // 如果达到最大等级，可以重置经验值
        }

        Debug.Log("LEVEL UP! Current Level: " + currentLevel + ", Max Health: " + maxHealth);
    }
}
