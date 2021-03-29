﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy_Controller : MonoBehaviour
{
    public float lookRadius = 10f;
    const float locoAnimSmoothTime = 0.1f;

    Enemy enemyInteractor;

    Transform target;
    NavMeshAgent agent;
    CharacterCombat combat;
    Character_Stats stats;
    PlayerManager playerManager;

    Animator enemyAnim;

    // Start is called before the first frame update
    void Start()
    {
        playerManager = PlayerManager.instance;

        agent = GetComponent<NavMeshAgent>();
        combat = GetComponent<CharacterCombat>();
        stats = GetComponent<Character_Stats>();
        enemyAnim = GetComponentInChildren<Animator>();
        enemyInteractor = GetComponent<Enemy>();

        agent.updateRotation = false;

        agent.speed = stats.moveSpeed.GetValue();

        foreach (Ability ability in stats.GetComponent<Character_Stats>().abilities)
        {
            ability.cooldownTimer = 0;
        }

        target = playerManager.activePerson.transform;
    }

    // Update is called once per frame
    void Update()
    {
        float speedPercent = agent.velocity.magnitude / agent.speed;
        enemyAnim.SetFloat("speedPercent", speedPercent, locoAnimSmoothTime, Time.deltaTime);

        if (GetComponent<CharacterCombat>().castTime > 0)
        {
            target = null;
            agent.velocity = Vector3.zero;
            return;
        }

        target = playerManager.activePerson.transform;

        float distance = Vector3.Distance(target.position, transform.position);
        if(distance <= lookRadius)
        {
            agent.SetDestination(target.position);

            if(distance <= agent.stoppingDistance)
            {
                //attack
                Character_Stats targetStats = target.GetComponent<Character_Stats>();
                if(targetStats != null)
                {
                    if (stats.abilities[0].cooldownTimer < 0 && combat.castTime < 0)
                    {
                        enemyAnim.SetTrigger("ability1");
                        stats.abilities[0].Use(gameObject);
                    }

                    enemyAnim.SetTrigger("basicAttack");
                    combat.Attack(targetStats);
                }

            }

            FaceTarget();
        }
        

    }

    //make sure to face target when attacking
    void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, lookRadius);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Vector3 direction = transform.TransformDirection(Vector3.forward) * 2;
        Gizmos.DrawRay(transform.position, direction);
    }
}
