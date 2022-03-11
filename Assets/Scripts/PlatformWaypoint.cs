using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformWaypoint : MonoBehaviour {

    public PlatformWaypoint NextWaypoint;

    void OnDrawGizmosSelected() {
        if (NextWaypoint) {
            Gizmos.color = Color.cyan * new Color(1, 1, 1, 0.5f);
            Gizmos.DrawLine(transform.position, NextWaypoint.transform.position);
        }
        Gizmos.color = Color.blue * new Color(1, 1, 1, 0.5f);
        Gizmos.DrawWireSphere(transform.position, 0.25f);
    }
}