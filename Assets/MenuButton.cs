using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour, ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler {

  public Material textMaterialSelected, textMaterialNormal;
  ParticleSystem _particles;
  TextMeshProUGUI _text;
  Button _button;
  bool _selected = false;

  void Start () {
    _button = GetComponent<Button> ();
    _particles = GetComponentInChildren<ParticleSystem> ();
    _particles.Stop ();
    _text = GetComponentInChildren<TextMeshProUGUI> ();
    _text.fontMaterial = textMaterialNormal;

    _selected = EventSystem.current.currentSelectedGameObject == gameObject;
    if (_selected) {
      ShowSelected ();
    } else {
      HideSelected ();
    }
  }

  void ShowSelected () {
    if (_selected) return;
    _selected = true;
    _particles.Play ();
    _text.fontMaterial = textMaterialSelected;
  }

  void HideSelected () {
    if (!_selected) return;
    _selected = false;
    _particles.Stop ();
    _text.fontMaterial = textMaterialNormal;
  }

  public void OnSelect (BaseEventData eventData) {
    ShowSelected ();
  }

  public void OnDeselect (BaseEventData eventData) {
    HideSelected ();
  }

  public void OnPointerEnter (PointerEventData eventData) {
    ShowSelected ();
  }

  public void OnPointerExit (PointerEventData eventData) {
    HideSelected ();
  }
}