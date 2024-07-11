using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using static CanvasRotator;

public class GameStateControllerScript : MonoBehaviour {

    public static GameStateControllerScript Instance { get; private set; }
    public CanvasRotator canvasRotator;

    public enScreenOrientation ScreenOrientation = enScreenOrientation.LANDSCAPE;

    public GameObject mainMenuCanvas;
    public GameObject playCanvas;
    public GameObject gameOverCanvas;
    public GameObject optionsMenu;

    public Text playScore;
    public Text gameOverScore;
    public Text topScore;
    public Text playerName;

    public GameObject EagleObject;

    public int score, top;

    private GameObject currentCanvas;
    public enum enGameState { NULL, MAINMENU, PLAY, GAMEOVER, OPTIONS, CHARACTERSELECT }
    public enGameState state = enGameState.MAINMENU;
    enGameState prepause_state = enGameState.MAINMENU;

    public string filename = "top.txt";

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
        //EagleObject.GetComponent<EagleScript>().SetEagleMovement(onThisPosition + Vector3.forward * eagleRange, onThisPosition - Vector3.forward * eagleRange);
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject); // This will make sure the instance is not destroyed between scenes
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
    }

    public void Update() {
        if (state == enGameState.PLAY) {//"play") {
            //topScore.text = PlayerPrefs.GetInt("Top").ToString();
            playScore.text = score.ToString();
            //playerName.text = PlayerPrefs.GetString("Name");
        }
        else if (state == enGameState.MAINMENU) {//"mainmenu") {
            //We want this bound to a button now
            /*
            if (Input.GetButtonDown("Cancel")) {
                Application.Quit();
            }
            else if (Input.anyKeyDown) {
                Play();
            }*/
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
    }

    public void MainMenu() {
        CurrentCanvas = mainMenuCanvas;
        state = enGameState.MAINMENU;// "mainmenu";

        //PROBLEM: This is terrible coding!
        /*
        GameObject.Find("LevelController").SendMessage("Reset");
        GameObject.FindGameObjectWithTag("Player").SendMessage("Reset");
        GameObject.FindGameObjectWithTag("MainCamera").SendMessage("Reset");
        */

        /*
        File.SetAttributes(Application.dataPath + "/" + filename, FileAttributes.Normal);
        StreamReader sr = new StreamReader(Application.dataPath + "/" + filename);
        string fileContent = sr.ReadLine();

        sr.Close();
        
        topScore.text = fileContent;
        */
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
        if (score > top) {
            PlayerPrefs.SetInt("Top", top);
            /*
            var sw = File.CreateText(filename);
            sw.Write(top);
            sw.Close();
            */
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
}
