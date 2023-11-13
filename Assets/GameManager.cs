using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton {

  protected override void OnAwake () {
    base.OnAwake ();
  }

  void Start () {
    SoundManager.PlaySong ("Song1", .5f);
  }

  public static GameManager GetInstance () {
    return GetInstance<GameManager> ();
  }

  // public void LoadStart () {
  //     LoadScene ("Start");
  // }

  public void LoadGame () {
    Resume ();
    LoadScene ("SampleScene");
  }

  public void LoadStart() {
    Resume();
    LoadScene ("Start");
  }

  // void LoadLevel () {
  //     Time.timeScale = 1;
  //     StartCoroutine (LoadLevelAsync (levelName, levelRound));
  // }

  // public void RestartLevel () {
  //     LoadLevel ("Game");
  // }

  public void Quit () => Application.Quit ();

  // void SetupCurrentLevel (int level = 1) {
  //     if (currentPlayerStats == null) {
  //         currentPlayerStats = new PlayerStatsModel();
  //     }

  //     currentLevel = GameObject.Find ("Level")?.GetComponent<LevelController> ();
  //     if (!currentLevel) {
  //         Debug.LogWarning ("No level found!");
  //         return;
  //     }

  //     currentLevel.Init (level, currentPlayerStats);
  // }

  // IEnumerator LoadLevelAsync (string levelName, int level) {
  //     Debug.Log ("Loading " + levelName);
  //     AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (levelName);

  //     // Wait until the asynchronous scene fully loads
  //     while (!asyncLoad.isDone) {
  //         yield return null;
  //     }

  //     SetupCurrentLevel (level);
  // }

  public void LoadScene (string name) {
    StartCoroutine (LoadSceneAsync (name));
  }

  public void Pause () {
    Time.timeScale = 0;
  }

  public void Resume () {
    Time.timeScale = 1;
  }

  public IEnumerator LoadSceneAsync (string name) {
    AsyncOperation asyncLoad = SceneManager.LoadSceneAsync (name);

    // Wait until the asynchronous scene fully loads
    while (!asyncLoad.isDone) {
      yield return null;
    }
  }
}