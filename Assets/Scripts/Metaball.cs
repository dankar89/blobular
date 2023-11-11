using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum MetaballColorType {
  Color1,
  Color2,
  Color3
}

public enum MetaballState {
  Normal,
  Obstacle,
  PowerUp
}

public enum MetaballStateChangeAction {
  None,
  RemoveObstacles,
}

public struct MetaballTimedStateChange {
  public float timer;
  public MetaballState newState;
  public MetaballStateChangeAction action;
}

public class Metaball : MonoBehaviour {
  public static float SCALE_MODIFIER = 0.42f;

  public int id;

  public Color color;
  public MetaballColorType colorType;
  // value will be between 1 and 100
  public int value;
  int _popThreshold;

  public bool isHighlighted = false;
  public bool isAbsorbing = false;

  Material material;

  public ParticleSystem absorbParticles, popParticles;

  public UnityAction<Metaball> onRelease;
  public UnityAction<Metaball> onHighlight;
  public UnityAction<Metaball> onClearHighlight;
  public UnityAction<Metaball, int> onAbsorbComplete;
  public UnityAction<Metaball, MetaballTimedStateChange> onStateChanged;
  public UnityAction<int> onPopped;
  public UnityAction onConnectedChanged;

  MetaballTimedStateChange _timedStateChange;

  Rigidbody2D rb;
  CircleCollider2D circleCollider, circleTrigger;

  Canvas _canvas;
  TextMeshProUGUI _textMesh;

  Coroutine _highlightCoroutine, _absorbCoroutine;

  Transform _parentWhenColliding, _parentWhenFalling;

  public MetaballState state;

  public Dictionary<int, Metaball> connectedMetaballs = new Dictionary<int, Metaball> ();

  bool _hasFirstContact = false;

  void Awake () {
    rb = GetComponent<Rigidbody2D> ();
    foreach (var collider in GetComponents<CircleCollider2D> ()) {
      if (collider.isTrigger) {
        circleTrigger = collider;
      } else {
        circleCollider = collider;
      }
    }
    material = GetComponent<Renderer> ().material;

    _canvas = GetComponentInChildren<Canvas> ();
    _textMesh = _canvas.GetComponentInChildren<TextMeshProUGUI> ();
  }

  public Vector2 Position {
    get {
      return rb.position;
    }
    set {
      rb.position = value;
    }
  }

  public Color ColorFromType (MetaballColorType colorType) {
    switch (colorType) {
      case MetaballColorType.Color1:
        return Color.red;
      case MetaballColorType.Color2:
        return Color.green;
      case MetaballColorType.Color3:
        return Color.blue;
      default:
        return Color.white;
    }
  }

  public void Init (Vector3 startPosition, MetaballColorType color, int value, int popThreshold, UnityAction<Metaball> onRelease) {
    this.id = GetInstanceID ();
    transform.position = startPosition;
    this.colorType = color;
    this.color = ColorFromType (color);
    this.name = $"Metaball_{colorType}_{color}_{id}";
    material.color = this.color;
    this.value = value;
    this.onRelease = onRelease;
    state = MetaballState.Normal;
    _popThreshold = popThreshold;

    _canvas.gameObject.SetActive (false);

    this.transform.localScale = SCALE_MODIFIER * Vector3.one * Mathf.Sqrt (value);
    gameObject.layer = MetaballManager.NORMAL_LAYER;
    circleCollider.enabled = true;
    circleTrigger.enabled = true;
    rb.gravityScale = 1;
    rb.isKinematic = false;
    _timedStateChange = new MetaballTimedStateChange { timer = 0, newState = MetaballState.Normal, action = MetaballStateChangeAction.None };
  }

  public void Release () {
    circleCollider.enabled = false;
    circleTrigger.enabled = false;
    isAbsorbing = false;
    onRelease (this);
    onRelease = null;
    onHighlight = null;
    onClearHighlight = null;
    onConnectedChanged = null;
    onAbsorbComplete = null;
    onStateChanged = null;
    onPopped = null;
    _highlightCoroutine = null;
    _absorbCoroutine = null;
    _parentWhenColliding = null;
    _parentWhenFalling = null;
    StopAllCoroutines ();

    ClearHighlight (true);
    isHighlighted = false;
    _hasFirstContact = false;

    connectedMetaballs.Clear ();
  }

