using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using DG.Tweening;

public class EnemyStingerController : MonoBehaviour
{
    public Transform Target;
    public float Speed;
    public float NextWaypointDistance;

    private Path _path;
    private int _currentWaypoint = 0;
    private bool _reachedEndOfPath = false;

    private Seeker _seeker;

    private void Start()
    {
        _seeker = GetComponent<Seeker>();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.1f);
    }
    void UpdatePath()
    {
        if (_seeker.IsDone())
        {
            _seeker.StartPath(transform.position, Target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error) {
            _path = p;
            _currentWaypoint = 0;
        }
    }
    private void FixedUpdate()
    {
        if(_path == null)
        {
            return;
        }
        if(_currentWaypoint >= _path.vectorPath.Count)
        {
            _reachedEndOfPath = true;
            return;
        } else
        {
            _reachedEndOfPath = false;
        }
        Vector2 direction = (_path.vectorPath[_currentWaypoint] - transform.position).normalized;
        Vector2 force = direction * Speed * Time.deltaTime;
        transform.Translate(force);

        float distance = Vector2.Distance(transform.position, _path.vectorPath[_currentWaypoint]);

        if(distance < NextWaypointDistance)
        {
            _currentWaypoint++;
        }
    }
}
