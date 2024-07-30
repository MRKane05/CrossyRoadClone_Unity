using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;

using static CanvasRotator;

[System.Serializable]
public class Powerup_Item
{
    public string PowerupName = "";
    public string PowerupDescription = "";
    public GameObject powerupPrefab;
    public int powerupCost = 125;
    public int count = 0;
    public bool bEnabledThisRun = false;

    public Powerup_Item(string newPowerupName, int newPowerupCount)
    {
        PowerupName = newPowerupName;
        count = newPowerupCount;
    }
}


[System.Serializable]
public class SelectableCharacter
{
    public string CharacterName = "";
    public GameObject Character;
    public int cost = 100;  //Don't know if we'll be buying directly or having a gambling mechanic
    public bool Unlocked = false;
}

[System.Serializable]
public class CharacterGroup
{
    public string GroupName = "";
    public List<SelectableCharacter> GroupCharacters = new List<SelectableCharacter>();
}

public class GameStateControllerScript : MonoBehaviour {

    public List<CharacterGroup> CharacterGroups = new List<CharacterGroup>();
    public List<Powerup_Item> PowerupItems = new List<Powerup_Item>();
    public static GameStateControllerScript Instance { get; private set; }
    public CanvasRotator canvasRotator;

    public enScreenOrientation ScreenOrientation = enScreenOrientation.LANDSCAPE;

    public GameObject mainMenuCanvas;
    public GameObject playCanvas;
    public GameObject gameOverCanvas;
    public GameObject optionsMenu;
    public GameObject characterSelect;
    public GameObject prizeMenu;
    public GameObject powerupMenu;

    public BonusNotification playNotification;

    public Text playScore;
    public Text gameOverScore;
    public Text topScore;
    public Text playerName;

    public GameObject EagleObject;

    public int score, score_top, coins;
    public int _score;
    public Text CoinsDisplay;

    private GameObject currentCanvas;
    public enum enGameState { NULL, MAINMENU, PLAY, GAMEOVER, OPTIONS, CHARACTERSELECT, PRIZEMENU, POWERUPMENU }
    public enGameState state = enGameState.MAINMENU;
    enGameState prepause_state = enGameState.MAINMENU;

    public string filename = "top.txt";

    //Details for keeping a ticker on our score beating
    bool bPassedHalfway = false;
    bool bBeatTopScore = false;
    int passedIntiger = 1; //This will be multiplied by 25 to mark milestones



    public void UISetScreenOrientation(string orientation)
    {
        PlayerPrefs.SetString("ScreenOrientation", orientation);
        PlayerPrefs.Save();
        switch (orientation)
        {
            case "landscape":
                SetScreenOrientation(enScreenOrientation.LANDSCAPE);
                break;
            case "left":
                SetScreenOrientation(enScreenOrientation.LEFT);
                break;
            case "right":
                SetScreenOrientation(enScreenOrientation.RIGHT);
                break;
            default:
                SetScreenOrientation(enScreenOrientation.LANDSCAPE);
                break;
        }
    }

    public void SetScreenOrientation(enScreenOrientation newScreenOrientation)
    {
        //of course we need to know our rotator for our UI...
        canvasRotator.SetScreenOrientation(newScreenOrientation);
        LevelControllerScript.Instance.camera.GetComponent<CameraMovementScript>().SetScreenOrientation(newScreenOrientation);
        ScreenOrientation = newScreenOrientation;
    }