  void SetState (MetaballState newState, bool updateConnected = false) {
    Debug.Log ($"Set state to {newState} updateConnected: {updateConnected}");
    switch (newState) {
      case MetaballState.Normal:
        gameObject.layer = MetaballManager.NORMAL_LAYER;
        break;
      case MetaballState.Obstacle:
        ClearHighlight ();
        gameObject.layer = MetaballManager.OBSTACLE_LAYER;
        Debug.Log ($"Setting {name} to gameObject.layer {LayerMask.LayerToName (gameObject.layer)} obstacle layer: {LayerMask.LayerToName (MetaballManager.OBSTACLE_LAYER)}");
        material.color = Color.white;
        // rb.isKinematic = true;
        // rb.velocity = Vector2.zero;
        // rb.gravityScale = 0;
        transform.SetParent (_parentWhenFalling);

        // Set all connected metaballs to obstacle as well
        if (updateConnected) {
          SoundManager.PlaySfx ("bad_thing", .5f);
          foreach (var metaball in connectedMetaballs.Values) {
            metaball.SetState (MetaballState.Obstacle);
          }
        }
        connectedMetaballs.Clear ();
        break;
      case MetaballState.PowerUp:
        // Just remove metaball on timeout?
        break;
    }

    state = newState;
    onStateChanged?.Invoke (this, _timedStateChange);
  }

  IEnumerator TimedStateChangeAsync () {
    float timer = _timedStateChange.timer;
    MetaballState newState = _timedStateChange.newState;
    MetaballStateChangeAction action = _timedStateChange.action;

    _canvas.gameObject.SetActive (true);
    _textMesh.enabled = true;
    _textMesh.text = $"{Mathf.RoundToInt (timer)}";

    yield return new WaitUntil (() => _hasFirstContact);

    // yield return new WaitForSeconds (timer);
    // Update the text with time left in seconds and count down to 0 before continuing
    float timeFlashMultiplier = 7.5f;
    int flashThreshold = 5;
    while (timer > 0 && state == MetaballState.Normal) {
      timer -= Time.deltaTime;
      _textMesh.text = $"{Mathf.RoundToInt (timer)}";

      if (timer <= flashThreshold) {
        // Flash text if close to 0
        _textMesh.enabled = Mathf.RoundToInt (timer * (timeFlashMultiplier + (flashThreshold - timer))) % 2 == 0;
      }

      yield return null;
    }

    if (state != newState) {
      _textMesh.enabled = true;
      // Show the time as 0 for a few frames before changing the state
      _textMesh.text = $"0";
      yield return new WaitForSeconds (0.25f);
      _canvas.gameObject.SetActive (false);

      // switch (action) {
      //   case MetaballStateChangeAction.RemoveObstacles:
      //     break;
      // }
      yield return new WaitForEndOfFrame ();
      SetState (newState, true);
    }
    Debug.Log ($"Timed state change complete for {name}. new Layer: {LayerMask.LayerToName (gameObject.layer)}");
  }

  public void SetTimedStateChange (MetaballTimedStateChange timedStateChange) {
    if (_timedStateChange.timer > 0) {
      Debug.LogError ($"Trying to set timed state change when one is already set for {name}");
      return;
    }

    if (timedStateChange.newState == state) {
      Debug.LogError ($"Trying to set timed state {state} change to the same state for {name}");
      return;
    }

    _timedStateChange = timedStateChange;
    StartCoroutine (TimedStateChangeAsync ());
  }

  void HandleConnectedChanged () {
    if (isHighlighted) {
      foreach (var metaball in connectedMetaballs.Values) {
        metaball.Highlight ();
      }
    }
  }

  public void AddConnected (Metaball metaball, bool addToBoth = true) {
    if (metaball == this) {
      Debug.LogError ($"Trying to add self as connected. id: {id}");
      return;
    }
    if (connectedMetaballs.ContainsKey (metaball.id)) {
      // TODO: Get here a lot?
      // Debug.LogError ($"Trying to add already connected metaball. id: {id}");
      return;
    };
    connectedMetaballs.Add (metaball.id, metaball);
    // if (addToBoth) {
    //   metaball.AddConnected (this, false);
    // }
    onConnectedChanged?.Invoke ();
    HandleConnectedChanged ();
  }

