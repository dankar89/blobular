using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OverflowTrigger : MonoBehaviour {
  Dictionary<int, Metaball> _overflowingMetaballs = new Dictionary<int, Metaball> ();
  Dictionary<int, Metaball> _metaballsMarkFofOverflow = new Dictionary<int, Metaball> ();

  public static UnityAction<bool> OnOverflowChanged = delegate { };
  public float overflowDelay = 3f;

  bool _isOverFlowing = false;

  Coroutine _overflowCoroutine;

  IEnumerator OverflowCoroutine () {
    yield return new WaitForSeconds (overflowDelay);

    // If there are any metaballs that are marked for overflow, add them to the overflowing metaballs
    foreach (var metaball in _metaballsMarkFofOverflow.Values) {
      _overflowingMetaballs.Add (metaball.GetInstanceID (), metaball);
    }

    // Clear the list of metaballs that are marked for overflow
    _metaballsMarkFofOverflow.Clear ();

    // Only call the event if the state has changed to overflowing
    bool newState = _overflowingMetaballs.Count > 0;
    if (newState && _isOverFlowing != newState) {
      _isOverFlowing = newState;
      OnOverflowChanged (_isOverFlowing);
    }

    Debug.Log ($"Has {_overflowingMetaballs.Count} overflowing metaballs");

    _overflowCoroutine = null;
  }

  void OnTriggerEnter2D (Collider2D other) {
    var metaball = other.GetComponent<Metaball> ();

    if (metaball && !_metaballsMarkFofOverflow.ContainsKey (metaball.GetInstanceID ())) {

      // Mark this metaball for overflow
      _metaballsMarkFofOverflow[metaball.GetInstanceID ()] = metaball;

      // If no metaballs are overflowing, start the overflow coroutine
      if (_overflowCoroutine == null && _metaballsMarkFofOverflow.Count > 0) {
        _overflowCoroutine = StartCoroutine (OverflowCoroutine ());
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

      // Stop the coroutine only if there are no metaballs left that could cause an overflow
      if (_overflowingMetaballs.Count == 0 && _metaballsMarkFofOverflow.Count == 0 && _overflowCoroutine != null) {
        StopCoroutine (_overflowCoroutine);
        _overflowCoroutine = null;

        if (_isOverFlowing) {
          _isOverFlowing = false;
          OnOverflowChanged (_isOverFlowing);
        }
      }
    }
  }
}