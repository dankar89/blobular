using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnData {
  public float duration;
  public float spawnInterval;
  public int metaballsPerSpawn;
  public int minSpawnValue;
  public int maxSpawnValue;

  public float timedObstacleChance;
  public float powerUpChance;
}