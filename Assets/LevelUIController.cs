using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour {

  TextMeshProUGUI _scoreText, _levelText;
  int targetScore = 0;
  float currentScore = 0;

  public float scoreUpdateSpeed = 10f;

  Coroutine _updateScoreCoroutine;

  public LevelCircleController levelProgress;

  LevelController _level;

  public GameObject gameOverScreen;
  Button _restartButton, _backButton;

  // Start is called before the first frame update
  void Awake () {

  }

  public void Init (LevelController levelController) {
    Debug.Log ("Init LevelUIController");
    _level = levelController;
    levelProgress.Init (levelController);
    _scoreText = transform.Find ("ScoreText").GetComponent<TextMeshProUGUI> ();
    _scoreText.text = "0";

    _levelText = transform.Find ("LevelText").GetComponent<TextMeshProUGUI> ();
    UpdateLevel (1);

    levelController.OnScoreChanged += UpdateScore;
    levelController.OnLevelChanged += UpdateLevel;

    _restartButton = gameOverScreen.transform.Find ("Buttons/RestartButton").GetComponent<Button> ();
    _restartButton.onClick.AddListener (() => {
      GameManager.GetInstance ().LoadGame ();
    });

    _backButton = gameOverScreen.transform.Find ("Buttons/BackButton").GetComponent<Button> ();
    _backButton.onClick.AddListener (() => {
      GameManager.GetInstance ().LoadStart ();
    });
    gameOverScreen.SetActive (false);
  }

  public void ShowGameOver () {
    gameOverScreen.SetActive (true);
  }

  void UpdateLevel (int newLevel) {
    _levelText.text = "Level " + newLevel.ToString ();
    _levelText.color = _level.currentPalette[1];
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