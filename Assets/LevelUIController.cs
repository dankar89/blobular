using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelUIController : MonoBehaviour {

  TextMeshProUGUI _scoreText;
  int targetScore = 0;
  float currentScore = 0;

  public float scoreUpdateSpeed = 10f;

  Coroutine _updateScoreCoroutine;

  public LevelCircleController levelProgress;

  // Start is called before the first frame update
  void Awake () {

  }

  public void Init (LevelController levelController) {
    Debug.Log ("Init LevelUIController");
    levelProgress.Init (levelController);
    _scoreText = transform.Find ("ScoreText").GetComponent<TextMeshProUGUI> ();
    _scoreText.text = "0";

    levelController.OnScoreChanged += UpdateScore;
  }

  IEnumerator UpdateScoreAsync () {
    float speed = Mathf.Abs (targetScore - currentScore) * Time.deltaTime * scoreUpdateSpeed;
    // Move current score towards target. The further away the target is, the faster we move.
    while (currentScore < targetScore) {
      currentScore = Mathf.MoveTowards (currentScore, targetScore, speed);
      _scoreText.text = Mathf.RoundToInt (currentScore).ToString ();
      yield return null;
    }

    _scoreText.text = targetScore.ToString ();

    _updateScoreCoroutine = null;
    yield return null;
  }

  public void UpdateScore (int newScore) {
    targetScore = newScore;
    if (_updateScoreCoroutine != null) {
      StopCoroutine (_updateScoreCoroutine);
    }

    _updateScoreCoroutine = StartCoroutine (UpdateScoreAsync ());
  }
}