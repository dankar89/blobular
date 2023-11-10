using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecycleMetaballTrigger : MonoBehaviour {
  void OnTriggerEnter2D (Collider2D other) {
    Metaball metaball = other.GetComponent<Metaball> ();
    if (metaball != null) {
      metaball.Release ();
    }
  }
}