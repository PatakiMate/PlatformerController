using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyBlackboardFiller : MonoBehaviour
{
    public EnemyBlackboard Blackboard;
    void Start()
    {
        Blackboard.Setup(this.transform);
    }
}
