using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OverflowTrigger : MonoBehaviour {
  Dictionary<int, Metaball> _overflowingMetaballs = new Dictionary<int, Metaball> ();
  Dictionary<int, Metaball> _metaballsMarkFofOverflow = new Dictionary<int, Metaball> ();

  public static UnityAction<bool> OnOverflowChanged = delegate { };
  public float overflowDelay = 2f;

  bool _isOverFlowing = false;

  // Coroutine that waits for a delay before checking if there are any
  IEnumerator OverflowCoroutine () {
    yield return new WaitForSeconds (overflowDelay);

    // If there are any metaballs that are marked for overflow, add them to the overflowing metaballs
    foreach (var metaball in _metaballsMarkFofOverflow.Values) {
      _overflowingMetaballs[metaball.GetInstanceID ()] = metaball;
    }

    // Clear the list of metaballs that are marked for overflow
    _metaballsMarkFofOverflow.Clear ();

    bool newState = _overflowingMetaballs.Count > 0;
    if (_isOverFlowing != newState) {
      _isOverFlowing = newState;
      OnOverflowChanged (_isOverFlowing);
    }
  }

  void OnTriggerEnter2D (Collider2D other) {
    var metaball = other.GetComponent<Metaball> ();

    if (metaball && !_metaballsMarkFofOverflow.ContainsKey (metaball.GetInstanceID ())) {
      bool startCoroutine = _metaballsMarkFofOverflow.Count == 0;

      // Mark this metaball for overflow
      _metaballsMarkFofOverflow[metaball.GetInstanceID ()] = metaball;

      // If no metaballs are overflowing, start the overflow coroutine
      if (startCoroutine) {
        StartCoroutine (OverflowCoroutine ());
      }
    }
  }

  void OnTriggerExit2D (Collider2D other) {
    var metaball = other.GetComponent<Metaball> ();
    if (metaball) {
      int id = metaball.GetInstanceID ();
      if (_metaballsMarkFofOverflow.ContainsKey (id)) {
        _metaballsMarkFofOverflow.Remove (id);
      } else if (_overflowingMetaballs.ContainsKey (id)) {
        _overflowingMetaballs.Remove (id);
      }

      if (_overflowingMetaballs.Count == 0) {
        StopAllCoroutines ();
      }
    }
  }
}