  public void RemoveConnected (Metaball metaball, bool removeFromBoth = true) {
    if (!connectedMetaballs.ContainsKey (metaball.id)) return;
    connectedMetaballs.Remove (metaball.id);
    if (removeFromBoth) {
      metaball.RemoveConnected (this, false);
    }
    metaball.ClearHighlight ();
    onConnectedChanged?.Invoke ();
    HandleConnectedChanged ();
  }

  public void Highlight (bool highlightConnected = false) {
    if (isHighlighted) return;
    if (state == MetaballState.Obstacle) return;
    if (_highlightCoroutine != null) StopCoroutine (_highlightCoroutine);
    _highlightCoroutine = StartCoroutine (HighlightAsync (highlightConnected));
  }

  IEnumerator HighlightAsync (bool highlightConnected = false) {
    if (isHighlighted) yield break;

    if (highlightConnected) {
      foreach (var metaball in connectedMetaballs.Values) {
        metaball.Highlight ();
      }
    }

    onHighlight?.Invoke (this);
    isHighlighted = true;
    gameObject.layer = MetaballManager.HIGHLIGHT_LAYER;
  }

  public void ClearHighlight (bool clearConnected = false) {
    if (_highlightCoroutine != null) StopCoroutine (_highlightCoroutine);

    onClearHighlight?.Invoke (this);
    isHighlighted = false;

    if (state != MetaballState.Obstacle) {
      gameObject.layer = MetaballManager.NORMAL_LAYER;
    }

    // Clear highlight on connected metaballs
    if (clearConnected) {
      foreach (var metaball in connectedMetaballs.Values) {
        metaball.ClearHighlight ();
      }
    }
  }

  IEnumerator AbsorbByAsync (Metaball absorbingMetaball, float speed) {
    circleCollider.enabled = false;
    rb.velocity = Vector2.zero;
    rb.gravityScale = 0;
    isAbsorbing = true;
    Vector3 targetScale = SCALE_MODIFIER * Mathf.Sqrt (1) * Vector3.one;
    // Move towards the absorbing metaball
    while (Vector2.Distance (Position, absorbingMetaball.Position) > 0.1f) {
      Position = Vector2.MoveTowards (Position, absorbingMetaball.Position, speed * Time.deltaTime);

      // Scale down this metaball
      transform.localScale = Vector3.MoveTowards (transform.localScale, targetScale, speed * Time.deltaTime);
      yield return null;
    }

    // instantiate absorb particles
    var particles = Instantiate (absorbParticles, transform.position, Quaternion.identity);
    particles.GetComponent<ParticleSystemRenderer> ().material.color = color;
    particles.Play ();

    Release ();
  }

  int AbsorbBy (Metaball absorbingMetaball, float speed) {
    int valueAbsorbed = value;
    StopAllCoroutines ();
    _canvas.gameObject.SetActive (false);
    StartCoroutine (AbsorbByAsync (absorbingMetaball, speed));
    return valueAbsorbed;
  }

