using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevelController : MonoBehaviour {

  public static int NEXT_LEVEL_THRESHOLD = 140;
  public static int MAX_POP_THRESHOLD = 75;
  public static int START_POP_THRESHOLD = 40;
  public static int POP_THRESHOLD_INCREMENT = 5;
  public Metaball highlightedMetaball;
  public Transform metaballParent;

  public MeshRenderer quadRenderer;
  Material _quadMetaballMaterial;

  CircleRenderer _circleRenderer, _overflowCircleRenderer;

  public List<Texture2D> colorPalettes = new List<Texture2D> ();
  public Color[] currentPalette;
  public Color overflowColor = Color.red;
  public UnityAction<int> OnScoreChanged = delegate { };
  public UnityAction<int> OnLevelChanged = delegate { };

  public int score = 0;
  public int level = 1;

  public int startLife = 5;
  float life = 5;
  public float overflowLifePenaltyPerSecond = 0.5f;
  public float lifeRecoveredPerSecond = 0.5f;
  float _circleStartAngle, _circleEndAngle;

  public int popThreshold = START_POP_THRESHOLD;

  Vector2 movement;
  LevelUIController _levelUI;

  Coroutine _rotateCoroutine, _lifeTimerCoroutine;

  void Awake () {
    Debug.Log ("LevelController Awake");
    _quadMetaballMaterial = quadRenderer.material;
    UpdateColors ();

    life = startLife;

    _levelUI = GetComponentInChildren<LevelUIController> ();
    _levelUI.Init (this);

    _circleRenderer = transform.Find ("LevelCircle").GetComponent<CircleRenderer> ();
    _circleStartAngle = _circleRenderer.startAngle;
    _circleEndAngle = _circleRenderer.endAngle;
    _overflowCircleRenderer = _circleRenderer.transform.Find ("OverflowCircle").GetComponent<CircleRenderer> ();
    _overflowCircleRenderer.SetEndAngle (_circleStartAngle);
    _overflowCircleRenderer.Hide ();
    OverflowTrigger.OnOverflowChanged += OnOverflowChanged;
  }

  void GameOver () {
    Debug.Log ("Game Over");
    GameManager.GetInstance ().Pause ();
    _levelUI.ShowGameOver ();
    StopAllCoroutines();
    OverflowTrigger.OnOverflowChanged -= OnOverflowChanged;
  }

  IEnumerator LifeTimer (float fromAngle, float toAngle, float speed, int direction) {
    _overflowCircleRenderer.SetColor (overflowColor);
    _overflowCircleRenderer.Show ();
    while (enabled) {
      life = life + (speed * Time.deltaTime * direction);

      // Update the endAngle of the overflow circle to match the percentage of life lost
      float percentage = life > 0 ? 1f - (life / (float) startLife) : 0;
      Debug.Log ($"Overflow percentage: {percentage} life: {life}");

      float angle = Mathf.Lerp (fromAngle, toAngle, percentage);
      _overflowCircleRenderer.SetEndAngle (angle);

      if (direction < 0 && life < 0) {
        life = 0;
        break;
      }

      if (direction > 0 && life > startLife) {
        life = startLife;
        break;
      }
      yield return null;
    }

    if (life <= 0) {
      GameOver ();
    }
    _overflowCircleRenderer.Hide ();
    _lifeTimerCoroutine = null;
  }

  void OnOverflowChanged (bool overflow) {
    if (_lifeTimerCoroutine != null) {
      StopCoroutine (_lifeTimerCoroutine);
      _lifeTimerCoroutine = null;
      _overflowCircleRenderer.Hide ();
    }

    if (overflow) {
      _lifeTimerCoroutine = StartCoroutine (LifeTimer (_circleStartAngle, _circleEndAngle, overflowLifePenaltyPerSecond, -1));
    } else {
      if (life < startLife) {
        _lifeTimerCoroutine = StartCoroutine (LifeTimer (_circleStartAngle, _circleEndAngle, lifeRecoveredPerSecond, 1));
      }
    }
  }

  void OnAbsorbComplete (Metaball metaball, int amountAbsorbed) {
    highlightedMetaball?.ClearHighlight ();
    highlightedMetaball = null;
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
    if (popThreshold < MAX_POP_THRESHOLD) {
      popThreshold = Mathf.Min (popThreshold + POP_THRESHOLD_INCREMENT, MAX_POP_THRESHOLD);
    }

    MetaballManager.instance.ClearObstacles ();

    // Play level up sound
    SoundManager.PlaySfx ("level_up", .5f);

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

  public void OnPoint (InputAction.CallbackContext ctx) { }

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
  }

  void OnDestroy () {
    Debug.LogError ("LevelController OnDestroy. WHYYY???");
  }
}