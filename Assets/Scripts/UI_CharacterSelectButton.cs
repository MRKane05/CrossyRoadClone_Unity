using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class UI_CharacterSelectButton : MonoBehaviour {
	public TMP_Text NameText;
	public string SelectionName;
	public GameObject LockIcon;
	public GameObject CharacterAnchor;
	public bool bIsUnlocked = false;
	public void setButtonDetails(string newSelectionName, GameObject newPrefab, bool bUnlocked)
    {
		SelectionName = newSelectionName;
        NameText.text = newSelectionName;
		LockIcon.SetActive(!bUnlocked);
		bIsUnlocked = bUnlocked;

		//Quickly spawn our character for display
		GameObject displayCharacter = Instantiate(newPrefab, CharacterAnchor.transform);
		displayCharacter.transform.localPosition = Vector3.zero;
		displayCharacter.transform.localScale = Vector3.one;
		CharacterAnchor.transform.localEulerAngles = new Vector3(0, 145f, 0);
    }

    public void SelectCharacter()
    {
		if (bIsUnlocked)
		{
			GameStateControllerScript.Instance.SelectCharacter(SelectionName);
		} else
        {
			LockIcon.transform.DOPunchScale(Vector3.one * 0.5f, 0.5f).SetUpdate(true).OnComplete(() => { LockIcon.transform.localScale = Vector3.one; }); ;
        }
	}

	public void Update()
    {
		CharacterAnchor.transform.localEulerAngles += Vector3.up * Time.deltaTime * 30f / Time.timeScale;
	}
}
