using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New AttackData",menuName = "Character Stats/Attack Data")]
public class AttackData_SO : ScriptableObject
{
    public float attackRange;
    public float skillRange;
    //CD
    public float coolDown;
    //最小攻击数值
    public int minDamge;
    public int maxDamge;

    //暴击百分比
    public float ctiticalMultiplier;
    //暴击率
    public float criticalChance;
}
