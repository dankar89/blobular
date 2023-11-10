using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class MetaballPoolManager : MonoBehaviour {

  public Metaball metaballPrefab;
  public Transform metaballParent;
  IObjectPool<Metaball> _pool;

  public List<Metaball> activeMetaballs = new List<Metaball> ();

  public IObjectPool<Metaball> Pool {
    get {
      if (_pool == null) {
        _pool = new ObjectPool<Metaball> (CreatePooledItem, OnGet, OnRelease, OnDestroyPoolItem, true, 100, 200);
      }
      return _pool;
    }
  }

  Metaball CreatePooledItem () {
    Metaball metaball = Instantiate (metaballPrefab, metaballParent);
    return metaball;
  }

  void OnRelease (Metaball metaball) {
    metaball.gameObject.SetActive (false);
    activeMetaballs.Remove (metaball);
  }

  void OnGet (Metaball metaball) {
    metaball.gameObject.SetActive (true);
    activeMetaballs.Add (metaball);
  }

  void OnDestroyPoolItem (Metaball metaball) {
    Destroy (metaball.gameObject);
  }

  void ReleaseMetaball (Metaball metaball) {
    Pool.Release (metaball);
  }

  public Metaball GetMetaball (Vector3 startPosition, MetaballColorType color, int value, bool setParentOnCollision = true) {
    Metaball metaball = Pool.Get ();
    metaball.SetParentOnCollision (metaballParent, transform);
    metaball.Init (startPosition, color, value, ReleaseMetaball);
    return metaball;
  }
}