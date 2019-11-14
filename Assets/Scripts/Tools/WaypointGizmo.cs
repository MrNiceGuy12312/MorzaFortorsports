using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointGizmo : MonoBehaviour
{
    public float size = 1f;
    public Color color = Color.yellow;
    private Transform[] waypoints;

    private void OnDrawGizmos()
    {
        waypoints = GetComponentsInChildren<Transform>();
        Vector3 lastWP = waypoints[waypoints.Length - 1].position;

        for (int i = 1; i < waypoints.Length; ++i)
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(waypoints[i].position, size);
            Gizmos.DrawLine(lastWP, waypoints[i].position);
            waypoints[i].gameObject.name = "Waypoint " + i;
            lastWP = waypoints[i].position;
        }
    }
}
