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
    public bool negativeX;
    public bool negativeY;
    public List<GameObject> corners = new List<GameObject>();
    public float DirectionChangeDistance;
    public GameObject Point;
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
            //Debug.LogError("REACHED: " + LastPoint.gameObject);
            goingVertical = !goingVertical;
            partTarget = GetNextPoint();
        }

        if (Mathf.Abs(target.position.x - transform.position.x) > DirectionChangeDistance && Mathf.Abs(target.position.y - transform.position.y) < DirectionChangeDistance)
        {
            bool neg = target.position.x < transform.position.x ? true : false;
            if (neg != negativeX)
            {
                //Debug.Log("CHANGE DIRECTION X");
                partTarget = GetNextPoint(true);
                negativeX = neg;
            }
        }
        if (Mathf.Abs(target.position.y - transform.position.y) > DirectionChangeDistance && Mathf.Abs(target.position.x - transform.position.x) < DirectionChangeDistance)
        {
            bool neg = target.position.y < transform.position.y ? true : false;
            if (neg != negativeY)
            {
                //Debug.Log("CHANGE DIRECTION Y");
                partTarget = GetNextPoint(true);
                negativeY = neg;
            }
        }
    }
    public Vector2 GetNextPoint(bool change = false)
    {
        corners.Clear();
        bool addingVertical = false;
        //Debug.LogError("LAST POINT: " + LastPoint.gameObject.name);
        foreach (GameObject corner in CornerChecker.Instance.Corners)
        {
            if (goingVertical)
            {
                if (Mathf.Abs(corner.transform.position.x - LastPoint.transform.position.x) < 0.1f)
                {
                    corners.Add(corner);
                    //Debug.LogError("CLOSE: " + corner.name + ", " + corner.transform.position.x + "-" + LastPoint.transform.position.x);
                }
                //if (corner.transform.position.x == LastPoint.transform.position.x)
                //{
                //    Debug.LogError("ADDING: " + corner.name);
                //    corners.Add(corner);
                //}
            }
            else
            {
                if (Mathf.Abs(corner.transform.position.y - LastPoint.transform.position.y) < 0.1f)
                {
                    corners.Add(corner);
                }
                //    if (corner.transform.position.y == LastPoint.transform.position.y)
                //{
                //    Debug.LogError("ADDING: " + corner.name);
                //    corners.Add(corner);
                //}
            }
        }
        List<GameObject> removables = new List<GameObject>();
        //Debug.LogError("VERTICAL: " + goingVertical);


        foreach (GameObject corner in corners)
        {
            //Debug.Log("CHECKING RAY: " + corner.name);
            List<RaycastHit2D> rays = new List<RaycastHit2D>();
            bool hasRay = false;
            bool hasNoRay = false;
            for (int i = -1; i < 2; i++)
            {
                Vector3 endPoint = corner.transform.position;
                Vector3 originPoint = LastPoint.transform.position + (corner.transform.position - LastPoint.transform.position).normalized * 0.1f;

                if (Mathf.Abs(originPoint.x - LastPoint.transform.position.x) > Mathf.Abs(originPoint.y - LastPoint.transform.position.y))
                {
                    //Debug.Log("VERTICAL CHANGE");
                    originPoint.y += 0.2f * i;
                    endPoint.y += 0.2f * i;
                }
                else
                {
                    originPoint.x += 0.2f * i;
                    endPoint.x += 0.2f * i;
                    //Debug.Log("HORIZONTAL CHANGE");
                }

                RaycastHit2D ray = Physics2D.Raycast(originPoint, endPoint - originPoint, 0.3f, CornerChecker.Instance.CollisionMask);
                Debug.DrawRay(originPoint, (endPoint - originPoint).normalized * 0.3f, Color.blue, 0.2f);
                if (!ray)
                {
                    hasNoRay = true;
                    //Debug.Log("NO RAY: " + corner.name);
                }
                if (ray)
                {
                    //Instantiate(Point, ray.point, Quaternion.identity);
                    //Debug.Log("HAS RAY: " + corner.name);
                    hasRay = true;
                }
            }
            if (hasNoRay && hasRay)
            {
                //Debug.Log("SUCCESS: " + corner.name);
            }
            else
            {
                //Debug.LogError("ADD TO REMOVABLE: " + corner.name);
                removables.Add(corner);
            }
        }
        foreach (GameObject removable in removables)
        {
            //Debug.LogError("REMOVED: " + removable.name);
            corners.Remove(removable);
        }
        removables.Clear();

        if (corners.Contains(LastPoint.gameObject))
        {
            //Debug.LogError("REMOVE SAME");
            corners.Remove(LastPoint.gameObject);
        }
        else
        {
            //Debug.LogError("RETURN SAME");
        }
        //Debug.LogError("CORNER COUNT: " + corners.Count + " , REMOVEABLE COUNT: " + removables.Count);
        corners = corners.OrderByDescending(element => Vector2.Distance(LastPoint.transform.position, element.transform.position)).ToList();
        if (corners.Count > 2)
        {
            //Debug.Log("LAST: " + corners[corners.Count - 1] + " BEFORE LAST: " + corners[corners.Count - 2]);
        }
        if(!goingVertical)
        {
            //Debug.LogError("CHECKING DIRECTIONS");
            foreach (GameObject corner in corners)
            {
                if (target.position.x < transform.position.x && corner.transform.position.x > transform.position.x)
                {
                    if (!removables.Contains(corner))
                    {
                        //Debug.LogError("ADD TO REMOVABLE: " + corner.name);
                        removables.Add(corner);
                    }
                }
                if (target.position.x > transform.position.x && corner.transform.position.x < transform.position.x)
                {
                    if (!removables.Contains(corner))
                    {
                        //Debug.LogError("ADD TO REMOVABLE: " + corner.name);
                        removables.Add(corner);
                    }
                }
            }
        }

        //Debug.LogError("CORNER COUNT: " + corners.Count + " , REMOVEABLE COUNT: " + removables.Count);
        if (removables.Count == corners.Count)
        {
            //Debug.LogError("SAME COUNT");
            for (int i = 0; i < corners.Count - 1; i++)
            {
                GameObject removable = removables[i];
                //Debug.LogError("REMOVED: " + removable.name);
                corners.Remove(removable);
            }
        }
        else
        {
            foreach (GameObject removable in removables)
            {
                //Debug.LogError("REMOVED: " + removable.name);
                corners.Remove(removable);
            }
        }
        if (corners.Count > 0)
        {
            corners = corners.OrderBy(element => Vector2.Distance(transform.position, element.transform.position)).ToList();
            LastPoint = corners[0];
            return corners[0].transform.position;
        }  else
        {
            return LastPoint.transform.position;
        }
        //Debug.LogError("NEW POINT: " + corners[0].name);
       
    }
}
