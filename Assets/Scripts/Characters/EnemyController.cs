using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public enum EnumStatus
{
    GUARD, PATROL, CHASE, DEAD
}


[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CharaterStats))]
public class EnemyController : MonoBehaviour,IEndGameObserver
{

    
    NavMeshAgent agent;
    Animator animator;
    protected CharaterStats charaterStats;
    Collider collider;

    private EnumStatus enumStatus;

    //设置检查范围
    [Header("Basic Setting")]
    public float sightRadius;

    public bool isGuard;

    protected GameObject attackTarget;

    public float lookAtTime;
    private float remainLookAtTime;

    private float lastAttackTime;

    private float speed;

    private Quaternion guardRotation;

    //巡逻
    [Header("Patrol State")]
    public float patrolRadius;
    private Vector3 wayPoint;
    //原始坐标
    Vector3 guardPos;
    //bool配合动画
    bool isWalk;
    bool isChase;
    bool isFollow;
    bool isDead;

    bool playerDead;



    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        charaterStats = GetComponent<CharaterStats>();
        collider = GetComponent<Collider>();
        guardPos = transform.position;
        guardRotation = transform.rotation;
        speed = agent.speed;
        remainLookAtTime = lookAtTime;
    }

    private void Start()
    {
        if(isGuard)
        {
            enumStatus = EnumStatus.GUARD;
        }
        else
        {
            enumStatus = EnumStatus.PATROL;
            GetNewWayPoint();
        }


        //FIXME:场景切换后修改掉
        GameManager.Instance.AddObserver(this);
    }

    //切换场景时启用
    /*private void OnEnable()
    {
        GameManager.Instance.AddObserver(this);
    }*/

    private void OnDisable()
    {
        if (!GameManager.IsInitialized) return;
        GameManager.Instance.RemoveObserver(this);
    }

    private void Update()
    {
        isDead = charaterStats.CurrentHealth == 0;

        if (!playerDead)
        {
            SwitchStatus();
            SwitchAnimation();
            lastAttackTime -= Time.deltaTime;
        }
            
    }

    void SwitchAnimation()
    {
        animator.SetBool("Walk",isWalk);
        animator.SetBool("Chase",isChase);
        animator.SetBool("Follow",isFollow);
        animator.SetBool("Critical", charaterStats.isCritical);
        animator.SetBool("Death",isDead);
    }

    void SwitchStatus()
    {
        if(isDead)
        {
            enumStatus = EnumStatus.DEAD;
        }
        //如果发现player，就切换为CHASE
        else if(FoundPlayer())
        {
            enumStatus = EnumStatus.CHASE;
        }

        switch (enumStatus)
        {
            case EnumStatus.GUARD:
                isChase = false;

                if(transform.position != guardPos)
                {
                    isWalk = true;
                    agent.isStopped = false;
                    agent.destination = guardPos;

                    if(Vector3.SqrMagnitude(guardPos-transform.position) <= agent.stoppingDistance)
                    {
                        isWalk=false;
                        transform.rotation = Quaternion.Slerp(transform.rotation, guardRotation, 0.01f);
                    }
                }
            break;
            case EnumStatus.PATROL:
                isChase = false;
                agent.speed = speed*0.5f;

                //判断是否到了随机巡逻点
                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    //停留时间
                    if(remainLookAtTime > 0)
                    {
                        remainLookAtTime -= Time.deltaTime;
                    }
                    else
                    {
                        GetNewWayPoint();
                        remainLookAtTime = lookAtTime;
                    }

                    
                }
                else
                {
                    isWalk=true;
                    agent.destination = wayPoint;
                }
            break;
            case EnumStatus.CHASE:
                isWalk = false;
                isChase = true;

                agent.speed = speed;
                
                //配合动画
                if (!FoundPlayer())
                {
                    //拉脱回到上一个状态
                    isFollow = false;
                    if (remainLookAtTime > 0)
                    {
                        agent.destination = transform.position;
                        remainLookAtTime -= Time.deltaTime;
                    }else if (isGuard)
                    {
                        enumStatus = EnumStatus.GUARD;
                    }
                    else
                    {
                        enumStatus=EnumStatus.PATROL;
                    }
                    
                }
                else
                {
                    isFollow = true;
                    agent.isStopped = false;
                    remainLookAtTime = lookAtTime ;
                    //追Player
                    agent.destination = attackTarget.transform.position;
                }
                //在攻击范围内侧攻击
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow=false;
                    agent.isStopped = true;

                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = charaterStats.attackData.coolDown;

                        //暴击判断
                        charaterStats.isCritical = Random.value < charaterStats.attackData.criticalChance;
                        //执行攻击
                        Attck();
                    }
                }


            break;
            case EnumStatus.DEAD:
                collider.enabled = false;
                //agent.enabled = false;
                agent.radius = 0;
                Destroy(gameObject, 2f);
            break;
        }
    }

    void Attck()
    {
        transform.LookAt(attackTarget.transform.position);
        if(TargetInAttackRange() )
        {
            //近身攻击动画
            animator.SetTrigger("Attack");
        }
        if( TargetInSkillRange() )
        {
            //技能攻击动画
            animator.SetTrigger("Skill");
        }
    }

    //是否发现player
    bool FoundPlayer()
    {
        //计算并存储接触球体或位于球体内部的碰撞体。
        var colliders = Physics.OverlapSphere(transform.position, sightRadius);
        foreach (var target in colliders)
        {
            if (target.CompareTag("Player"))
            {
                attackTarget = target.gameObject;
                return true;
            }
        }

        attackTarget = null;
        return false;
    }

    bool TargetInAttackRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= charaterStats.attackData.attackRange;
        }
        else
        {
            return false;
        }
    }
    bool TargetInSkillRange()
    {
        if (attackTarget != null)
        {
            return Vector3.Distance(transform.position, attackTarget.transform.position) <= charaterStats.attackData.skillRange;
        }
        else
        {
            return false;
        }
    }

    void GetNewWayPoint()
    {
        float randomX = Random.Range(-patrolRadius, patrolRadius);
        float randomZ = Random.Range(-patrolRadius, patrolRadius);

        Vector3 randomPoint = new Vector3(guardPos.x+randomX,transform.position.y, guardPos.z+randomZ);
        //用于找到最近的NavMesh表面上的一个点，这个点是由指定的世界空间位置采样得来的。
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint,out  hit,patrolRadius,1)? hit.position:transform.position;
    }

    //在Scene窗口显示范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        
        //巡逻范围
        /*Gizmos.color= Color.green;
        Gizmos.DrawWireSphere(guardPos,potrolRadius);*/
    }


    //Animation Event
    void Hit()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharaterStats>();
            targetStats.TakeDamage(charaterStats, targetStats);
        }
    }

    public void EndNotify()
    {
        //获胜动画
        //停止所有移动
        //停止Agent
        animator.SetBool("Win",true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
