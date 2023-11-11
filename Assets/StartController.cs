using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartController : MonoBehaviour {
  public Button playButton, quitButton;

  public Renderer quadRenderer;

  public List<Texture2D> colorPalettes = new List<Texture2D> ();

  void Awake () {
    playButton.onClick.AddListener (() => {
      GameManager.GetInstance ().LoadGame ();
    });

    quitButton.onClick.AddListener (() => {
      Application.Quit ();
    });

    UpdateColors ();
  }

  void UpdateColors () {
    Texture2D palette = colorPalettes[Random.Range (0, colorPalettes.Count)];

    var pixels = palette.GetPixels ();
    int colorCount = Mathf.Min (pixels.Length, 3);
    // Shuffle the pixels
    for (int i = 0; i < colorCount; i++) {
      Color temp = pixels[i];
      int randomIndex = Random.Range (i, colorCount);
      pixels[i] = pixels[randomIndex];
      pixels[randomIndex] = temp;
    }

    quadRenderer.material.SetColor ("_Color1", pixels[0]);
    quadRenderer.material.SetColor ("_Color2", pixels[1]);
    quadRenderer.material.SetColor ("_Color3", pixels[2]);
  }
}