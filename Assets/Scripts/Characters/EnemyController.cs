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

    //���ü�鷶Χ
    [Header("Basic Setting")]
    public float sightRadius;

    public bool isGuard;

    protected GameObject attackTarget;

    public float lookAtTime;
    private float remainLookAtTime;

    private float lastAttackTime;

    private float speed;

    private Quaternion guardRotation;

    //Ѳ��
    [Header("Patrol State")]
    public float patrolRadius;
    private Vector3 wayPoint;
    //ԭʼ����
    Vector3 guardPos;
    //bool��϶���
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


        //FIXME:�����л����޸ĵ�
        GameManager.Instance.AddObserver(this);
    }

    //�л�����ʱ����
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
        //�������player�����л�ΪCHASE
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

                //�ж��Ƿ������Ѳ�ߵ�
                if(Vector3.Distance(wayPoint,transform.position) <= agent.stoppingDistance)
                {
                    isWalk = false;
                    //ͣ��ʱ��
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
                
                //��϶���
                if (!FoundPlayer())
                {
                    //���ѻص���һ��״̬
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
                    //׷Player
                    agent.destination = attackTarget.transform.position;
                }
                //�ڹ�����Χ�ڲ๥��
                if (TargetInAttackRange() || TargetInSkillRange())
                {
                    isFollow=false;
                    agent.isStopped = true;

                    if(lastAttackTime < 0)
                    {
                        lastAttackTime = charaterStats.attackData.coolDown;

                        //�����ж�
                        charaterStats.isCritical = Random.value < charaterStats.attackData.criticalChance;
                        //ִ�й���
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
            //����������
            animator.SetTrigger("Attack");
        }
        if( TargetInSkillRange() )
        {
            //���ܹ�������
            animator.SetTrigger("Skill");
        }
    }

    //�Ƿ���player
    bool FoundPlayer()
    {
        //���㲢�洢�Ӵ������λ�������ڲ�����ײ�塣
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
        //�����ҵ������NavMesh�����ϵ�һ���㣬���������ָ��������ռ�λ�ò��������ġ�
        NavMeshHit hit;
        wayPoint = NavMesh.SamplePosition(randomPoint,out  hit,patrolRadius,1)? hit.position:transform.position;
    }

    //��Scene������ʾ��Χ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
        
        //Ѳ�߷�Χ
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
        //��ʤ����
        //ֹͣ�����ƶ�
        //ֹͣAgent
        animator.SetBool("Win",true);
        playerDead = true;
        isChase = false;
        isWalk = false;
        attackTarget = null;
    }
}
