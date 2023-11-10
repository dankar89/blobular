using System.Collections;
using UnityEngine;

[RequireComponent (typeof (LineRenderer))]
public class CircleRenderer : MonoBehaviour {
  public int segments = 50;
  public float radius = 5f;
  public float startAngle = 0f; // In degrees
  public float endAngle = 360f; // In degrees
  LineRenderer _lineRenderer;

  Material _material;

  public bool reRender = false;

  Coroutine _animateEndAngleCoroutine;

  void Awake () {
    _lineRenderer = GetComponent<LineRenderer> ();
    _material = GetComponent<Renderer> ().material;
    RenderCircle ();
  }

  void OnEnable () {
    _lineRenderer.enabled = true;
  }

  void OnDisable () {
    _lineRenderer.enabled = false;
  }

  public void SetColor (Color color) {
    _material.color = color;
  }

  void RenderCircle () {
    _lineRenderer.positionCount = segments + 1;
    _lineRenderer.useWorldSpace = false;

    float deltaAngle = (endAngle - startAngle) / segments;
    float angle = startAngle;

    for (int i = 0; i <= segments; i++) {
      float x = -Mathf.Cos (angle * Mathf.Deg2Rad) * radius;
      float y = Mathf.Sin (angle * Mathf.Deg2Rad) * radius;
      _lineRenderer.SetPosition (i, new Vector3 (x, y, 0));

      angle += deltaAngle;
    }
  }

  IEnumerator AnimateEndAngle (float targetAngle, float duration) {
    float startAngle = this.endAngle;
    float time = 0;

    while (time < duration) {
      this.endAngle = Mathf.Lerp (startAngle, targetAngle, time / duration);
      RenderCircle ();
      time += Time.deltaTime;
      yield return null;
    }

    this.endAngle = targetAngle;
    RenderCircle ();
  }

  public void SetEndAngle (float angle, float duration = 0f) {
    if (_animateEndAngleCoroutine != null) {
      StopCoroutine (_animateEndAngleCoroutine);
    }
    if (duration > 0) {
      _animateEndAngleCoroutine = StartCoroutine (AnimateEndAngle (angle, duration));
    } else {
      endAngle = angle;
      RenderCircle ();
    }
  }

  void Update () {
    if (_lineRenderer == null) return;
    if (reRender) {
      RenderCircle ();
      reRender = false;
    }
  }
}