using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class SoundManager : MonoBehaviour {

  [SerializeField]
  [System.Serializable]
  private struct SfxSource {
    public AudioSource source;
    public int index;
  }

  public AudioSource musicSource; //Drag a reference to the audio source which will play the music.

  public static SoundManager instance;

  public List<AudioClip> soundEffects;
  public List<AudioClip> songs;

  public float globalVolume = .5f;

  private List<SfxSource> sfxSources;

  private float initialVolume;

  void Awake () {
    if (instance) return;

    instance = this;

    Init ();
  }

  void Init () {
    Debug.Log ("Init SoundManager");
    initialVolume = AudioListener.volume;

    musicSource = GetComponent<AudioSource> ();
    musicSource.loop = true;
    sfxSources = new List<SfxSource> ();
  }

  private SfxSource GetFreeSfxSource () {
    SfxSource sfxSource = sfxSources.Where ((s) => !s.source.isPlaying).FirstOrDefault ();
    if (!sfxSource.source) {
      sfxSource = new SfxSource ();
      sfxSource.source = gameObject.AddComponent<AudioSource> ();
      sfxSource.index = sfxSources.Count;

      sfxSources.Add (sfxSource);
    }

    return sfxSource;
  }

  private SfxSource GetBusySfxSource (string clipName) {
    return sfxSources
      .Where (s => s.source.isPlaying && s.source.clip.name == clipName)
      .FirstOrDefault ();
  }

  public static int PlaySfx (string clipName, float volume = .5f, float pitch = 1, bool single = false, bool loop = false, float delay = 0) {
    if (string.IsNullOrEmpty (clipName)) {
      return -1;
    }

    if (single) {
      SfxSource sfx = instance.GetBusySfxSource (clipName);
      if (sfx.source) {
        return sfx.index;
      }
    }

    AudioClip clip = instance.soundEffects.Where ((ac) => ac.name == clipName).FirstOrDefault ();
    if (!clip) {
      Debug.LogError ($"Audio clip {clipName} not found!");
      return -1;
    }
    SfxSource sfxSource = instance.GetFreeSfxSource ();

    sfxSource.source.clip = clip;
    sfxSource.source.volume = volume;
    sfxSource.source.pitch = pitch;
    sfxSource.source.loop = loop;
    sfxSource.source.playOnAwake = false;
    if (delay > 0) {
      sfxSource.source.PlayDelayed (delay);
    } else {
      sfxSource.source.Play ();
    }

    return sfxSource.index;
  }

  public static void ChangeSfxPitch (int index, float newPitch) {
    SfxSource s = instance.sfxSources[index];
    if (s.source) {
      s.source.pitch = newPitch;
    }
  }

  public static void StopSfx (int index) {
    SfxSource s = instance.sfxSources[index];
    if (s.source && s.source.isPlaying) {
      s.source.Stop ();
    }
  }

  public static void PlaySong (string songName, float volume = 1) {
    if (instance.musicSource.isPlaying && instance.musicSource.clip?.name == songName) {
      return;
    }

    AudioClip clip = instance.songs.Where ((ac) => ac.name == songName).Single ();
    instance.musicSource.clip = clip;
    instance.musicSource.volume = volume;
    instance.musicSource.Play ();
  }

  public static void ToggleMute () {
    AudioListener.volume = AudioListener.volume == 0 ? instance.initialVolume : 0;
  }

  public static bool isMuted () {
    return AudioListener.volume == 0;
  }

}