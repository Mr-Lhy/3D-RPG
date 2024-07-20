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
    //��С������ֵ
    public int minDamge;
    public int maxDamge;

    //�����ٷֱ�
    public float ctiticalMultiplier;
    //������
    public float criticalChance;
}
