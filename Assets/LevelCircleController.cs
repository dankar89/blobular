using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCircleController : MonoBehaviour {
  CircleRenderer _circleRenderer;
  float _endAngle, _startAngle;

  LevelController _level;

  public void Init (LevelController levelController) {
    _circleRenderer = GetComponent<CircleRenderer> ();
    _startAngle = _circleRenderer.startAngle;
    _endAngle = _circleRenderer.endAngle;

    _level = levelController;
    levelController.OnScoreChanged += UpdateScore;
    levelController.OnLevelChanged += UpdateLevel;

    UpdateColor ();
    UpdateScore (0);
  }

  void UpdateColor () {
    _circleRenderer.SetColor (_level.currentPalette[2]);
  }

  void UpdateScore (int score) {
    float scorePercentage = _level.GetScorePercentageForLevel ();
    // Get the angle of the circle that corresponds to the score percentage
    float angle = Mathf.Lerp (_startAngle, _endAngle, scorePercentage);

    if (scorePercentage < 0.001f) {
      _circleRenderer.enabled = false;
    } else if (_circleRenderer.enabled == false) {
      _circleRenderer.enabled = true;
    }
    _circleRenderer.SetEndAngle (angle);
  }

  void UpdateLevel (int level) {
    UpdateScore (_level.score);
    UpdateColor ();
  }
}