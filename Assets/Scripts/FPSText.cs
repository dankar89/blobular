using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour {
  private float deltaTime = 0.0f;
  TextMeshProUGUI fpsText;

  void Start () {
    fpsText = GetComponent<TextMeshProUGUI> ();
  }

  void Update () {
    // Calculate deltaTime to get the time it takes to render one frame
    deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;

    // Calculate the FPS
    float fps = 1.0f / deltaTime;

    // Format and set the text
    fpsText.text = string.Format ("{0:0.} fps", fps);
  }
}