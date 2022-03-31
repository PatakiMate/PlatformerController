using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyCheckPlayerDistance : ActionNode
{
    public float IdealDistance;
    protected override void OnStart() {
    }

    protected override void OnStop() {
    }

    protected override State OnUpdate() {
        if (Vector3.Distance(PlayerObject.Instance.transform.position, context.transform.position) < IdealDistance)
        {
            return State.Success;
        } else
        {
            return State.Failure;
        }
    }
}
