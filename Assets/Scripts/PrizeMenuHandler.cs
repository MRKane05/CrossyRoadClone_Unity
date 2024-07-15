using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class PrizeMenuHandler : MonoBehaviour {
    public List<SelectableCharacter> CharacterList = new List<SelectableCharacter>();
    [Header("Character Display Values")]
    public GameObject displayArea;
    public GameObject displayAnchor; //What our prefab will be displayed on
    public GameObject displayIcon;
    public GameObject prizeMachineGraphic;
    public TextMeshProUGUI WinningTitle;
    public GameObject alreadyOwnedLabel;
    public Sprite CharacterIcon, MoneyIcon, PowerupIcon;
    [Space]
    [Header("Finesse Elements")]
    public Button DoSpinButton, PlayButton;
    public GameObject InsufficentCoinsNotice;
    public TextMeshProUGUI InsufficentCoinsText;
    public AudioClip sound_SelectPrize, sound_Character, sound_Powerup, sound_Cash;

    AudioSource ourAudio;
    string UnlockedCharcter = "";

    bool bPrizeRunning = false;
    public void Start()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();
        //Vector3 q = gameObject.transform.rotation * new Vector3(0, 360, 0);
        //displayAnchor.transform.DORotate(q, 5).SetLoops(-1).SetEase(Ease.Linear);
    }

    //As this needs a default state start
    public void OpenMenu()
    {
        WinningTitle.text = "";  //Clear our text
        displayIcon.SetActive(false);
        displayArea.SetActive(false);
        alreadyOwnedLabel.SetActive(false);
        displayAnchor.SetActive(false);
        //Clear our gameobject children
        foreach (Transform child in displayAnchor.transform)
        {
            Destroy(child.gameObject);
        }
        Time.timeScale = 1f;    //Set our time scale just in case it got screwy with the menu

        //We need to look at what our current setup is and disable buttons as necessary/show notices
        UnlockedCharcter = "";
        //PlayButton.interactable = false;
        DoSpinButton.interactable = GameStateControllerScript.Instance.coins > 100;
        InsufficentCoinsNotice.SetActive(GameStateControllerScript.Instance.coins < 100);
        if (GameStateControllerScript.Instance.coins < 100)
        {
            InsufficentCoinsText.text = "Not enough coins\nyou need " + (100 - GameStateControllerScript.Instance.coins) + " more";
        }
    }

    void Update()
    {
        displayAnchor.transform.localEulerAngles += Vector3.up * Time.deltaTime * 30f;
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

        //Put our character on the display point
        displayAnchor.SetActive(true);
        GameObject characterPrefab = Instantiate(newPrize.Character, displayAnchor.transform);
        characterPrefab.transform.localPosition = Vector3.zero;
        characterPrefab.transform.localScale = Vector3.one;
        displayAnchor.transform.localEulerAngles = new Vector3(0, 125, 0);  //Set our angle
        displayAnchor.transform.DOShakeScale(0.5f);

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
                        //thisCharacter.Unlocked = true;  //Unlock this character for our player
                        GameStateControllerScript.Instance.SetCharacterLockState(thisCharacter.CharacterName, true);
                        UnlockedCharcter = thisCharacter.CharacterName;
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
        //PlayButton.interactable = true; // UnlockedCharcter == "" ? false : true;    //Set our play button depending on what we got
        //We'd be wise to save at this point also
        SaveScoreUtility.Instance.SaveGameInformation();
        bPrizeRunning = false; //Free up our system for re-use
    }

    public void PlayWithNewCharacter()
    {
        if (UnlockedCharcter != "")
        {
            GameStateControllerScript.Instance.SelectCharacter(UnlockedCharcter);
        }
        GameStateControllerScript.Instance.SelectPrizeMenu(false);
        GameStateControllerScript.Instance.Play();
    }
}
