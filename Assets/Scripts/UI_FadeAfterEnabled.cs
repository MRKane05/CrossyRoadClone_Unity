using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_FadeAfterEnabled : MonoBehaviour {
	public CanvasGroup ourCanvasGroup;

	float fadeTime = 0;
	float fadeDuration = 0.5f;

	void OnEnable()
    {
		fadeTime = fadeDuration * 3f;
    }

	// Use this for initialization
	void Start () {
		ourCanvasGroup = GetComponent<CanvasGroup>();
	}
	
	// Update is called once per frame
	void Update () {
		fadeTime -= Time.unscaledDeltaTime;
		ourCanvasGroup.alpha = Mathf.Clamp01(fadeTime / fadeDuration);
		if (ourCanvasGroup.alpha < 0.03f)
        {
			gameObject.SetActive(false);
        }
	}
}
