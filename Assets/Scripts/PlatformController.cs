using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class PlatformController : MonoBehaviour
{
    public PlatformWaypoint CurrentWaypoint;
    public float MaxSpeed;
    public float AccelerationDistance;
    public float DecelerationDistance;
    public float WaitTime;

    private Vector2 _speed = Vector2.zero;
    private float _currentWaitTime = 0;
    private List<ObjectController> _objs = new List<ObjectController>();
    private PhysicsConfig _pConfig;

    void Start()
    {
        _pConfig = GameObject.FindObjectOfType<PhysicsConfig>();
    }

    void FixedUpdate()
    {
        if (CurrentWaypoint)
        {
            #region Movement
            //Wait
            if (_currentWaitTime > 0)
            {
                _currentWaitTime -= Time.fixedDeltaTime;
                return;
            }
            //Decelerate
            Vector2 distance = CurrentWaypoint.transform.position - transform.position;
            if (distance.magnitude <= DecelerationDistance)
            {
                if (distance.magnitude > 0)
                {
                    _speed -= Time.fixedDeltaTime * distance.normalized * MaxSpeed * MaxSpeed / (2 * DecelerationDistance);
                }
                else
                {
                    _speed = Vector2.zero;
                }
            }
            //Accelerate
            else if (_speed.magnitude < MaxSpeed)
            {
                if (AccelerationDistance > 0)
                {
                    _speed += Time.fixedDeltaTime * distance.normalized * MaxSpeed * MaxSpeed /
                        (2 * AccelerationDistance);
                }
                if (_speed.magnitude > MaxSpeed || AccelerationDistance <= 0)
                {
                    _speed = distance.normalized * MaxSpeed;
                }
            }
            //Move
            Vector3 newPos = Vector2.MoveTowards(transform.position, CurrentWaypoint.transform.position, _speed.magnitude * Time.fixedDeltaTime);
            Vector2 velocity = newPos - transform.position;
            //Move objects
            if (_speed.y > 0)
            {
                MoveObjects(velocity);
                transform.position = newPos;
            }
            else
            {
                transform.position = newPos;
                MoveObjects(velocity);
            }
            //Reach target
            distance = CurrentWaypoint.transform.position - transform.position;
            if (distance.magnitude < 0.00001f)
            {
                _speed = Vector2.zero;
                CurrentWaypoint = CurrentWaypoint.NextWaypoint;
                _currentWaitTime = WaitTime;
            }
            #endregion
        }
    }

    #region Manage Attached Objects
    private void AttachObject(Collider2D other)
    {
        ObjectController obj = other.GetComponent<ObjectController>();
        if (obj && !_objs.Contains(obj))
        {
            if (_pConfig.OneWayPlatformMask == (_pConfig.OneWayPlatformMask | (1 << gameObject.layer)) && (obj.transform.position.y < transform.position.y || obj.TotalSpeed.y > 0))
            {
                return;
            }
            else
            {
                _objs.Add(obj);
            }
        }
    }

    private void MoveObjects(Vector2 velocity)
    {
        foreach (ObjectController obj in _objs)
        {
            obj.Move(velocity);
        }
    }
    #endregion

    #region Collisions
    void OnTriggerEnter2D(Collider2D other)
    {
        AttachObject(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        AttachObject(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        ObjectController obj = other.GetComponent<ObjectController>();
        if (obj && _objs.Contains(obj))
        {
            _objs.Remove(obj);
            obj.ApplyForce(_speed);
        }
    }
    #endregion
}