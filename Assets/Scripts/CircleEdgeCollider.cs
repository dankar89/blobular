using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleEdgeCollider : MonoBehaviour {
  PolygonCollider2D _polygonCollider;
  PolygonCollider2D PolygonCollider {
    get {
      if (_polygonCollider == null) {
        _polygonCollider = GetComponent<PolygonCollider2D> ();
      }

      if (_polygonCollider == null) {
        _polygonCollider = gameObject.AddComponent<PolygonCollider2D> ();
      }

      return _polygonCollider;
    }
  }

  public int pointCount = 40; // Number of points on the circle
  public float innerRadius = 5f; // Inner radius of the boundary
  public float outerRadius = 6f; // Outer radius of the boundary
  public float gapAngle = 15f;
  public float angleOffset = 45f;

  void Start () {
    GenerateBoundary ();
  }

  void GenerateBoundary () {
    List<Vector2> vertices = new List<Vector2> ();

    float gapAngleRad = gapAngle * Mathf.Deg2Rad;

    float topAngle = Mathf.PI / 2 + (angleOffset * Mathf.Deg2Rad);
    float halfGapAngleRad = gapAngleRad / 2;

    float angleIncrement = (2 * Mathf.PI - gapAngleRad) / pointCount;

    // Generate vertices for outer radius
    for (int i = 0; i < pointCount; i++) {
      float angle = topAngle + i * angleIncrement;
      float x = Mathf.Cos (angle) * outerRadius;
      float y = Mathf.Sin (angle) * outerRadius;
      vertices.Add (new Vector2 (x, y));
    }

    // Generate vertices for inner radius in reverse order
    for (int i = pointCount - 1; i >= 0; i--) {
      float angle = topAngle + i * angleIncrement;
      float x = Mathf.Cos (angle) * innerRadius;
      float y = Mathf.Sin (angle) * innerRadius;
      vertices.Add (new Vector2 (x, y));
    }

    // Close the gap by connecting the last vertex of the inner circle to the first vertex of the outer circle
    vertices.Add (vertices[0]);

    PolygonCollider.SetPath (0, vertices.ToArray ()); // Set the vertices on the PolygonCollider2D
  }

}