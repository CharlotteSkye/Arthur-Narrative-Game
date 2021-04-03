﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


//This controls combat between 2 characters
public class CharacterCombat : MonoBehaviour
{
    Character_Stats myStats;
    CharacterAnimator anim;
    
    float nextTime = 0;
    public float attackSpeed; 
    private float attackCooldown = 0f;

    const float combatCooldown = 5;
    float lastAttackTime;

    [HideInInspector]
    public bool rooted;

    [HideInInspector]
    public bool silenced;

    private float castTime;

    public float attackDelay = 0.6f;

    public List<CC_Effect> cc_Effects = new List<CC_Effect>();

    public bool InCombat { get; private set; }
    public float CastTime { get => castTime; set => castTime = value; }

    private void Start()
    {
        myStats = GetComponent<Character_Stats>();
        attackSpeed = myStats.attackSpeed.GetValue();
        anim = GetComponent<CharacterAnimator>();
    }

    private void Update()
    {
        attackSpeed = myStats.attackSpeed.GetValue();
    
        attackCooldown -= Time.deltaTime;
        castTime -= Time.deltaTime;

        if (Time.time - lastAttackTime > combatCooldown)
        {
            InCombat = false;
        }

        foreach(Ability ability in myStats.abilities)
        {
            ability.cooldownTimer -= Time.deltaTime;
        }

        ManageBuffs();
        Manage_CC();
    }

    //method for attackng another character
    public void Attack (Character_Stats targetStats)
    {
        //if attack not on cooldown
        if (attackCooldown <= 0f)
        {
            if (anim != null) anim.characterAnim.SetBool("basicAttack", true);
            
            //tells attack target's stats that they take damage after small delay
            StartCoroutine(DoDamage(targetStats, attackDelay));
                
            //resets attack timer
            attackCooldown = 1f / attackSpeed;

            InCombat = true;
            lastAttackTime = Time.time;
        }
    }

    public Character_Stats GetMyStats(){ return myStats; }

    public void AbilityHit(Character_Stats targetStats, float mod)
    {
        //tells target's stats that they take damage equal myStats damage variable
        targetStats.TakeDam(myStats.damage.GetValue() + mod);
    }

    public void AbilityHeal(Character_Stats targetStats, float healAmount)
    {
        //tells target's stats that they heal for healAmount HP
        targetStats.Heal(healAmount);
    }


    public void Manage_CC()
    {
        for (int i = 0; i < cc_Effects.Count; i++)
        {
            CC_Effect effect = cc_Effects[i];

            effect.durationTimer -= Time.deltaTime;

            switch (effect.affect)
            {
                case StatusEffects.Stun:
                    attackCooldown = effect.durationTimer;
                    castTime = effect.durationTimer;
                    break;
                case StatusEffects.Silence:
                    silenced = true;
                    break;
                case StatusEffects.Root:
                    rooted = true;
                    break;
                case StatusEffects.Disarm:
                    attackCooldown = effect.durationTimer;
                    break;
            }

            //removes buff if it's duration is 0
            if (effect.durationTimer <= 0)
            {
                if (effect.affect == StatusEffects.Root)
                    rooted = false;
                if (effect.affect == StatusEffects.Silence)
                    silenced = false;

                myStats.buffs.RemoveAt(i);
            }
        }
    }
    void ManageBuffs()
    {
        for (int i = 0; i < myStats.buffs.Count; i++)
        {
            BufforDebuff buff = myStats.buffs[i];

            buff.durationTimer -= Time.deltaTime;

            //if the buff is a ramping effect that adds every second, then update the stat every second here
            if (buff.ramping)
            {
                if (Time.time >= nextTime)
                {
                    switch (buff.affects)
                    {
                        case StatBuffs.Armor:
                            myStats.armor.AddModifier(buff.amount);
                            break;
                        case StatBuffs.AttackSpeed:
                            myStats.attackSpeed.AddModifier(buff.amount);
                            break;
                        case StatBuffs.Health:
                            if (buff.amount < 0)
                                myStats.TakePureDam(buff.amount);
                            else
                                myStats.Heal(buff.amount);
                            break;
                        case StatBuffs.Damage:
                            myStats.damage.AddModifier(buff.amount);
                            break;
                        case StatBuffs.MoveSpeed:
                            myStats.moveSpeed.AddModifier(buff.amount);
                            break;

                            
                    }
                    //do something here every interval seconds
                    nextTime = Mathf.FloorToInt(Time.time) + 1;
                }

            }
            
            //removes buff if it's duration is 0
            if (buff.durationTimer <= 0)
            {
                switch (buff.affects)
                {
                    case StatBuffs.Armor:
                        if (buff.ramping)
                        {
                            for (int y = 0; y < buff.duration; y++)
                            {
                                myStats.armor.RemoveModifier(buff.amount);
                            }
                        }
                        else
                            myStats.armor.RemoveModifier(buff.amount);
                        break;
                    
                    
                    case StatBuffs.AttackSpeed:
                        if (buff.ramping)
                        {
                            for (int y = 0; y < buff.duration; y++)
                            {
                                myStats.attackSpeed.RemoveModifier(buff.amount);
                            }
                        }
                        else
                            myStats.attackSpeed.RemoveModifier(buff.amount);
                        break;
                    
                    
                    case StatBuffs.Damage:
                        if (buff.ramping)
                        {
                            for (int y = 0; y < buff.duration; y++)
                            {
                                myStats.damage.RemoveModifier(buff.amount);
                            }
                        }
                        else
                            myStats.damage.RemoveModifier(buff.amount);
                        break;
                    
                    
                    case StatBuffs.MoveSpeed:
                        if (buff.ramping)
                        {
                            for (int y = 0; y < buff.duration; y++)
                            {
                                myStats.moveSpeed.RemoveModifier(buff.amount);
                            }
                        }
                        else
                            myStats.moveSpeed.RemoveModifier(buff.amount);
                        break;
                }
                myStats.buffs.RemoveAt(i);
            }
        }
    }

    public float GetAttackCooldown() { return attackCooldown; }
    public void SetAttackCooldown(float newAttackCooldown) {  attackCooldown = newAttackCooldown; }


    IEnumerator DoDamage (Character_Stats stats, float delay)
    {
        yield return new WaitForSeconds(delay);

        stats.TakeDam(myStats.damage.GetValue());
    }

    public void UseAbility(CharacterCombat target, Ability ability)
    {
        StartCoroutine(ability.UseAbility(target));
    }

}
