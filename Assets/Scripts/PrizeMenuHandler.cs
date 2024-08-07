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
    public Button DoSpinButton, PlayButton, CloseButton;
    public GameObject InsufficentCoinsNotice;
    public TextMeshProUGUI InsufficentCoinsText;
    public AudioClip sound_SelectPrize, sound_Character, sound_Powerup, sound_Cash;

    AudioSource ourAudio;
    string UnlockedCharcter = "";

    float CharacterOdds = 0.4f; //If we're below this we'll get a character
    float PowerupOdds = 0.6f; //If we're above this we'll get a powerup. If we're neither we'll get money

    bool bPrizeRunning = false;
    bool bCanRotate = false;
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
        CheckInsufficentDisplay();
    }

    void Update()
    {
        if (bCanRotate)
        {
            displayAnchor.transform.localEulerAngles += Vector3.up * Time.deltaTime * 30f;
        }
    }

    public void PlayPrizeMachine()
    {
        OpenMenu();
        if (!bPrizeRunning)
        {
            GameStateControllerScript.Instance.ChangeCoinTotal(-100);
            //OpenMenu(); //reset everything to our default state
            StartCoroutine(DoPrizeMachine());
        }
    }

    IEnumerator DoPrizeMachine() { 
        bPrizeRunning = true;

        DoSpinButton.interactable = false;
        PlayButton.interactable = false;
        CloseButton.interactable = false;
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
        bCanRotate = false;
        //Select a character as a prize

        //newPrize = new SelectableCharacter();
        //newPrize.CharacterName = "Chicken";
        //newPrize.Unlocked = true;

        //Debug.Log(newPrize.CharacterName);
        ourAudio.clip = sound_SelectPrize;
        ourAudio.Play();

        //prizeMachineGraphic.transform.DOPunchScale(Vector3.one * 0.25f, 1.2f);
        prizeMachineGraphic.transform.DOShakeScale(1.25f);

        yield return new WaitForSeconds(1.2f);  //Wait for the play, and maybe do something with the backing icon

        //We need some way of selecting what prize we'll be giving out.

        displayArea.SetActive(true);
        displayArea.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f).OnComplete(() => { displayArea.transform.localScale = Vector3.one;}); ;
        displayIcon.SetActive(true);
        
        //We need to see about setting things up for character/money/powerup
        float prizeDraw = Random.value;

        //First up our character :)
        if (prizeDraw < CharacterOdds)
        {
            ourAudio.clip = sound_Character;
            ourAudio.Play();
            SelectableCharacter newPrize = CharacterList[Mathf.FloorToInt(Random.RandomRange(0, CharacterList.Count))];
            WinningTitle.text = newPrize.CharacterName;

            displayIcon.GetComponent<Image>().sprite = CharacterIcon;
            //Put our character on the display point
            displayAnchor.SetActive(true);
            GameObject characterPrefab = Instantiate(newPrize.Character, displayAnchor.transform);
            characterPrefab.transform.localPosition = Vector3.zero;
            characterPrefab.transform.localScale = Vector3.one;
            displayAnchor.transform.localEulerAngles = new Vector3(0, 125, 0);  //Set our angle
            displayAnchor.transform.DOShakeScale(0.5f).SetUpdate(true).OnComplete(() => { bCanRotate = true; }) ;

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
            }
            else
            {
                yield return new WaitForSeconds(1.5f);
                ourAudio.clip = sound_Cash;
                ourAudio.Play();
                alreadyOwnedLabel.SetActive(true);
                alreadyOwnedLabel.transform.DOPunchScale(Vector3.one * 1.5f, 0.75f).OnComplete(() => { alreadyOwnedLabel.transform.localScale = Vector3.one;});
                GameStateControllerScript.Instance.ChangeCoinTotal(75); //Give our player 75 coins
                
            }
        } else if (prizeDraw > PowerupOdds) //get a powerup
        {
            //Ok, we need to pick a powerup from our list, and award it to the player
            int selectedPowerup = Mathf.Clamp(Random.RandomRange(0, PowerupHandler.Instance.PowerupItems.Count), 0, PowerupHandler.Instance.PowerupItems.Count-1);
            Powerup_Item WonPowerup = PowerupHandler.Instance.PowerupItems[selectedPowerup];

            displayAnchor.SetActive(true);
            GameObject characterPrefab = Instantiate(WonPowerup.powerupPrefab, displayAnchor.transform);
            characterPrefab.transform.localPosition = Vector3.zero;
            characterPrefab.transform.localScale = Vector3.one;
            displayAnchor.transform.localEulerAngles = new Vector3(0, 125, 0);  //Set our angle
            displayAnchor.transform.DOShakeScale(0.5f, 100).SetUpdate(true).OnComplete(() => { bCanRotate = true; });

            displayIcon.GetComponent<Image>().sprite = WonPowerup.powerupSprite;

            WinningTitle.text = WonPowerup.PowerupName;
            ourAudio.clip = sound_Powerup;
            ourAudio.Play();

            //And finally we've got to add our powerup!
            GameStateControllerScript.Instance.ChangePowerupCount(WonPowerup.PowerupName, 1);

        } else //You get cash!
        {
            int cashAmount = 50 + 25 * Mathf.FloorToInt(Random.RandomRange(0, 6));
            WinningTitle.text = "Coins: " + cashAmount;
            GameStateControllerScript.Instance.ChangeCoinTotal(cashAmount);

            displayIcon.GetComponent<Image>().sprite = MoneyIcon;
            //yield return new WaitForSeconds(1.5f);
            ourAudio.clip = sound_Cash;
            ourAudio.Play();
            //alreadyOwnedLabel.SetActive(true);
            //alreadyOwnedLabel.transform.DOPunchScale(Vector3.one * 1.5f, 0.75f);
            //GameStateControllerScript.Instance.ChangeCoinTotal(75); //Give our player 75 coins


        }

        //CheckInsufficentDisplay();

        DoSpinButton.interactable = GameStateControllerScript.Instance.coins > 100;
        PlayButton.interactable = true;
        CloseButton.interactable = true;
        //We'd be wise to save at this point also
        SaveScoreUtility.Instance.SaveGameInformation();
        bPrizeRunning = false; //Free up our system for re-use
    }

    void CheckInsufficentDisplay()
    {
        DoSpinButton.interactable = GameStateControllerScript.Instance.coins > 100;
        InsufficentCoinsNotice.SetActive(GameStateControllerScript.Instance.coins < 100);
        if (GameStateControllerScript.Instance.coins < 100)
        {
            InsufficentCoinsText.text = "Not enough coins\nyou need " + (100 - GameStateControllerScript.Instance.coins) + " more";
        }
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
