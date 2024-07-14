using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_CharacterSelectButton : MonoBehaviour {
	public TMP_Text NameText;
	public string SelectionName;
	public GameObject LockIcon;

	public bool bIsUnlocked = false;
	public void setButtonDetails(string newSelectionName, bool bUnlocked)
    {
		SelectionName = newSelectionName;
        NameText.text = newSelectionName;
		LockIcon.SetActive(!bUnlocked);
		bIsUnlocked = bUnlocked;
    }

    public void SelectCharacter()
    {
		if (bIsUnlocked)
		{
			GameStateControllerScript.Instance.SelectCharacter(SelectionName);
		} else
        {
			LockIcon.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f).SetUpdate(true);
        }
	}
}
