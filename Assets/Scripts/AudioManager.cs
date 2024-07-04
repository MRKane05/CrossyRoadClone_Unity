using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	public static AudioManager Instance { get; private set; }

	void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will make sure the instance is not destroyed between scenes
        }
        else
        {
            Destroy(gameObject); // Ensures there is only one instance
        }
    }
}
