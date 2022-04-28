using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacterController : ObjectController
{
    public Transform StartPoint;
    public Transform EndPoint;
    public float CurrentSpeed;
    public Vector2 _target;
    public bool CanCrawl;
    public Transform Armature;
    [HideInInspector]
    public EnemyData EData;
    [HideInInspector]
    public Transform Player;
    [HideInInspector]
    public Animator Animator;
    public Transform MeleeHitPoint;
    public LayerMask HitLayer;
    public Action AnimHit;
    public float MeleeCooldownTimer;

    public override void Start()
    {
        EData = GetComponent<EnemyData>();
        Player = PlayerObject.Instance.transform;
        Animator = GetComponent<Animator>();
        base.Start();
    }
    private void Update()
    {
        RunTimer();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        SetTarget(_target);
        if (TotalSpeed.x != 0)
        {
            FacingRight = TotalSpeed.x > 0;
        }
        if (FacingRight)
        {
            float angle = Mathf.LerpAngle(Armature.transform.eulerAngles.y, 0, Time.fixedDeltaTime * EData.DirectionChangeLerpTime);
            Armature.transform.eulerAngles = new Vector3(0, angle, 0);
        }
        else
        {
            float angle = Mathf.LerpAngle(Armature.transform.eulerAngles.y, 180, Time.fixedDeltaTime * EData.DirectionChangeLerpTime);
            Armature.transform.eulerAngles = new Vector3(0, angle, 0);
        }
    }


    protected override void UpdateGravity()
    {
        float g = PConfig.Gravity * GravityScale * Time.fixedDeltaTime;
        if (Speed.y > 0)
        {
            Speed.y += g;
        }
        else
        {
            ExternalForce.y += g;
        }
    }
    public override Vector2 Move(Vector2 deltaMove)
    {
        int layer = gameObject.layer;
        gameObject.layer = Physics2D.IgnoreRaycastLayer;
        PreMove(ref deltaMove);
        float xDir = Mathf.Sign(deltaMove.x);
        if (deltaMove.x != 0)
        {
            // Slope checks and processing
            if (deltaMove.y <= 0)
            {
                if (Collisions.onSlope)
                {
                    if (Collisions.groundDirection == xDir)
                    {
                        DescendSlope(ref deltaMove);
                    }
                    else
                    {
                        ClimbSlope(ref deltaMove);
                    }
                }
            }
            //HorizontalCollisions(ref deltaMove);
        }
        HorizontalCollisions(ref deltaMove);
        if (Collisions.onSlope && Collisions.groundAngle >= MinWallAngle && Collisions.groundDirection != xDir && Speed.y < 0)
        {
            Speed.x = 0;
            Vector2 origin = Collisions.groundDirection == -1 ? RayOrigins.bottomRight : RayOrigins.bottomRight;
            Collisions.hHit = Physics2D.Raycast(origin, Vector2.left * Collisions.groundDirection, 1f, CollisionMask);
        }
        if (deltaMove.y > 0 || (deltaMove.y < 0 && (!Collisions.onSlope || deltaMove.x == 0)))
        {
            VerticalCollisions(ref deltaMove);
        }
        if (Collisions.onGround && deltaMove.x != 0 && Speed.y <= 0)
        {
            HandleSlopeChange(ref deltaMove);
        }
        Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        transform.Translate(deltaMove);
        // Checks for ground and ceiling, resets jumps if grounded
        if (Collisions.vHit)
        {
            if ((Collisions.below && TotalSpeed.y < 0) || (Collisions.above && TotalSpeed.y > 0))
            {
                if (!Collisions.onSlope || Collisions.groundAngle < MinWallAngle)
                {
                    Speed.y = 0;
                    ExternalForce.y = 0;
                }
            }
        }
        gameObject.layer = layer;
        return deltaMove;
    }

    public virtual void Walk(float directionX, float directionY = 0)
    {
        if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle && Collisions.groundAngle < MinWallAngle)
        {
            directionX = 0;
        }
        float acc = 0f;
        float dec = 0f;
        acc = EData.AccelerationTime;
        dec = EData.DecelerationTime;

        float accY = 0f;
        float decY = 0f;
        accY = EData.AccelerationTime;
        decY = EData.DecelerationTime;


        //XXX
        if (Mathf.Abs(transform.position.x - _target.x) > 0.5f)
        {
            if (acc > 0)
            {
                if (ExternalForce.x != 0 && Mathf.Sign(ExternalForce.x) != Mathf.Sign(directionX))
                {
                    ExternalForce.x += directionX * (1 / acc) * CurrentSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    if (Mathf.Abs(Speed.x) < CurrentSpeed)
                    {
                        Speed.x += directionX * (1 / acc) * CurrentSpeed * Time.fixedDeltaTime;
                        Speed.x = Mathf.Min(Mathf.Abs(Speed.x), CurrentSpeed * Mathf.Abs(directionX)) *
                            Mathf.Sign(Speed.x);
                    }
                }

            }
            else
            {
                Speed.x = CurrentSpeed * directionX;
            }
            if (directionX == 0 || Mathf.Sign(directionX) != Mathf.Sign(Speed.x))
            {
                if (dec > 0)
                {
                    Speed.x = Mathf.MoveTowards(Speed.x, 0, (1 / dec) * CurrentSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    Speed.x = 0;
                }
            }
        }
    }
    public void SetSpeed(float speed)
    {
        CurrentSpeed = speed;
    }
    public virtual void SetTarget(Vector2 target)
    {
        _target = target;
    }
    public void CallHit()
    {
        AnimHit?.Invoke();
    }

    public Vector2 GetRandomTarget()
    {
        Vector2 output = new Vector2(UnityEngine.Random.Range(StartPoint.position.x, EndPoint.position.x), StartPoint.position.y);
        return output;
    }

    public void RunTimer()
    {
        if(MeleeCooldownTimer > 0)
        {
            MeleeCooldownTimer -= Time.deltaTime;
        }
    }
}
