using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelCircleController : MonoBehaviour {
  CircleRenderer _circleRenderer;
  CircleRenderer circleRenderer {
    get {
      if (_circleRenderer == null) {
        _circleRenderer = GetComponent<CircleRenderer> ();
      }
      return _circleRenderer;
    }
  }

  float _endAngle, _startAngle;

  LevelController _level;

  public void Init (LevelController levelController) {
    Debug.Log ("Init LevelCircleController");

    if (levelController == null) {
      Debug.LogError ("LevelController is null");
    }

    _startAngle = circleRenderer.startAngle;
    _endAngle = circleRenderer.endAngle;

    _level = levelController;
    levelController.OnScoreChanged += UpdateScore;
    levelController.OnLevelChanged += UpdateLevel;

    UpdateColor ();
    UpdateScore (0);
  }

  void UpdateColor () {
    circleRenderer.SetColor (_level.currentPalette[2]);
  }

  void UpdateScore (int score) {
    float scorePercentage = _level.GetScorePercentageForLevel ();
    // Get the angle of the circle that corresponds to the score percentage
    float angle = Mathf.Lerp (_startAngle, _endAngle, scorePercentage);

    if (scorePercentage < 0.001f) {
      circleRenderer.Hide ();
    } else if (!circleRenderer.IsVisible ()) {
      circleRenderer.Show ();
    }
    circleRenderer.SetEndAngle (angle, .5f);
  }

  void UpdateLevel (int level) {
    UpdateScore (_level.score);
    UpdateColor ();
  }
}