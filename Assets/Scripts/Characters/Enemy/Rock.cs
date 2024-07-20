using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Rock : MonoBehaviour
{
    public enum RockStates { HitPlayer,HitEnemy,HitNothing}

    Rigidbody rb;

    public RockStates rockStates;

    [Header("Basic Settings")]
    public float force;
    public int damage;

    public GameObject target;

    Vector3 direction;

    public GameObject breakEffect;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rockStates = RockStates.HitPlayer;
        rb.velocity = Vector3.one;
        FlyToTarget();
    }

    private void FixedUpdate()
    {
        if(rb.velocity.sqrMagnitude < 1f)
        {
            rockStates=RockStates.HitNothing;
        }
    }

    void FlyToTarget()
    {

        direction = (target.transform.position - transform.position + Vector3.up).normalized;

        rb.AddForce(direction*force,ForceMode.Impulse);
    }


    private void OnCollisionEnter(Collision collision)
    {
        switch(rockStates)
        {
            case RockStates.HitPlayer:
                if (collision.gameObject.CompareTag("Player"))
                {
                    collision.gameObject.GetComponent<NavMeshAgent>().isStopped = true;
                    collision.gameObject.GetComponent <NavMeshAgent>().velocity = direction*force;

                    collision.gameObject.GetComponent<Animator>().SetTrigger("Dizzy");
                    collision.gameObject.GetComponent<CharaterStats>().TakeDamage(damage, collision.gameObject.GetComponent<CharaterStats>());

                    rockStates = RockStates.HitNothing;
                }
                break;

            case RockStates.HitEnemy:
                if (collision.gameObject.GetComponent<Gloem>())
                {
                    var otherStats = collision.gameObject.GetComponent<CharaterStats>();
                    otherStats.TakeDamage(damage, otherStats);
                    Instantiate(breakEffect, transform.position, Quaternion.identity);
                    Destroy(gameObject);
                }

                break;
        }
    }
}
