using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Takes a sprite and creates an edge collider around it
public class SpriteToEdgeCollider : MonoBehaviour {
  // Start is called before the first frame update
  void Start () {
    GenerateEdgeCollider();
  }

  void GenerateEdgeCollider () {
    PolygonCollider2D polygonCollider = GetComponent<PolygonCollider2D> ();
    Vector2[] polygonVertices = polygonCollider.GetPath (0); // Assumes one path; adjust if your collider has multiple paths

    EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D> ();
    edgeCollider.points = polygonVertices;

    // Optional: Disable or remove the Polygon Collider if it's no longer needed
    polygonCollider.enabled = false;
    // Destroy(polygonCollider);
  }
}