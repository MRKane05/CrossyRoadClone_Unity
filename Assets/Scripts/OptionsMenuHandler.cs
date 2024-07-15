using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionsMenuHandler : MonoBehaviour {

	// Turn this off after a tick so that all of our prefs settings will be propigated throughout
	IEnumerator Start () {
		yield return null;
		gameObject.SetActive(false);		
	}
}
