using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_MoveSpeedDisplay : MonoBehaviour {
	public TextMeshProUGUI displayText;
	public CanvasGroup ourCanvas;
	float targetAlpha = 0;
	float setTime = 0, displaytime = 2f;
	// Use this for initialization
	void Start () {
		ourCanvas.alpha = 0f;
	}
	
	public void setDisplayText(string toThis)
    {
		setTime = Time.unscaledTime + displaytime*2f;
		displayText.text = toThis;
    }

	void Update()
    {
		ourCanvas.alpha = Mathf.Clamp01((setTime - Time.unscaledTime) / displaytime);
    }
}
