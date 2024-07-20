using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class playerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    private GameObject attackTarget;
    private float lastTimer;

    private CharaterStats charaterStats;

    bool isDead;

    float stopDistance;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        charaterStats = GetComponent<CharaterStats>();

        stopDistance = agent.stoppingDistance;
    }

    private void OnEnable()
    {
        MouseManager.Instance.OnMouseClicked += MoveToTarget;
        MouseManager.Instance.OnEnemyClicked += EventAttack;
        GameManager.Instance.RigisterPlayer(charaterStats);
    }

    private void Start()
    {
        SaveManager.Instance.LoadPlayerData();
    }

    private void OnDisable()
    {
        if(!MouseManager.IsInitialized) { return; }
        MouseManager.Instance.OnMouseClicked -= MoveToTarget;
        MouseManager.Instance.OnEnemyClicked -= EventAttack;
    }

    private void Update()
    {
        isDead = charaterStats.CurrentHealth == 0;
        if (isDead)
        {
            GameManager.Instance.NotifyObservers();
        }
        SwitchAnimation();

        lastTimer -= Time.deltaTime;
    }


    //切换动画
    private void SwitchAnimation()
    {
        //NavMeshAgent.velocity 是一个属性，表示代理当前的移动速度向量。这是一个 Vector3，其中包含了代理在 x, y, z 轴方向上的速度分量
        //sqrMagnitude 是上述 Vector3 向量（即速度向量）的平方长度。
        animator.SetFloat("Speed",agent.velocity.sqrMagnitude);

        animator.SetBool("Death",isDead);
    }

    public void MoveToTarget(Vector3 target)
    {
        //停止协同
        StopAllCoroutines();

        if (isDead) return;
        agent.stoppingDistance = stopDistance;
        agent.isStopped = false;

        agent.destination = target;
    }

    private void EventAttack(GameObject target)
    {
        if (isDead) return;

        if(target!=null)
        {
            attackTarget = target;
            charaterStats.isCritical = UnityEngine.Random.value < charaterStats.attackData.criticalChance;
            StartCoroutine(MoveToAttackTarget());
        }
    }

    IEnumerator MoveToAttackTarget()
    {
        agent.isStopped = false;
        agent.stoppingDistance = charaterStats.attackData.attackRange;
        transform.LookAt(attackTarget.transform);

        while(Vector3.Distance(attackTarget.transform.position, transform.position) > charaterStats.attackData.attackRange)
        {
            agent.destination = attackTarget.transform.position;
            yield return null;
        }

        agent.isStopped = true;
        //Attack
        if(lastTimer < 0)
        {
            animator.SetBool("Critical",charaterStats.isCritical);
            animator.SetTrigger("Attack");
            //重置冷却时间
            lastTimer = charaterStats.attackData.coolDown;
        }
    }


    //Animation Event
    void Hit()
    {

        if (attackTarget.CompareTag("Attackable"))
        {
            if (attackTarget.GetComponent<Rock>() && attackTarget.GetComponent<Rock>().rockStates == Rock.RockStates.HitNothing)
            {
                attackTarget.GetComponent<Rock>().rockStates = Rock.RockStates.HitEnemy;
                attackTarget.GetComponent<Rigidbody>().velocity = Vector3.one;
                attackTarget.GetComponent<Rigidbody>().AddForce(transform.forward*20,ForceMode.Impulse);
            }
        }
        else
        {
            var targetStats = attackTarget.GetComponent<CharaterStats>();

            targetStats.TakeDamage(charaterStats, targetStats);
        } 
    }
}
