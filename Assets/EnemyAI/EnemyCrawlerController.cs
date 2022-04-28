using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCrawlerController : EnemyCharacterController
{
    private Vector2 _partTarget;
    public float ReachDistance;

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

    public Vector2 GetClosestCorner()
    {
        Vector2 closest = new Vector2(1000, 1000);
        List<GameObject> corners = new List<GameObject>();
        foreach (GameObject corner in CornerChecker.Instance.Corners)
        {
            corners.Add(corner);
        }

        corners = corners.OrderBy(x => Vector2.Distance(this.transform.position, x.transform.position)).ToList();
        if (Vector2.Distance(corners[0].transform.position, Player.transform.position) < Vector2.Distance(corners[1].transform.position, Player.transform.position))
        {
            closest = corners[0].transform.position;
        }
        else
        {
            closest = corners[1].transform.position;
        }
        return closest;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (_partTarget.y > transform.position.y)
        {
            Walk(0, transform.position.y < _target.y ? 1 : -1);
        } else
        {
            Walk(transform.position.x < _target.x ? 1 : -1, 0);
        }
        if(Vector2.Distance(transform.position, _partTarget) < ReachDistance)
        {
            _partTarget = GetClosestCorner();
        }
    }


    protected override void UpdateGravity()
    {
        return;
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

        if (CanCrawl && Mathf.Abs(transform.position.y - _target.y) > 0.5f)
        {
            //YYY
            Speed.y = CurrentSpeed * directionY;
        }
    }
    public override void SetTarget(Vector2 target)
    {
        _target = target;
        _partTarget = GetClosestCorner();
    }
}