  IEnumerator AbsorbConnectedAsync (List<Metaball> connected, float speed) {
    isAbsorbing = true;
    int amountAbsorbed = 0;
    foreach (var metaball in connected) {
      if (metaball.state == MetaballState.Obstacle) continue;

      Debug.Log ($"Absorbing {metaball.name}");
      amountAbsorbed += metaball.AbsorbBy (this, speed);
    }

    if (amountAbsorbed == 0) {
      // If we could not absorb anything for some reason, we are done
      onAbsorbComplete?.Invoke (this, amountAbsorbed);
      onAbsorbComplete = null;
      _absorbCoroutine = null;
      isAbsorbing = false;
      yield break;
    }

    int targetValue = value + amountAbsorbed;

    SoundManager.PlaySfx ("blip3", .5f);
    yield return null;

    // scale up this metaball
    Vector3 targetScale = SCALE_MODIFIER * Vector3.one * Mathf.Sqrt (targetValue);
    float currentValue = this.value;

    // Use a small value to determine "close enough"
    float epsilon = 0.01f;

    while (Vector3.Distance (transform.localScale, targetScale) > epsilon || Mathf.Abs (currentValue - targetValue) > epsilon) {
      Debug.Log ($"currentValue: {currentValue}, targetValue: {targetValue}");
      Debug.Log ($"currentScale: {transform.localScale}, targetScale: {targetScale}");
      transform.localScale = Vector3.MoveTowards (transform.localScale, targetScale, speed * Time.deltaTime);
      currentValue = Mathf.MoveTowards (currentValue, targetValue, speed * 5 * Time.deltaTime);
      this.value = Mathf.RoundToInt (currentValue); // Use RoundToInt to avoid casting issues
      yield return null;
    }

    // Update the value and scale to make sure they are correct
    this.value = targetValue;
    transform.localScale = targetScale;

    if (onAbsorbComplete == null) {
      Debug.LogError ($"onAbsorbComplete is null for {name}");
    }

    if (value >= _popThreshold) {
      Pop ();
    } else {
      onAbsorbComplete?.Invoke (this, amountAbsorbed);
      onAbsorbComplete = null;
      _absorbCoroutine = null;
    }

    isAbsorbing = false;
    yield return null;
  }

  public bool CanAbsorbConnected () {
    return connectedMetaballs.Count > 0 && _absorbCoroutine == null && isAbsorbing == false;
  }

  public void AbsorbConnected () {
    if (!CanAbsorbConnected ()) return;
    float absorbSpeed = 10f;
    if (!this.gameObject.activeInHierarchy) Debug.LogError ($"Trying to absorb inactive metaball {name}");
    StopAllCoroutines ();
    _canvas.gameObject.SetActive (false);
    _absorbCoroutine = StartCoroutine (AbsorbConnectedAsync (new List<Metaball> (connectedMetaballs.Values), absorbSpeed));
  }

  public Metaball GetLargestConnected () {
    Metaball largest = this;
    foreach (var metaball in connectedMetaballs.Values) {
      if (metaball.value > largest.value) {
        largest = metaball;
      }
    }
    return largest;
  }

  void Pop () {
    // instantiate absorb particles
    var particles = Instantiate (absorbParticles, transform.position, Quaternion.identity);
    particles.GetComponent<ParticleSystemRenderer> ().material.color = color;
    particles.Play ();
    onPopped?.Invoke (value);
    onPopped = null;
    SoundManager.PlaySfx ("harp", .5f, 1.5f);
    Release ();
  }

  public void SetParentOnCollision (Transform parentWhenColliding, Transform parentWhenFalling) {
    transform.SetParent (parentWhenFalling);
    _parentWhenColliding = parentWhenColliding;
    _parentWhenFalling = parentWhenFalling;
  }

  void OnTriggerEnter2D (Collider2D other) {
    if (!_hasFirstContact) {
      _hasFirstContact = true;
    }

    if (other.gameObject.CompareTag ("Metaball")) {
      // Don't do anything if this is an obstacle
      if (state == MetaballState.Obstacle) return;

      var otherMetaball = other.gameObject.GetComponent<Metaball> ();

      if (otherMetaball == this) return;

      if (otherMetaball && otherMetaball.state != MetaballState.Obstacle) {
        if (otherMetaball.color.Equals (color)) {
          AddConnected (otherMetaball);
        }
      }
    } else if (other.gameObject.CompareTag ("LevelBounds")) {
      if (_parentWhenColliding && transform.parent != _parentWhenColliding) {
        transform.SetParent (_parentWhenColliding);
      }
    }
  }

  void OnTriggerExit2D (Collider2D other) {
    if (other.gameObject.CompareTag ("Metaball")) {
      if (connectedMetaballs.Count == 0) return;
      var otherMetaball = other.gameObject.GetComponent<Metaball> ();
      if (otherMetaball && otherMetaball.state != MetaballState.Obstacle) {
        if (otherMetaball.color.Equals (color)) {
          RemoveConnected (otherMetaball);
        }
      }
    } else if (other.gameObject.CompareTag ("LevelBounds")) {
      if (transform.parent == _parentWhenColliding) {
        transform.SetParent (_parentWhenFalling);
      }
    }
  }
}