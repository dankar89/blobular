using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MetaballManager : MonoBehaviour {

  public static LayerMask NORMAL_LAYER, HIGHLIGHT_LAYER;

  MetaballPoolManager poolManager;

  public Transform[] metaballSpawnPoints;

  public SpawnDataScriptableObject spawnData;
  SpawnData currentSpawnData;
  int currentSpawnDataIndex = 0;

  static bool isPaused = false;

  List<Metaball> metaballObstacles = new List<Metaball> ();

  public LevelController level;

  public static MetaballManager instance;

  void Awake () {
    instance = this;
  }

  void Start () {
    poolManager = GetComponentInChildren<MetaballPoolManager> ();
    currentSpawnDataIndex = 0;
    currentSpawnData = spawnData.spawnDataList[currentSpawnDataIndex];

    NORMAL_LAYER = LayerMask.NameToLayer ("Metaballs");
    HIGHLIGHT_LAYER = LayerMask.NameToLayer ("MetaballsHighlight");

    StartSpawning (currentSpawnData);
  }

  void OnMetaballStateChanged (Metaball metaball, MetaballTimedStateChange timedStateChange) {
    if (metaball.state == MetaballState.Obstacle) {
      metaballObstacles.Add (metaball);
    }
  }

  void OnMetaballRelease (Metaball metaball) {
    if (metaball.state == MetaballState.Obstacle && metaballObstacles.Contains (metaball)) {
      metaballObstacles.Remove (metaball);
    }
  }

  Metaball SpawnMetaball () {
    int minSpawnValue = currentSpawnData.minSpawnValue;
    int maxSpawnValue = currentSpawnData.maxSpawnValue;
    int value = Random.Range (minSpawnValue, maxSpawnValue);
    bool centerSpawnPoint = false;
    // Sometimes spawn a double or triple value metaball
    float rnd = Random.value;
    if (rnd < spawnData.chanceOfDoubleValue) {
      value *= 2;
    } else if (rnd < spawnData.chanceOfTripleValue) {
      value *= 3;
    } else if (rnd < spawnData.chanceOf5xValue) {
      value *= 5;
      centerSpawnPoint = true;
    } else if (rnd < spawnData.chanceOf10xValue) {
      value *= 10;
      centerSpawnPoint = true;
    }

    Vector2 position = centerSpawnPoint ?
      metaballSpawnPoints[0].position :
      metaballSpawnPoints[Random.Range (0, metaballSpawnPoints.Length)].position;
    MetaballColorType colorType = (MetaballColorType) Random.Range (0, 3);

    Metaball metaball = poolManager.GetMetaball (position, colorType, value, level.popThreshold);
    metaball.onStateChanged += OnMetaballStateChanged;
    metaball.onRelease += OnMetaballRelease;

    if (currentSpawnData.timedObstacleChance > 0 && Random.value < currentSpawnData.timedObstacleChance) {
      int maxTimer = 15;
      // reduce the min timer by 1 every 5 levels
      int minTimer = Mathf.Max (7, maxTimer - (level.level / 5));
      int timer = Random.Range (minTimer, maxTimer);

      MetaballTimedStateChange stateChange = new MetaballTimedStateChange () {
        newState = MetaballState.Obstacle,
        timer = timer
      };
      metaball.SetTimedStateChange (stateChange);
    }
    return metaball;
  }

  IEnumerator SpawnMetaballsCoroutine () {
    // Duration in seconds
    float duration = currentSpawnData.duration;
    float spawnInterval = currentSpawnData.spawnInterval;
    int minSpawnValue = currentSpawnData.minSpawnValue;
    int maxSpawnValue = currentSpawnData.maxSpawnValue;

    bool isLastSpawnData = currentSpawnDataIndex == spawnData.spawnDataList.Length - 1;
    while (duration > 0 || isLastSpawnData) {
      yield return new WaitWhile (() => isPaused);

      for (int i = 0; i < currentSpawnData.metaballsPerSpawn; i++) {
        SpawnMetaball ();
        yield return new WaitForSeconds (.2f);
      }

      duration -= spawnInterval;
      yield return new WaitForSeconds (spawnInterval);
    }

    // Find the next spawn data if it exists
    if (!isLastSpawnData) {
      currentSpawnDataIndex++;
      currentSpawnData = spawnData.spawnDataList[currentSpawnDataIndex];
      StartSpawning (currentSpawnData);
    }
  }

  void StartSpawning (SpawnData spawnData) {
    StartCoroutine (SpawnMetaballsCoroutine ());
  }

  public static void PauseSpawn () {
    isPaused = true;
  }

  public static void ResumeSpawn () {
    isPaused = false;
  }

  public void ClearObstacles () {
    Debug.Log ($"Clearing obstacles: {metaballObstacles.Count}");
    foreach (var metaball in new List<Metaball> (metaballObstacles)) {
      metaball.Pop ();
    }
    metaballObstacles.Clear ();
  }

  private void OnDestroy () {
    StopAllCoroutines ();
  }
}