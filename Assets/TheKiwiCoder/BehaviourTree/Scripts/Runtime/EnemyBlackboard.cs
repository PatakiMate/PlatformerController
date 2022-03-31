using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheKiwiCoder
{
    [CreateAssetMenu()]
    public class EnemyBlackboard : Blackboard
    {
        public string enemyName;
        public Transform EnemyTransform;

        public void Setup(Transform transform)
        {
            this.EnemyTransform = transform;
        }
    }
}