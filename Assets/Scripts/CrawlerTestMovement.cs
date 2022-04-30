using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CrawlerTestMovement : MonoBehaviour
{
    public float Reached;
    public float Distance;
    public Transform target;
    private Vector2 partTarget;
    public GameObject LastPoint;
    public float speed;
    public bool goingVertical;
    public bool negative;
    public List<GameObject> corners = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        partTarget = GetNextPoint();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, partTarget, speed * Time.deltaTime);
        Distance = Vector2.Distance(transform.position, partTarget);
        if (Vector2.Distance(transform.position, partTarget) < Reached)
        {
            goingVertical = !goingVertical;
            partTarget = GetNextPoint();
        }

        if (Mathf.Abs(target.position.x - transform.position.x) > 1)
        {
            bool neg = target.position.x < transform.position.x ? true : false;
            if (neg != negative)
            {
                partTarget = GetNextPoint();
                negative = neg;
            }
        }
    }
    public Vector2 GetNextPoint()
    {
        corners.Clear();
        foreach (GameObject corner in CornerChecker.Instance.Corners)
        {
            if (goingVertical)
            {
                if (corner.transform.position.x == LastPoint.transform.position.x)
                {
                    corners.Add(corner);
                }
            }
            else
            {
                if (corner.transform.position.y == LastPoint.transform.position.y)
                {
                    corners.Add(corner);
                }
            }
        }
        if (goingVertical == false)
        {
            List<GameObject> removables = new List<GameObject>();
            foreach (GameObject corner in corners)
            {
                if (target.position.x < transform.position.x && corner.transform.position.x > transform.position.x)
                {
                    removables.Add(corner);
                }
                if (target.position.x > transform.position.x && corner.transform.position.x < transform.position.x)
                {
                    removables.Add(corner);
                }
            }
            if (removables.Count < corners.Count)
            {
                foreach (GameObject removable in removables)
                {
                    corners.Remove(removable);
                }
            }
        }
        if(corners.Count > 1)
        {
            if(corners.Contains(LastPoint.gameObject))
            {
                corners.Remove(LastPoint.gameObject);
            }
        }
        corners = corners.OrderBy(element => Vector2.Distance(transform.position, element.transform.position)).ToList();

        LastPoint = corners[0];
        return corners[0].transform.position;
    }
}
