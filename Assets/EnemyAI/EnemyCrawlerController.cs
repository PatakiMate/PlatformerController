using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyCrawlerController : EnemyCharacterController
{
    private Vector2 _partTarget;
    public float ReachDistance;
    public float Distance;
    public Transform CurrentTargetPoint;

    public Transform Closest;
    public Transform SecondClosest;
    private bool _following;
    private bool climb;
    public LayerMask WallLayer;
    public List<GameObject> corners;

    public override void Start()
    {
        EData = GetComponent<EnemyData>();
        Player = PlayerObject.Instance.transform;
        Animator = GetComponent<Animator>();
        base.Start();
        SetTarget(Player.transform.position);
        _partTarget = GetClosestCorner();
    }
    private void Update()
    {
        RunTimer();
    }

    public Vector2 GetClosestCorner()
    {
        Debug.LogError("UPDATE CORNERS");
        Vector2 closest = new Vector2(1000, 1000);
        corners.Clear();
        foreach (GameObject corner in CornerChecker.Instance.Corners)
        {
            corner.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            if (CurrentTargetPoint != null)
            {
                if(corner.transform.position.x == CurrentTargetPoint.position.x && corner.gameObject != CurrentTargetPoint.gameObject)
                {
                    corners.Add(corner);
                    continue;
                }
                Vector3 rayOrigin = new Vector2(transform.position.x, transform.position.y + 1);
                RaycastHit2D ray = Physics2D.Raycast(rayOrigin, corner.transform.position - rayOrigin, Vector2.Distance(rayOrigin, corner.transform.position) - 1, WallLayer);
                Debug.DrawRay(rayOrigin, (corner.transform.position - rayOrigin), Color.blue, 2);
                if (!ray)
                {
                    if (corner != CurrentTargetPoint.gameObject)
                    {
                        corner.gameObject.GetComponent<SpriteRenderer>().color = Color.green;
                        corners.Add(corner);
                    }
                }
                else
                {
                    Debug.Log("HIT: " + ray.collider.gameObject.name);
                }
            } else
            {
                Vector3 rayOrigin = new Vector2(transform.position.x, transform.position.y + 1);
                RaycastHit2D ray = Physics2D.Raycast(rayOrigin, corner.transform.position - rayOrigin, Vector2.Distance(rayOrigin, corner.transform.position) - 1, WallLayer);
                Debug.DrawRay(rayOrigin, (corner.transform.position - rayOrigin), Color.blue, 2);
                if (!ray)
                {
                    corners.Add(corner);
                } else
                {
                    Debug.Log("HIT: " + ray.collider.gameObject.name);
                }
            }
        }
        corners = corners.OrderBy(element => Mathf.Abs(element.transform.position.x - _target.x)).ToList();
        if(corners.Count > 0)
        {
            closest = corners[0].transform.position;
            CurrentTargetPoint = corners[0].transform;
            Closest = corners[0].transform;
            //SecondClosest = corners[1].transform;
            CurrentTargetPoint.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            return closest;
        }

        if (corners.Count < 2)
        {
            closest = corners[0].transform.position;
            CurrentTargetPoint = corners[0].transform;
            Closest = corners[0].transform;
        }
        else
        {
            //Debug.LogError("D1: " + Vector2.Distance(corners[0].transform.position, _target) + " D2: " + Vector2.Distance(corners[1].transform.position, _target));
            if(Mathf.Abs(corners[0].transform.position.x - _target.x) < Mathf.Abs(corners[1].transform.position.x - _target.x))
            //if (Vector2.Distance(corners[0].transform.position, _target) < Vector2.Distance(corners[1].transform.position, _target))
            {
                closest = corners[0].transform.position;
                CurrentTargetPoint = corners[0].transform;
                Closest = corners[0].transform;
                SecondClosest = corners[1].transform;
            }
            else
            {
                closest = corners[1].transform.position;
                CurrentTargetPoint = corners[1].transform;
                Closest = corners[1].transform;
                SecondClosest = corners[0].transform;
            }
        }
        CurrentTargetPoint.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
        return closest;
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        if (Mathf.Abs(_partTarget.y - transform.position.y) > Mathf.Abs(_partTarget.x - transform.position.x))
        {
            Debug.LogError("VERTICAL");
            Walk(0, transform.position.y < _partTarget.y ? 1 : -1);
        } else
        {
            Debug.LogError("HORIZONTAL");
            Walk(transform.position.x < _partTarget.x ? 1 : -1, 0);
        }
        Distance = Vector2.Distance(transform.position, _partTarget);
        if (Vector2.Distance(transform.position, _partTarget) < ReachDistance)
        {
            Debug.LogError("REACHED");
            _partTarget = GetClosestCorner();
        }
    }


    protected override void UpdateGravity()
    {
        return;
    }
    public override Vector2 Move(Vector2 deltaMove)
    {
        //int layer = gameObject.layer;
        //gameObject.layer = Physics2D.IgnoreRaycastLayer;
        //PreMove(ref deltaMove);
        //float xDir = Mathf.Sign(deltaMove.x);
        //if (deltaMove.x != 0)
        //{
        //    // Slope checks and processing
        //    if (deltaMove.y <= 0)
        //    {
        //        if (Collisions.onSlope)
        //        {
        //            if (Collisions.groundDirection == xDir)
        //            {
        //                DescendSlope(ref deltaMove);
        //            }
        //            else
        //            {
        //                ClimbSlope(ref deltaMove);
        //            }
        //        }
        //    }
        //    //HorizontalCollisions(ref deltaMove);
        //}
        //HorizontalCollisions(ref deltaMove);
        //if (Collisions.onSlope && Collisions.groundAngle >= MinWallAngle && Collisions.groundDirection != xDir && Speed.y < 0)
        //{
        //    Speed.x = 0;
        //    Vector2 origin = Collisions.groundDirection == -1 ? RayOrigins.bottomRight : RayOrigins.bottomRight;
        //    Collisions.hHit = Physics2D.Raycast(origin, Vector2.left * Collisions.groundDirection, 1f, CollisionMask);
        //}
        //if (deltaMove.y > 0 || (deltaMove.y < 0 && (!Collisions.onSlope || deltaMove.x == 0)))
        //{
        //    VerticalCollisions(ref deltaMove);
        //}
        //if (Collisions.onGround && deltaMove.x != 0 && Speed.y <= 0)
        //{
        //    HandleSlopeChange(ref deltaMove);
        //}
        //Debug.DrawRay(transform.position, deltaMove * 3f, Color.green);
        //transform.Translate(deltaMove);
        //// Checks for ground and ceiling, resets jumps if grounded
        //if (Collisions.vHit)
        //{
        //    if ((Collisions.below && TotalSpeed.y < 0) || (Collisions.above && TotalSpeed.y > 0))
        //    {
        //        if (!Collisions.onSlope || Collisions.groundAngle < MinWallAngle)
        //        {
        //            Speed.y = 0;
        //            ExternalForce.y = 0;
        //        }
        //    }
        //}
        //gameObject.layer = layer;
        return deltaMove;
    }

    public override void Walk(float directionX, float directionY = 0)
    {
        transform.Translate(new Vector2(directionX * CurrentSpeed * Time.fixedDeltaTime, directionY * CurrentSpeed * Time.fixedDeltaTime));
        //if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle && Collisions.groundAngle < MinWallAngle)
        //{
        //    directionX = 0;
        //}
        //float acc = 0f;
        //float dec = 0f;
        //acc = EData.AccelerationTime;
        //dec = EData.DecelerationTime;

        //float accY = 0f;
        //float decY = 0f;
        //accY = EData.AccelerationTime;
        //decY = EData.DecelerationTime;


        ////XXX

        //if (acc > 0)
        //{
        //    if (ExternalForce.x != 0 && Mathf.Sign(ExternalForce.x) != Mathf.Sign(directionX))
        //    {
        //        ExternalForce.x += directionX * (1 / acc) * CurrentSpeed * Time.fixedDeltaTime;
        //    }
        //    else
        //    {
        //        if (Mathf.Abs(Speed.x) < CurrentSpeed)
        //        {
        //            Speed.x += directionX * (1 / acc) * CurrentSpeed * Time.fixedDeltaTime;
        //            Speed.x = Mathf.Min(Mathf.Abs(Speed.x), CurrentSpeed * Mathf.Abs(directionX)) *
        //                Mathf.Sign(Speed.x);
        //        }
        //    }

        //}
        //else
        //{
        //    Speed.x = CurrentSpeed * directionX;
        //}
        //if (directionX == 0 || Mathf.Sign(directionX) != Mathf.Sign(Speed.x))
        //{
        //    if (dec > 0)
        //    {
        //        Speed.x = Mathf.MoveTowards(Speed.x, 0, (1 / dec) * CurrentSpeed * Time.fixedDeltaTime);
        //    }
        //    else
        //    {
        //        Speed.x = 0;
        //    }
        //}


        //Speed.y = CurrentSpeed * directionY;

    }
    public override void SetTarget(Vector2 target)
    {
        _target = target;
        if(_following != Following)
        {
            _following = Following;
            //_partTarget = GetClosestCorner();
        }
    }
}
