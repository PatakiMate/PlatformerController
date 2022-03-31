using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyPatrol : ActionNode
{
    public Vector2 TargetPosition;
    public float ReachedDistance;
    protected override void OnStart()
    {
        TargetPosition = context.enemyController.GetRandomTarget();
        context.enemyController.SetTarget(TargetPosition);
        context.enemyController.SetSpeed(context.enemyController.EData.PatrolSpeed);
    }

    protected override void OnStop()
    {
    }

    public float TargetDistance(Vector2 target)
    {
        return Vector3.Distance(context.transform.position, target);
    }

    protected override State OnUpdate()
    {
        if(TargetDistance(TargetPosition) < ReachedDistance)
        {
            return State.Success;
        } else
        {
            return State.Running;
        }
    }
}
