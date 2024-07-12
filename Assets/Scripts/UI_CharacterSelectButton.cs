using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UI_CharacterSelectButton : MonoBehaviour {
	public TMP_Text NameText;
	public string SelectionName;

	public void setButtonDetails(string newSelectionName)
    {
		SelectionName = newSelectionName;
		NameText.text = newSelectionName;
    }

	public void SelectCharacter()
    {
		GameStateControllerScript.Instance.SelectCharacter(SelectionName);
	}
}
