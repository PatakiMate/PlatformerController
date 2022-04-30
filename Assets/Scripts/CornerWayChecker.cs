using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerWayChecker : MonoBehaviour
{
    public List<GameObject> Connections;
    public int checkSteps;
    public float checkDistance;
    public float size;

    [ContextMenu("CHECK")]
    public void CheckConnections()
    {
        foreach (GameObject corner in CornerChecker.Instance.Corners)
        {
            for (int x = 0; x < checkSteps; x++)
            {
                for (int y = 0; y < checkSteps; y++)
                {
                    Vector3 rayOrigin = new Vector2((transform.position.x - size) + x * checkDistance, (transform.position.y - size) + y * checkDistance);
                    RaycastHit2D ray = Physics2D.Raycast(rayOrigin, corner.transform.position - rayOrigin, Vector2.Distance(rayOrigin, corner.transform.position), CornerChecker.Instance.CollisionMask);
                    Debug.DrawRay(rayOrigin, (corner.transform.position - rayOrigin), Color.blue, 2);
                    if (ray)
                    {
                        Debug.Log("HIT: " + ray.collider.gameObject.name);
                        CornerWayChecker point = ray.collider.gameObject.GetComponent<CornerWayChecker>();
                        if (point != null)
                        {
                            if (!Connections.Contains(point.gameObject) && point != this)
                            {
                                Connections.Add(point.gameObject);
                            }
                        }
                    }
                }
            }
        }
    }
}
