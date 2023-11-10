using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour {

  public static int NEXT_LEVEL_THRESHOLD = 125;
  public Metaball highlightedMetaball;
  public Transform metaballParent;

  public MeshRenderer quadRenderer;
  Material _quadMetaballMaterial;

  public List<Texture2D> colorPalettes = new List<Texture2D> ();
  public Color[] currentPalette;
  public UnityAction<int> OnScoreChanged = delegate { };
  public UnityAction<int> OnLevelChanged = delegate { };

  public int score = 0;
  public int level = 1;
  public int life = 5;

  Vector2 movement;
  LevelUIController _levelUI;

  Coroutine _rotateCoroutine;

  void Awake () {
    _quadMetaballMaterial = quadRenderer.material;
    UpdateColors ();

    _levelUI = GetComponentInChildren<LevelUIController> ();
    _levelUI.Init (this);

    OverflowTrigger.OnOverflowChanged += (overflow) => {
      Debug.LogError ($"OVERFLOW: {overflow}");
    };
  }

  void OnAbsorbComplete (Metaball metaball, int amountAbsorbed) {
    highlightedMetaball?.ClearHighlight ();
    highlightedMetaball = null;
    // MetaballManager.ResumeSpawn ();
    Debug.Log ("Absorbed " + amountAbsorbed);
    UpdateScore (amountAbsorbed);
  }

  void UpdateColors () {
    Texture2D palette;
    if (level > colorPalettes.Count) {
      // Get random palette
      palette = colorPalettes[Random.Range (0, colorPalettes.Count)];
    } else {
      // Get the color palette for the current level
      palette = colorPalettes[level - 1];
    }

    var pixels = palette.GetPixels ();
    currentPalette = pixels;

    for (int i = 0; i < 3; i++) {
      // Set colors in material
      _quadMetaballMaterial.SetColor ("_Color" + (i + 1), pixels[i]);
    }
  }

  void NextLevel () {
    level++;
    UpdateColors ();
    OnLevelChanged (level);
  }

  int GetLevelThreshold (int level) {
    return Mathf.Clamp (level - 1, 0, int.MaxValue) * NEXT_LEVEL_THRESHOLD;
  }

  public int GetNextLevelThreshold () {
    return level * NEXT_LEVEL_THRESHOLD;
  }

  public float GetScorePercentageForLevel () {
    int levelThreshold = GetLevelThreshold (level);
    int nextLevelThreshold = GetNextLevelThreshold ();
    int scoreTowardsNextLevel = score - levelThreshold;
    return (float) scoreTowardsNextLevel / (nextLevelThreshold - levelThreshold);
  }

  void UpdateScore (int amount) {
    score += amount;

    if (score >= GetNextLevelThreshold ()) {
      NextLevel ();
    }

    OnScoreChanged (score);
  }

  void OnPopped (int value) {
    highlightedMetaball?.ClearHighlight ();
    highlightedMetaball = null;
    UpdateScore (value);
  }

  public void OnClick (InputAction.CallbackContext ctx) {
    if (!highlightedMetaball) return;

    Metaball largestConnected = highlightedMetaball.GetLargestConnected ();
    if (largestConnected.CanAbsorbConnected ()) {
      largestConnected.onAbsorbComplete -= OnAbsorbComplete;
      largestConnected.onAbsorbComplete += OnAbsorbComplete;
      largestConnected.onPopped -= OnPopped;
      largestConnected.onPopped += OnPopped;
      largestConnected.AbsorbConnected ();
    }
  }

  public void OnPoint (InputAction.CallbackContext ctx) {
    // RaycastForMetaball ();
  }

  IEnumerator RotateAsync () {
    float speed = 0;
    float acceleration = 200f;
    float maxSpeed = 85f;
    float rotationTargetZ = metaballParent.localEulerAngles.z;

    while (true) {
      if (movement.x != 0) {
        speed = Mathf.MoveTowards (speed, maxSpeed, acceleration * Time.deltaTime);
      } else {
        speed = Mathf.MoveTowards (speed, 0, acceleration * Time.deltaTime);
        if (speed == 0) {
          break;
        }
      }

      rotationTargetZ += speed * movement.x * Time.deltaTime;

      metaballParent.rotation = Quaternion.Euler (0, 0, rotationTargetZ);
      yield return null;
    }

    _rotateCoroutine = null;
  }

  public void OnMove (InputAction.CallbackContext ctx) {
    if (ctx.started || ctx.performed) {
      movement = ctx.ReadValue<Vector2> ();
    } else if (ctx.canceled) {
      movement = Vector2.zero;
    }

    if (_rotateCoroutine == null) {
      _rotateCoroutine = StartCoroutine (RotateAsync ());
    }
  }

  void RaycastForMetaball () {
    Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
    RaycastHit2D hit = Physics2D.Raycast (ray.origin, ray.direction);
    if (hit.collider != null) {
      Metaball metaball = hit.collider.GetComponent<Metaball> ();
      if (metaball != null && metaball.state != MetaballState.Obstacle) {
        if (highlightedMetaball == metaball && highlightedMetaball.isHighlighted) return;

        highlightedMetaball?.ClearHighlight (true);
        highlightedMetaball = metaball;
        highlightedMetaball.Highlight (true);

        return;
      }
    }

    if (highlightedMetaball != null) {
      highlightedMetaball.ClearHighlight (true);
      highlightedMetaball = null;
    }
  }

  void Update () {
    if (Time.frameCount % 4 == 0) {
      RaycastForMetaball ();
    }

    // use the normalized movement direction to rotate the metaball parent 
    // if (movement != Vector2.zero) {
    //   // Rotate around the z axis
    //   // Accelerate until max speed

    //   float z = metaballParent.localEulerAngles.z + (movement.x * Time.deltaTime * 100f);
    //   metaballParent.rotation = Quaternion.Euler (0, 0, z);
    // }
  }
}