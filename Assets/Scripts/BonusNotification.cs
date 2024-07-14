using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BonusNotification : MonoBehaviour {
	public TextMeshProUGUI messageText;

	RectTransform ourRect;
	float offsetDistance = 400f;
	Vector3 startPosition = Vector3.zero;

	float startDisplayTime = 0;
	float targetPosition = 1;
	float lerpPosition = 1;

	float displayTime = 3f;

	public bool bDoDisplay = false;
	// Use this for initialization
	void Start () {
		ourRect = gameObject.GetComponent<RectTransform>();
	}

	public void DisplayRect(string NotificationMessage)
    {
		startDisplayTime = Time.realtimeSinceStartup;
		targetPosition = 0;
		messageText.text = NotificationMessage;

	}

	void Update()
    {
		lerpPosition = Mathf.Lerp(lerpPosition, targetPosition, Time.fixedDeltaTime * 3f);
		ourRect.anchoredPosition = new Vector2(-offsetDistance * lerpPosition, 0); //, 0);
		//ourRect.Pos
		if (bDoDisplay)
        {
			bDoDisplay = false;
			DisplayRect("You Pressed Button");

		}

		if (Time.realtimeSinceStartup > startDisplayTime + displayTime && targetPosition != 1)
		{
			targetPosition = 1;
		}
    }

	public void Dismiss()
    {
		targetPosition = 1;
    }

}
