using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PrizeMenuHandler : MonoBehaviour {
    public List<SelectableCharacter> CharacterList = new List<SelectableCharacter>();
    public GameObject displayArea;
    public GameObject displayIcon;
    public GameObject prizeMachineGraphic;
    public TextMeshProUGUI WinningTitle;
    public GameObject alreadyOwnedLabel;
    public Sprite CharacterIcon, MoneyIcon, PowerupIcon;
    [Space]
    [Header("Bonus Notice")]
    public BonusNotification BonusNotice;
    [Space]
    public AudioClip sound_SelectPrize, sound_Character, sound_Powerup, sound_Cash;

    AudioSource ourAudio;

    bool bPrizeRunning = false;
    public void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
    }

    //As this needs a default state start
    public void OpenMenu()
    {
        WinningTitle.text = "";  //Clear our text
        displayIcon.SetActive(false);
        displayArea.SetActive(false);
        alreadyOwnedLabel.SetActive(false);
        Time.timeScale = 1f;    //Set our time scale just in case it got screwy with the menu
    }

    public void PlayPrizeMachine()
    { 
        if (!bPrizeRunning)
        {
            GameStateControllerScript.Instance.ChangeCoinTotal(-100);
            OpenMenu(); //reset everything to our default state
            StartCoroutine(DoPrizeMachine());
        }
    }

    IEnumerator DoPrizeMachine() { 
        bPrizeRunning = true;
        //So basically this needs to look at our characters, powerups (which will also be purchasable), and a monetry reward :)

        //For the moment lets just randomly go through our characters
        CharacterList = new List<SelectableCharacter>();
        foreach (CharacterGroup characterGroup in GameStateControllerScript.Instance.CharacterGroups)
        {
            foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
            {
                CharacterList.Add(thisCharacter);
            }
        }

        //Select a character as a prize
        SelectableCharacter newPrize = CharacterList[Mathf.FloorToInt(Random.RandomRange(0, CharacterList.Count))];
        //newPrize = new SelectableCharacter();
        //newPrize.CharacterName = "Chicken";
        //newPrize.Unlocked = true;

        Debug.Log(newPrize.CharacterName);
        ourAudio.clip = sound_SelectPrize;
        ourAudio.Play();

        //prizeMachineGraphic.transform.DOPunchScale(Vector3.one * 0.25f, 1.2f);
        prizeMachineGraphic.transform.DOShakeScale(1.25f);

        yield return new WaitForSeconds(1.2f);  //Wait for the play, and maybe do something with the backing icon

        //We need some way of selecting what prize we'll be giving out.

        ourAudio.clip = sound_Character;
        ourAudio.Play();

        displayArea.SetActive(true);
        displayArea.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);
        WinningTitle.text = newPrize.CharacterName;
        displayIcon.SetActive(true);
        displayIcon.GetComponent<Image>().sprite = CharacterIcon;

        if (!newPrize.Unlocked)
        {
            //Ok, awesome, we've got something here. Now to see if it's already unlocked, and if not we should give a monetry return :)
            bool bAlreadyUnlocked = false;
            foreach (CharacterGroup characterGroup in GameStateControllerScript.Instance.CharacterGroups)
            {
                foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
                {
                    if (thisCharacter.CharacterName == newPrize.CharacterName)
                    {
                        if (thisCharacter.Unlocked)
                        {
                            //We need to go onto the money display route
                        }
                        else
                        {
                            //thisCharacter.Unlocked = true;  //Unlock this character for our player
                            GameStateControllerScript.Instance.SetCharacterLockState(thisCharacter.CharacterName, true);
                        }
                    }
                }
            }
        } else
        {
            yield return new WaitForSeconds(1.5f);
            ourAudio.clip = sound_Cash;
            ourAudio.Play();
            alreadyOwnedLabel.SetActive(true);
            alreadyOwnedLabel.transform.DOPunchScale(Vector3.one * 1.5f, 0.75f);
            GameStateControllerScript.Instance.ChangeCoinTotal(75); //Give our player 75 coins
        }

        //We'd be wise to save at this point also
        SaveScoreUtility.Instance.SaveGameInformation();
        bPrizeRunning = false; //Free up our system for re-use
    }
}
