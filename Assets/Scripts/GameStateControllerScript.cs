using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using TMPro;
using DG.Tweening;

using static CanvasRotator;


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

    public static GameStateControllerScript Instance { get; private set; }
    public CanvasRotator canvasRotator;

    public enScreenOrientation ScreenOrientation = enScreenOrientation.LANDSCAPE;

    public GameObject mainMenuCanvas;
    public GameObject playCanvas;
    public GameObject gameOverCanvas;
    public GameObject optionsMenu;
    public GameObject characterSelect;
    public GameObject prizeMenu;

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
    public enum enGameState { NULL, MAINMENU, PLAY, GAMEOVER, OPTIONS, CHARACTERSELECT, PRIZEMENU }
    public enGameState state = enGameState.MAINMENU;
    enGameState prepause_state = enGameState.MAINMENU;

    public string filename = "top.txt";

    //Details for keeping a ticker on our score beating
    bool bPassedHalfway = false;
    bool bBeatTopScore = false;
    int passedIntiger = 1; //This will be multiplied by 25 to mark milestones



    public void UISetScreenOrientation(string orientation)
    {
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
        SetScreenOrientation(ScreenOrientation);

        //Load our prefs details
        coins = PlayerPrefs.GetInt("Coins");
        score_top = PlayerPrefs.GetInt("TopScore"); //Retrieve our top score
        topScore.text = score_top.ToString();
        SetCoinsDisplay(coins);
        playScore.text = "0";   //Set our scores

        bPassedHalfway = false;
        bBeatTopScore = false;
        passedIntiger = 1;
        optionsMenu.SetActive(true); //Turn this on so that our prefs can take effect
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
                    score_top = score;
                    PlayerPrefs.SetInt("TopScore", score_top);
                    topScore.text = score_top.ToString();
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
                    passedIntiger++;
                    playNotification.DisplayRect("passed " + passedIntiger * 25 + "\n get " + coinReward);
                    ChangeCoinTotal(coinReward);
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

        LevelControllerScript.Instance.player.GetComponent<PlayerMovementScript>().canMove = true;
        LevelControllerScript.Instance.camera.GetComponent<CameraMovementScript>().moving = true;
        //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovementScript>().canMove = true;
        //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovementScript>().moving = true;
    }

    public void GameOver() {
        CurrentCanvas = gameOverCanvas;
        state = enGameState.GAMEOVER;// "gameover";

        gameOverScore.text = score.ToString();
        if (score > score_top) {
            score_top = score;
            //PlayerPrefs.SetInt("TopScore", score_top);
            topScore.text = score_top.ToString();
            //topScore.transform.DOPunchScale(Vector3.one * 1.1f, 0.5f);  //This is big! We set our top score!
        }
        LevelControllerScript.Instance.camera.GetComponent<CameraMovementScript>().moving = false;
        //GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraMovementScript>().moving = false;
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
        Debug.Log("Changing Coin Total: " + byThis);   
        coins += byThis;
        PlayerPrefs.SetInt("Coins", coins); //Update our new prefs
        SetCoinsDisplay(coins);
    }

    public void SetCoinsDisplay(int toThis)
    {
        CoinsDisplay.text = toThis.ToString();
        CoinsDisplay.transform.DOPunchScale(Vector3.one * 0.25f, 0.75f).OnComplete(() => { CoinsDisplay.transform.localScale = Vector3.one; }); //To show that we've got something :)
    }
}
