using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyCharacterController : ObjectController
{
    public Transform StartPoint;
    public Transform EndPoint;
    private float _currentSpeed;
    private Vector2 _target;
    public Transform Armature;
    [HideInInspector]
    public EnemyData EData;
    [HideInInspector]
    public Transform Player;
    public override void Start()
    {
        EData = GetComponent<EnemyData>();
        Player = PlayerObject.Instance.transform;
        base.Start();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        Walk(transform.position.x < _target.x ? 1 : -1);
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
    public void Walk(float direction)
    {
        if (Collisions.onSlope && Collisions.groundAngle > MaxSlopeAngle && Collisions.groundAngle < MinWallAngle)
        {
            direction = 0;
        }
        float acc = 0f;
        float dec = 0f;
        acc = EData.AccelerationTime;
        dec = EData.DecelerationTime;
        if (acc > 0)
        {
            if (ExternalForce.x != 0 && Mathf.Sign(ExternalForce.x) != Mathf.Sign(direction))
            {
                ExternalForce.x += direction * (1 / acc) * _currentSpeed * Time.fixedDeltaTime;
            }
            else
            {
                if (Mathf.Abs(Speed.x) < _currentSpeed)
                {
                    Speed.x += direction * (1 / acc) * _currentSpeed * Time.fixedDeltaTime;
                    Speed.x = Mathf.Min(Mathf.Abs(Speed.x), _currentSpeed * Mathf.Abs(direction)) *
                        Mathf.Sign(Speed.x);
                }
            }

        }
        else
        {
            Speed.x = _currentSpeed * direction;
        }
        if (direction == 0 || Mathf.Sign(direction) != Mathf.Sign(Speed.x))
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
    public void SetSpeed(float speed)
    {
        _currentSpeed = speed;
    }
    public void SetTarget(Vector2 target)
    {
        _target = target;
    }

    public Vector2 GetRandomTarget()
    {
        Vector2 output = new Vector2(Random.Range(StartPoint.position.x, EndPoint.position.x), transform.position.y);
        return output;
    }
}
