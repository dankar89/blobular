// scriptable object that holds the spawn data for the game

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (fileName = "SpawnData", menuName = "SpawnData")]
public class SpawnDataScriptableObject : ScriptableObject {

  public float chanceOfDoubleValue = 0.2f;
  public float chanceOfTripleValue = 0.05f;
  public float chanceOf5xValue = 0.02f;
  public float chanceOf10xValue = 0.01f;
  public float minSpawnInterval = .5f;
  public SpawnData[] spawnDataList;
}