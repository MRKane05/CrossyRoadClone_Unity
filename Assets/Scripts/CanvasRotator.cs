using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CanvasRotator : MonoBehaviour {
	public enum enScreenOrientation {NULL, LANDSCAPE, LEFT, RIGHT }
	public enScreenOrientation ScreenOrientation = enScreenOrientation.LANDSCAPE;
	enScreenOrientation _screenOrientation = enScreenOrientation.LANDSCAPE;
	public RectTransform parentRect;
	
	// Update is called once per frame
	void Update () {
		if (_screenOrientation != ScreenOrientation)
        {
			SetScreenOrientation(ScreenOrientation);
        }
	}

	public void SetScreenOrientation(enScreenOrientation newScreenOrientation)
    {
		RectTransform ourRect = gameObject.GetComponent<RectTransform>();
		switch (newScreenOrientation)
        {
			case enScreenOrientation.LANDSCAPE:
				transform.eulerAngles = Vector3.zero;
				ourRect.sizeDelta = new Vector2(960, 544);
				break;
			case enScreenOrientation.LEFT:
				transform.eulerAngles = Vector3.forward * 270f;
				ourRect.sizeDelta = new Vector2(544, 960);
				break;
			case enScreenOrientation.RIGHT:
				transform.eulerAngles = Vector3.forward * 90f;
				ourRect.sizeDelta = new Vector2(544, 960);
				break;
			default:
				break;
        }
		ScreenOrientation = newScreenOrientation;
		_screenOrientation = newScreenOrientation;
    }
}
