using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyChargeAttack : ActionNode
{
    private float time = 0.2f;
    public float timer;
    private bool _startedOnCooldown;

    private float resetTimer;
    private float resetTime = 0.2f;
    protected override void OnStart()
    {
        timer = 0;
        context.enemyController.Block = true;
        if (context.enemyController.MeleeCooldownTimer > 0)
        {
            _startedOnCooldown = true;
        }
        else
        {
            _startedOnCooldown = false;
            context.enemyController.MeleeCooldownTimer = context.enemyController.EData.ChargeCooldownTime;
            resetTimer = resetTime;
            context.enemyController.Charge = true;
            context.enemyController.Speed = (context.enemyController.Player.transform.position - context.transform.position) * 4;
        }
       
    }

    protected override void OnStop()
    {
        context.enemyController.Block = false;
    }

    protected override State OnUpdate()
    {

        if(resetTimer > 0)
        {
            resetTimer -= Time.deltaTime;
        } else
        {
            context.enemyController.Charge = false;
        }

        if (_startedOnCooldown && resetTimer == 0)
        {
            return State.Success;
        }
        if (timer < time)
        {
            timer += Time.deltaTime;
            return State.Running;
        }
        else
        {
            return State.Success;
        }
    }
}
