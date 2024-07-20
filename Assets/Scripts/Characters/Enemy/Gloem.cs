using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Gloem : EnemyController
{
    //»÷ÍËµÄÁ¦
    [Header("Skill")]
    public float kickForce = 25;

    public GameObject rockPrefab;

    public Transform handPos;

    private GameObject defaultTarget;

    private void Start()
    {
        defaultTarget = FindObjectOfType<playerController>().gameObject;
    }

    //Animation Event
    public void KickOff()
    {
        if (attackTarget != null && transform.IsFacingTarget(attackTarget.transform))
        {
            var targetStats = attackTarget.GetComponent<CharaterStats>();

            Vector3 direction = attackTarget.transform.position - transform.position;
            direction.Normalize();

            targetStats.GetComponent<NavMeshAgent>().isStopped = true;
            targetStats.GetComponent<NavMeshAgent>().velocity = direction * kickForce;
            targetStats.TakeDamage(charaterStats, targetStats);
        }
    }


    //Animation Event
    public void ThrowRock()
    {
        GameObject target = attackTarget != null ? attackTarget:defaultTarget;

        
            var rock = Instantiate(rockPrefab, handPos.position, Quaternion.identity);
            rock.GetComponent<Rock>().target = target;
        
    }
}
