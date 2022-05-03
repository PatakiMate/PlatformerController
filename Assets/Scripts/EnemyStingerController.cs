using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyStingerController : MonoBehaviour
{
    public Transform Target;
    public float Speed;
    public float NextWaypointDistance;

    Path path;
    int currentWaypoint = 0;
    bool reachedEndOfPath = false;

    Seeker seeker;
    private void Start()
    {
        seeker = GetComponent<Seeker>();
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }
    void UpdatePath()
    {
        if (seeker.IsDone())
        {
            seeker.StartPath(transform.position, Target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error) {
            path = p;
            currentWaypoint = 0;
        }
    }
    private void Update()
    {
        if(path == null)
        {
            return;
        }
        if(currentWaypoint >= path.vectorPath.Count)
        {
            reachedEndOfPath = true;
            return;
        } else
        {
            reachedEndOfPath = false;
        }

        Vector2 direction = (path.vectorPath[currentWaypoint] - transform.position).normalized;
        Vector2 force = direction * Speed * Time.deltaTime;

        transform.Translate(force);

        float distance = Vector2.Distance(transform.position, path.vectorPath[currentWaypoint]);

        if(distance < NextWaypointDistance)
        {
            currentWaypoint++;
        }
    }
}
