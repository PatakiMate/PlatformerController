using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TheKiwiCoder;

public class EnemyChargeAttack : ActionNode
{
    private Transform _hitOrigin;
    private LayerMask _hitMask;
    private float _rayLenght = 0.5f;
    private float time = 0.2f;
    public float timer;
    private bool _startedOnCooldown;
    protected override void OnStart()
    {
        timer = 0;
        _hitOrigin = context.enemyController.MeleeHitPoint;
        _hitMask = context.enemyController.HitLayer;
        //context.enemyController.ExternalForce = (context.enemyController.Player.transform.position - context.transform.position) * 5;
    }

    private void Hit()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(_hitOrigin.position, Vector2.up, _rayLenght, _hitMask);
        Debug.DrawRay(_hitOrigin.position, Vector2.up * _rayLenght, Color.blue, 2);
        RaycastHit2D hit2 = Physics2D.Raycast(_hitOrigin.position, Vector2.left, _rayLenght, _hitMask);
        Debug.DrawRay(_hitOrigin.position, Vector2.left * _rayLenght, Color.blue, 2);
        RaycastHit2D hit3 = Physics2D.Raycast(_hitOrigin.position, Vector2.right, _rayLenght, _hitMask);
        Debug.DrawRay(_hitOrigin.position, Vector2.right * _rayLenght, Color.blue, 2);
        if (hit1.collider?.tag == "Player" || hit2.collider?.tag == "Player" || hit3.collider?.tag == "Player")
        {
            Debug.Log("Player Hit");
        }

    }

    protected override void OnStop()
    {
        context.enemyController.AnimHit -= Hit;
    }

    protected override State OnUpdate()
    {
            return State.Success;
    }
}
