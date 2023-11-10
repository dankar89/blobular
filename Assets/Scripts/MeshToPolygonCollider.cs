using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshToPolygonCollider : MonoBehaviour {
  Mesh mesh;
  void Start () {
    PolygonCollider2D polygonCollider = gameObject.AddComponent<PolygonCollider2D> ();
    mesh = GetComponent<MeshFilter> ().mesh;
    if (mesh == null) {
      Debug.LogError ("Mesh not assigned to MeshCollider.");
      return;
    }

    Vector3[] meshVertices3D = mesh.vertices;
    int[] triangles = mesh.triangles;

    // Assuming the mesh is flat on the XZ plane, convert 3D vertices to 2D
    Vector2[] colliderVertices2D = new Vector2[meshVertices3D.Length];
    for (int i = 0; i < meshVertices3D.Length; i++) {
      Vector3 vertex3D = meshVertices3D[i];
      colliderVertices2D[i] = new Vector2 (vertex3D.x, vertex3D.z);
    }

    // Find the perimeter vertices by checking the triangles
    HashSet<int> perimeterVertexIndices = new HashSet<int> ();
    for (int i = 0; i < triangles.Length; i += 3) {
      perimeterVertexIndices.Add (triangles[i]);
      perimeterVertexIndices.Add (triangles[i + 1]);
      perimeterVertexIndices.Add (triangles[i + 2]);
    }

    // Create an array of perimeter vertices in 2D
    Vector2[] perimeterVertices2D = new Vector2[perimeterVertexIndices.Count];
    int index = 0;
    foreach (int vertexIndex in perimeterVertexIndices) {
      perimeterVertices2D[index] = colliderVertices2D[vertexIndex];
      index++;
    }

    // Set the path of the PolygonCollider2D
    polygonCollider.SetPath (0, perimeterVertices2D);
  }
}