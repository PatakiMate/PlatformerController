using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyFollowPlayer : ActionNode
{
    private bool _following;
    public float RecognizeDistanceFace;
    public float RecognizeDistanceBack;
    public float MaxDistance;
    protected override void OnStart()
    {
    }

    protected override void OnStop()
    {
    }

    public float TargetDistance(Vector2 target)
    {
        return Vector3.Distance(context.transform.position, target);
    }
    private void StartFollowing()
    {
        _following = true;
        context.enemyController.SetSpeed(context.enemyController.EData.FollowSpeed);
    }
    private float GetRecognizeDistance()
    {
        if(context.enemyController.FacingRight && context.enemyController.Player.transform.position.x < context.transform.position.x)
        {
            return RecognizeDistanceBack;
        }
        if (context.enemyController.FacingRight && context.enemyController.Player.transform.position.x > context.transform.position.x)
        {
            return RecognizeDistanceFace;
        }
        if (!context.enemyController.FacingRight && context.enemyController.Player.transform.position.x < context.transform.position.x)
        {
            return RecognizeDistanceBack;
        }
        if (!context.enemyController.FacingRight && context.enemyController.Player.transform.position.x > context.transform.position.x)
        {
            return RecognizeDistanceFace;
        }
        return 0;
    }

    protected override State OnUpdate()
    {
        if (_following == false)
        {
            if (TargetDistance(context.enemyController.Player.transform.position) < GetRecognizeDistance())
            {
                StartFollowing();
                return State.Success;
            }
            if (TargetDistance(context.enemyController.Player.transform.position) > GetRecognizeDistance())
            {
                return State.Failure;
            }
        }
        if (_following)
        {
            if (TargetDistance(context.enemyController.Player.transform.position) < MaxDistance)
            {
                context.enemyController.SetTarget(new Vector2(context.enemyController.Player.transform.position.x, context.transform.position.y));
                return State.Running;
            }
            if (TargetDistance(context.enemyController.Player.transform.position) > MaxDistance)
            {
                _following = false;
                return State.Failure;
            }
        }
        return State.Failure;
    }
}