    public void TriggerEagle(Vector3 onThisPosition)
    {
        float eagleRange = 30f;
        EagleObject.GetComponent<EagleScript>().SetEagleMovement(onThisPosition + Vector3.forward * eagleRange, onThisPosition - Vector3.forward * eagleRange);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensures there is only one instance
        }
    }

    public void Start() {
        currentCanvas = null;
        MainMenu();
        //SetScreenOrientation(ScreenOrientation);

        //Load our prefs details
        SetTopScore(score_top);
        SetCoinsDisplay(coins);
        playScore.text = "0";   //Set our scores

        bPassedHalfway = false;
        bBeatTopScore = false;
        passedIntiger = 1;
        optionsMenu.SetActive(true); //Turn this on so that our prefs can take effect

        //handle our screent orientation
        string ScreenSet = PlayerPrefs.GetString("ScreenOrientation");
        if (ScreenSet.Length > 2)
        {
            UISetScreenOrientation(ScreenSet);
        }
    }

    public void SetTopScore(int toThis)
    {
        score_top = toThis;
        topScore.text = toThis.ToString();
    }

    public void Update() {
        //Add some coins for testing
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //ChangeCoinTotal(100);
        }

        if (state == enGameState.PLAY) {//"play") {
            //topScore.text = PlayerPrefs.GetInt("Top").ToString();
            //we really shouldn't update this every tick!
            if (_score != score)
            {
                _score = score;
                playScore.text = score.ToString();
                if (score > score_top && score_top > 0)
                {
                    if (!bBeatTopScore)
                    {
                        bBeatTopScore = true;
                        playNotification.DisplayRect("Top score\nget 25c");
                        ChangeCoinTotal(25);
                    }
                    SetTopScore(score);
                    topScore.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f).OnComplete(() => { topScore.transform.localScale = Vector3.one; }); ;  //This is big! We set our top score!
                }
                if (score > score_top / 2f && score_top > 0)
                {
                    if (!bPassedHalfway)
                    {
                        bPassedHalfway = true;
                        playNotification.DisplayRect("halfway\nget 15c");
                        ChangeCoinTotal(15);
                    }
                }
                if (score > passedIntiger * 25)
                {
                    int coinReward = 3 * passedIntiger;
                    playNotification.DisplayRect("passed " + passedIntiger * 25 + "\n get " + coinReward);
                    ChangeCoinTotal(coinReward);
                    passedIntiger++;
                }

                //We could do with some other benchmark things, like "passed 25 lines"
            }
        }
        else if (state == enGameState.MAINMENU) {//"mainmenu") {

        }
        else if (state == enGameState.GAMEOVER) {//"gameover") {
            if (Input.anyKeyDown) {
                //Application.LoadLevel("Menu");
                //We want to do a reload this time, and that'll involve the LevelControllerScript
                RestartGame();
            }
        }
    }

    public void RestartGame()
    {
        LevelControllerScript.Instance.GameReset(); //PROBLEM: The reset system needs a lot of refinement
        MainMenu();
        bPassedHalfway = false;
        bBeatTopScore = false;
        passedIntiger = 1;
    }

    public void MainMenu() {
        CurrentCanvas = mainMenuCanvas;
        state = enGameState.MAINMENU;// "mainmenu";
    }

    public void Play() {
        CurrentCanvas = playCanvas;
        state = enGameState.PLAY;// "play";
        score = 0;

        

        //Need to talk to our powerup handler and see if we've got powerups selected, and if so pass that information through to our level controller
        List<string> equippedPowerups = PowerupHandler.Instance.getEquippedPowerups();
        Debug.Log("Equipped Powerups: " + equippedPowerups.Count);
        LevelControllerScript.Instance.setPowerups(equippedPowerups);

        //We need to go through our equipped powerups and remove them from our internal count register
        foreach (string powerupName in equippedPowerups)
        {
            ChangePowerupCount(powerupName, -1);
        }

        LevelControllerScript.Instance.Play();
    }

    public void GameOver() {
        CurrentCanvas = gameOverCanvas;
        state = enGameState.GAMEOVER;// "gameover";

        gameOverScore.text = score.ToString();
        if (score > score_top) {
            SetTopScore(score);
        }
        LevelControllerScript.Instance.camera.GetComponent<CameraMovementScript>().moving = false;
        //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovementScript>().moving = false;
        SaveScoreUtility.Instance.SaveGameInformation();
    }

    public void SelectOptions(bool doOpen)
    {
        Time.timeScale = doOpen ? 0.001f : 1f;  //Pause our game
        if (doOpen)
        {
            prepause_state = state; //So we know what to come back to
            state = enGameState.OPTIONS;
        } else
        {
            state = prepause_state; //Return to whatever we were :)
        }
        optionsMenu.SetActive(doOpen);
    }

    public void SelectCharacter(bool doOpen)
    {
        Time.timeScale = doOpen ? 0.001f : 1f;  //Pause our game
        if (doOpen)
        {
            prepause_state = state; //So we know what to come back to
            state = enGameState.CHARACTERSELECT;
        }
        else
        {
            state = prepause_state; //Return to whatever we were :)
        }
        characterSelect.SetActive(doOpen);
    }

    public void SetCharacterLockState(string characterName, bool state)
    {
        foreach (CharacterGroup characterGroup in CharacterGroups)
        {
            foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
            {
                if (thisCharacter.CharacterName == characterName)
                {
                    thisCharacter.Unlocked = state;
                }
            }
        }
    }

    public void SelectPrizeMenu(bool doOpen)
    {
        Time.timeScale = doOpen ? 0.001f : 1f;  //Pause our game
        prizeMenu.SetActive(doOpen);
        
        if (doOpen)
        {
            prepause_state = state; //So we know what to come back to
            state = enGameState.PRIZEMENU;
            prizeMenu.GetComponent<PrizeMenuHandler>().OpenMenu();
        }
        else
        {
            state = prepause_state; //Return to whatever we were :)
        }
    }

    public void SelectPowerupMenu(bool doOpen)
    {
        Time.timeScale = doOpen ? 0.001f : 1f;  //Pause our game
        powerupMenu.SetActive(doOpen);

        if (doOpen)
        {
            prepause_state = state; //So we know what to come back to
            state = enGameState.PRIZEMENU;
            //powerupMenu.GetComponent<PowerupMenuItem>().OpenMenu();
            gameObject.GetComponent<PowerupHandler>().OpenMenu();
        }
        else
        {
            state = prepause_state; //Return to whatever we were :)
        }
    }

    private GameObject CurrentCanvas {
        get {
            return currentCanvas;
        }
        set {
            if (currentCanvas != null) {
                currentCanvas.SetActive(false);
            }
            currentCanvas = value;
            currentCanvas.SetActive(true);
        }
    }

    public void SelectCharacter(string thisCharacterName)
    {
        //So here we need to get the target character, and send the message through to the player that we want it changed
        foreach (CharacterGroup characterGroup in CharacterGroups)
        {
            foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
            {
                if (thisCharacterName == thisCharacter.CharacterName)
                {
                    LevelControllerScript.Instance.player.GetComponent<PlayerMovementScript>().SetCharacter(thisCharacter.Character);
                }
            }
        }

        //And after this we need to close our window
        SelectCharacter(false);
    }

    public void ChangeCoinTotal(int byThis)
    {
        //Debug.Log("Changing Coin Total: " + byThis);   
        coins += byThis;
        //PlayerPrefs.SetInt("Coins", coins); //Update our new prefs
        SetCoinsDisplay(coins);
    }

    public void ChangePowerupCount(string itemName, int countChange)
    {
        bool bHasAdded = false;
        foreach(Powerup_Item thisPowerup in PowerupItems)
        {
            if (thisPowerup.PowerupName == itemName)
            {
                bHasAdded = true;
                thisPowerup.count += countChange;
            }
        }
        //Ok, so we didn't get to add that powerup. Annoying. Lets see if our powerup handler has it and we'll add it to our list
        if (!bHasAdded)
        {
            foreach (Powerup_Item newPowerup in PowerupHandler.Instance.PowerupItems)
            {
                if (newPowerup.PowerupName == itemName)
                {
                    PowerupItems.Add(newPowerup);
                    //Logically it'll be at the end
                    PowerupItems[PowerupItems.Count - 1].count += countChange;
                }
            }
        }
    }

    public void setCoinTotal(int toThis)
    {
        coins = toThis;
        SetCoinsDisplay(toThis);
    }

    public void SetCoinsDisplay(int toThis)
    {
        CoinsDisplay.text = toThis.ToString();
        CoinsDisplay.transform.DOPunchScale(Vector3.one * 0.25f, 0.75f).SetUpdate(true).OnComplete(() => { CoinsDisplay.transform.localScale = Vector3.one; }); //To show that we've got something :)
    }
}
