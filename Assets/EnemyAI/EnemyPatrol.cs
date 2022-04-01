using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyPatrol : ActionNode
{
    public Vector2 TargetPosition;
    public float ReachedDistance;
    public float RecognizeDistance;
    public float RecognizeDistanceFace;
    public float RecognizeDistanceBack;
    protected override void OnStart()
    {
        TargetPosition = context.enemyController.GetRandomTarget();
        context.enemyController.SetTarget(TargetPosition);
        context.enemyController.SetSpeed(context.enemyController.EData.PatrolSpeed);
    }

    private float GetRecognizeDistance()
    {
        if (context.enemyController.FacingRight && context.enemyController.Player.transform.position.x < context.transform.position.x)
        {
            return RecognizeDistanceBack;
        }
        if (context.enemyController.FacingRight && context.enemyController.Player.transform.position.x > context.transform.position.x)
        {
            return RecognizeDistanceFace;
        }
        if (context.enemyController.FacingRight == false && context.enemyController.Player.transform.position.x < context.transform.position.x)
        {
            return RecognizeDistanceFace;
        }
        if (context.enemyController.FacingRight == false && context.enemyController.Player.transform.position.x > context.transform.position.x)
        {
            return RecognizeDistanceBack;
        }
        return 0;
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
        RecognizeDistance = GetRecognizeDistance();
        if (TargetDistance(context.enemyController.Player.transform.position) < GetRecognizeDistance())
        {
            return State.Success;
        }
        if (TargetDistance(TargetPosition) < ReachedDistance)
        {
            return State.Failure;
        }
        else
        {
            return State.Running;
        }
        if (TargetDistance(context.enemyController.Player.transform.position) > GetRecognizeDistance())
        {
            return State.Running;
        }
    }
}
