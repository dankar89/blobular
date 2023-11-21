using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateTextPosition : MonoBehaviour {

  public float speed = 10f;
  public float radius = .15f;
  public float rotationSpeed = 0.1f;
  public float rotationAngle = 10f;

  Vector2 _ellipse = new Vector2 (.75f, 1);

  Vector3 _initialPosition;
  float _initialRotationZ;

  void Start () {
    _initialPosition = transform.position;
    _initialRotationZ = transform.localEulerAngles.z;
    StartCoroutine (AnimatePosition ());
    StartCoroutine (AnimateRotation ());
  }

  IEnumerator AnimatePosition () {
    while (true) {
      Vector3 randomPosition = Random.insideUnitCircle * _ellipse * radius;
      Vector3 targetPosition = transform.position == _initialPosition ? _initialPosition + randomPosition : _initialPosition;
      while (Vector3.Distance (transform.position, targetPosition) > 0.01f) {
        transform.position = Vector3.MoveTowards (transform.position, targetPosition, speed * Time.deltaTime);
        yield return null;
      }
      yield return null;
    }
  }

  IEnumerator AnimateRotation () {
    Quaternion initialRotation = transform.localRotation;

    while (true) {
      float randomRotation = Random.Range (-rotationAngle, rotationAngle);
      Quaternion targetRotation = Quaternion.Euler (0, 0, _initialRotationZ + randomRotation);

      while (Quaternion.Angle (transform.localRotation, targetRotation) > 0.01f) {
        transform.localRotation = Quaternion.RotateTowards (transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        yield return null;
      }
      yield return null;
    }
  }

}