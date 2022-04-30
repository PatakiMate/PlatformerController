using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyFollowPlayer : ActionNode
{
    private bool _following;
    public float MaxDistance;
    public float ReachedDistance;
    protected override void OnStart()
    {
        _following = true;
        context.enemyController.SetSpeed(context.enemyController.EData.FollowSpeed);
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
        
    }
   

    protected override State OnUpdate()
    {
        //if (_following == false)
        //{
        //    if (TargetDistance(context.enemyController.Player.transform.position) < GetRecognizeDistance())
        //    {
        //        StartFollowing();
        //        return State.Success;
        //    }
        //    if (TargetDistance(context.enemyController.Player.transform.position) > GetRecognizeDistance())
        //    {
        //        return State.Failure;
        //    }
        //}
        if(TargetDistance(context.enemyController.Player.transform.position) < ReachedDistance)
        {
            context.enemyController.Following = false;
            //context.enemyController.Animator.SetTrigger("hit");
            return State.Success;
        }
        if (TargetDistance(context.enemyController.Player.transform.position) < MaxDistance)
        {
            context.enemyController.Following = true;
            if (context.enemyController.CanCrawl)
            {
                //Debug.LogError("SET TARGET PLAYER");
                context.enemyController.SetTarget(new Vector2(context.enemyController.Player.transform.position.x, context.enemyController.Player.transform.position.y));
            } else
            {
                context.enemyController.SetTarget(new Vector2(context.enemyController.Player.transform.position.x, context.transform.position.y));
            }
            return State.Running;
        }
        if (TargetDistance(context.enemyController.Player.transform.position) > MaxDistance)
        {
            context.enemyController.Following = false;
            return State.Failure;
        }
        return State.Failure;
    }
}
