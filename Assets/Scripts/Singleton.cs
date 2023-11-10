using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton : MonoBehaviour {

    public static Singleton instance = null; //Static instance of GameManager which allows it to be accessed by any other script.

    protected virtual void Awake () {
        //Check if instance already exists
        if (instance == null)
            instance = this;

        //If instance already exists and it's not this:
        else if (instance != this) {
            Destroy (gameObject);
            return;
        }

        //Sets this to not be destroyed when reloading scene
        DontDestroyOnLoad (gameObject);

        OnAwake ();
    }

    protected virtual void OnAwake () {

    }

    public static T GetInstance<T> () {
        return (T) (object) instance;
    }
}