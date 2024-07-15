using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsButton : MonoBehaviour {
	public AudioSource targetToggle;
	public string targetPrefs;
	public bool bState = true;
	Toggle ourButton;

	// Use this for initialization
	void Start () {
		ourButton = gameObject.GetComponent<Toggle>();
		if (PlayerPrefs.HasKey(targetPrefs))
		{
			bState = PlayerPrefs.GetInt(targetPrefs) == 1;
			targetToggle.enabled = bState; //.SetActive(bState);
			ourButton.isOn = bState;
		} else
        {
			PlayerPrefs.SetInt(targetPrefs, 1);
			targetToggle.enabled = bState; //.SetActive(bState);
			ourButton.isOn = bState;
		}
	}

	public void ToggleState(bool bNewState)
    {
		PlayerPrefs.SetInt(targetPrefs, bNewState? 1 : 0);
		bState = bNewState;
		targetToggle.enabled = bState; //.SetActive(bState);
	}	
}
