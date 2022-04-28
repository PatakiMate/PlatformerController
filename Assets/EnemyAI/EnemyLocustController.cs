using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyLocustController : EnemyCharacterController
{
    private float _currentSpeed;
    private Vector2 _targetOverride;
    private bool _crawling;
    public bool _climbing;

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
        if (CanCrawl == false)
        {
            if (Mathf.Abs(transform.position.x - _target.x) > 0.5f)
            {
                Walk(transform.position.x < _target.x ? 1 : -1);
            }
        }
        else
        {
            bool shouldClimb = false;
            if (transform.position.x < _target.x && transform.position.x < Collisions.hHit.point.x)
            {
                shouldClimb = true;
            }
            if (transform.position.x > _target.x && transform.position.x > Collisions.hHit.point.x)
            {
                shouldClimb = true;
            }
            //BOTH
            if (Collisions.hHit && Collisions.vHit)
            {
                _climbing = false;
                if (Mathf.Abs(transform.position.x - _target.x) > Mathf.Abs(transform.position.y - _target.y) && shouldClimb == false)
                {
                    _crawling = false;
                    Walk(transform.position.x < _target.x ? 1 : -1, 0);
                }
                else
                {
                    _crawling = true;
                    if (shouldClimb)
                    {
                        _targetOverride = new Vector2(0, transform.position.y + 1000);
                        _climbing = true;
                    }
                    Walk(0, transform.position.y < _target.y ? 1 : -1);
                }
            }
            //HORIZONTAL
            if (!Collisions.hHit && Collisions.vHit)
            {
                _crawling = false;
                Walk(transform.position.x < _target.x ? 1 : -1, 0);
            }
            //VERTICAL
            if (Collisions.hHit && !Collisions.vHit)
            {
                _crawling = true;
                if (shouldClimb)
                {
                    _targetOverride = new Vector2(0, transform.position.y + 1000);
                    _climbing = true;
                }
                Walk(0, transform.position.y < _target.y ? 1 : -1);
            }
            //NONE
            if (!Collisions.hHit && !Collisions.vHit)
            {
                if (_crawling)
                {
                    Walk(transform.position.x < _target.x ? 1 : -1, 0);
                }
                _crawling = false;
                _climbing = false;
            }
        }
        if (_climbing == false)
        {
            _targetOverride = Vector2.zero;
            SetTarget(_target);
        }
    }


    protected override void UpdateGravity()
    {
        if (_crawling)
        {
            return;
        }
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

    public override void Walk(float directionX, float directionY = 0)
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
                    ExternalForce.x += directionX * (1 / acc) * _currentSpeed * Time.fixedDeltaTime;
                }
                else
                {
                    if (Mathf.Abs(Speed.x) < _currentSpeed)
                    {
                        Speed.x += directionX * (1 / acc) * _currentSpeed * Time.fixedDeltaTime;
                        Speed.x = Mathf.Min(Mathf.Abs(Speed.x), _currentSpeed * Mathf.Abs(directionX)) *
                            Mathf.Sign(Speed.x);
                    }
                }

            }
            else
            {
                Speed.x = _currentSpeed * directionX;
            }
            if (directionX == 0 || Mathf.Sign(directionX) != Mathf.Sign(Speed.x))
            {
                if (dec > 0)
                {
                    Speed.x = Mathf.MoveTowards(Speed.x, 0, (1 / dec) * _currentSpeed * Time.fixedDeltaTime);
                }
                else
                {
                    Speed.x = 0;
                }
            }
        }

        if (CanCrawl && Mathf.Abs(transform.position.y - _target.y) > 0.5f)
        {
            //YYY
            Speed.y = _currentSpeed * directionY;
        }
    }
}
