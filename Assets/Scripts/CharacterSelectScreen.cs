using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSelectScreen : MonoBehaviour {
	public GameObject GridDisplayArea;
	public GameObject SelectButtonPrefab;
	// Use this for initialization
	/*
	void Start () {
		PopulateCharacterSelectArea();
	}*/

	void OnEnable()
    {
		PopulateCharacterSelectArea();
	}

	void OnDisable()
    {
		//Clear our children
		foreach (Transform child in GridDisplayArea.transform)
		{
			Destroy(child.transform.gameObject);
		}
	}

	
	public void PopulateCharacterSelectArea()
    {
		//Clear our children
		foreach(Transform child in GridDisplayArea.transform)
        {
			Destroy(child.transform.gameObject);
        }

		foreach (CharacterGroup characterGroup in GameStateControllerScript.Instance.CharacterGroups)
        {
			foreach(SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
            {
				GameObject newButton = Instantiate(SelectButtonPrefab, GridDisplayArea.transform);
				newButton.GetComponent<UI_CharacterSelectButton>().setButtonDetails(thisCharacter.CharacterName, thisCharacter.Unlocked);
			}
        }
    }
}
