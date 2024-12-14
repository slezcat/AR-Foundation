using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;

public class ARPlaneBoundary : MonoBehaviour
{
   public ARPlane arPlane;
    private Vector3[] boundaryPoints;

    private void Start()
    {
        if (arPlane != null)
        {
            // Update boundary points when the plane boundary is updated
            arPlane.boundaryChanged += OnPlaneBoundaryChanged;
        }
    }

    private void OnPlaneBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
    {
        // Get the boundary points as Vector2
        NativeArray<Vector2> boundary = eventArgs.plane.boundary;

        // Convert to Vector3 array (y is the height of the ARPlane)
        boundaryPoints = new Vector3[boundary.Length];
        for (int i = 0; i < boundary.Length; i++)
        {
            boundaryPoints[i] = new Vector3(boundary[i].x, arPlane.transform.position.y, boundary[i].y);
        }
    }

    void Update()
    {
        if (arPlane != null && boundaryPoints != null && boundaryPoints.Length > 0)
        {
            // Check if object is within plane boundary
            if (!IsPointInPolygon(transform.position, boundaryPoints))
            {
                // Move the object back inside the boundary
                Vector3 closestPoint = GetClosestPointOnPolygon(transform.position, boundaryPoints);
                transform.position = closestPoint;
            }
        }
    }

    // Check if a point is inside the plane boundary polygon
    bool IsPointInPolygon(Vector3 point, Vector3[] polygon)
    {
        int n = polygon.Length;
        int j = n - 1;
        bool inside = false;

        for (int i = 0; i < n; j = i++)
        {
            if (((polygon[i].z > point.z) != (polygon[j].z > point.z)) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.z - polygon[i].z) / (polygon[j].z - polygon[i].z) + polygon[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    // Get the closest point on the polygon to keep the object inside
    Vector3 GetClosestPointOnPolygon(Vector3 point, Vector3[] polygon)
    {
        Vector3 closestPoint = polygon[0];
        float minDistance = Vector3.Distance(point, closestPoint);

        for (int i = 0; i < polygon.Length; i++)
        {
            Vector3 segmentStart = polygon[i];
            Vector3 segmentEnd = polygon[(i + 1) % polygon.Length];

            Vector3 closestOnSegment = ClosestPointOnLine(segmentStart, segmentEnd, point);
            float distance = Vector3.Distance(point, closestOnSegment);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = closestOnSegment;
            }
        }

        return closestPoint;
    }

    // Calculate the closest point on a line segment
    Vector3 ClosestPointOnLine(Vector3 start, Vector3 end, Vector3 point)
    {
        Vector3 lineDirection = end - start;
        float lineLengthSquared = lineDirection.sqrMagnitude;

        if (lineLengthSquared == 0f) return start; // start and end are the same

        float t = Vector3.Dot(point - start, lineDirection) / lineLengthSquared;
        t = Mathf.Clamp01(t); // Clamp t to the segment

        return start + t * lineDirection;
    }
}
