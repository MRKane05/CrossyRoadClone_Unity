using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class LevelControllerScript : MonoBehaviour {
    public LayerMask stopperLayerMask;
    public enum enGameState { NULL, STARTSCREEN, PAUSED, PLAYING, GAMEOVER}
    public enGameState GameState = enGameState.STARTSCREEN;

    public static LevelControllerScript Instance { get; private set; }

    public int minZ = 3;
    public int lineAhead = 10;
    public int lineBehind = 10;

    public GameObject[] linePrefabs;
    public GameObject coins;

    public Dictionary<int, GameObject> lines;

    public GameObject player;
    public GameObject camera;

    private Vector3 playerPosition = Vector3.zero;
    private float playerHighest = 0;
    private int playerHighestRow = 0;
    private int mapLine = 0;

    public List<LineHandler> SpawnedLines = new List<LineHandler>();
    [Space]
    [Header("Powerup settings")]
    int currentPowerup = 0;
    public List<string> equippedPowerups = new List<string>();
    public int minPowerupSpacing = 7; //This is the distance we can be tossed by the catapult. The rest of the powerups will be spaced according to top score
    int nextPowerupLine = 0;
    int maxPowerupSpread = 0;
    int powerupRandom = 0;
    public List<GameObject> activatedPowerups = new List<GameObject>();
    public GameObject powerupButtonsBase;
    public GameObject powerupButtonPrefab;

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
        print("StartingLevel");
        player = GameObject.FindGameObjectWithTag("Player");
        lines = new Dictionary<int, GameObject>();
        playerPosition = player.transform.position;
        playerHighest = player.transform.position.z;
        //Setup our array set
        SpawnInitialMap();
	}

    public void setPowerups(List<string> newPowerups)
    {
        //equippedPowerups = newPowerups;
        //Lets add buttons with the powerups for our system to use. Man I didn't think this out did I?
        foreach(string thisPowerup in newPowerups)
        {
            foreach(Powerup_Item targetPowerup in PowerupHandler.Instance.PowerupItems)
            {
                if (targetPowerup.PowerupName == thisPowerup)
                {
                    GameObject newPowerupButton = Instantiate(powerupButtonPrefab, powerupButtonsBase.transform);
                    LivePowerupButton newButtonScript = newPowerupButton.GetComponent<LivePowerupButton>();
                    newButtonScript.setupButton(targetPowerup.powerupPrefab, targetPowerup.powerupSprite);
                } 
            }
        }
    }

    public bool addPowerupToLevel(GameObject thisPowerupPrefab)
    {
        //So we've got to look at where the player is, and take the next three (optimially) lines ahead of the player
        //Keep looping until we find one that's not water (because I can't be bothered programming a powerup that's on a log) and then
        //Look for a valid placement as we would a coin, then kick forward a line if we fail. Simple.
        //We need to get the player row :)
        int playerRow = Mathf.RoundToInt(player.transform.position.z / 3f);
        //We need to figure out where we're going to start for our row check
        int rowStart = Mathf.FloorToInt(Random.RandomRange(1, 3));
        bool bLineFound = false;
        int reps = 0;
        while (!bLineFound && reps < 6)
        {
            LineHandler targetLine = SpawnedLines[playerRow + rowStart + reps];
            if (targetLine.LineType != LineHandler.enLineType.WATER)
            {
                bLineFound = addPowerupToLine(playerRow + rowStart + reps, thisPowerupPrefab);
                reps++; //Go forward a line
            } else
            {
                reps++;
            }
        }
        return bLineFound;
    }

    bool addPowerupToLine(int lineNumber, GameObject thisPowerup)
    {
        LineHandler targetLine = SpawnedLines[lineNumber];
        //Debug.LogError("Powerup Dropped on Line: " + lineNumber);
        int cycles = 5;
        bool bCleared = false;
        Vector3 position = Vector3.zero;
        while (!bCleared && cycles > 0)
        {
            position = new Vector3(Random.RandomRange(-9.5f, 9.5f), 0f, targetLine.gameObject.transform.position.z);  //We'll have to have some height stuff sorted out here
            if (!Physics.CheckSphere(position, 0.1f, stopperLayerMask))
            {
                bCleared = true;
            }
            cycles--;
        }
        if (!bCleared)
        {
            return false;
        }
        GameObject newPowerup = Instantiate(thisPowerup);
        Debug.Log("Added powerup: " + thisPowerup.gameObject.name);
        //newCoin.transform.localScale = Vector3.one;
        newPowerup.transform.localPosition = position;	//Who cares if it gets stuck in a tree or something
        Vector3 objectScale = newPowerup.transform.localScale;
        newPowerup.transform.localScale = Vector3.zero;
        //Do a fade in and punch animation
        newPowerup.transform.DOScale(objectScale, 0.5f).OnComplete(() => { newPowerup.transform.DOPunchScale(Vector3.one * 1.125f, 0.5f); });

        activatedPowerups.Add(newPowerup);
        return true;
    }

    public void Play()
    {
        currentPowerup = 0;
        nextPowerupLine = 0;
        /*
        if (equippedPowerups.Count > 0)
        {
            maxPowerupSpread = Mathf.FloorToInt(((float)GameStateControllerScript.Instance.score_top / equippedPowerups.Count) * Random.Range(1.5f, 2f)); //Make our spread a little more trying
        }
        */
        //Unlock and set everything in action :)
        player.GetComponent<PlayerMovementScript>().canMove = true;
        camera.GetComponent<CameraMovementScript>().moving = true;
        /*
        nextPowerupLine = Mathf.Max(minPowerupSpacing, Random.RandomRange(minPowerupSpacing, maxPowerupSpread));
        //We need to add powerups until our visual lines are exhaused here, and then keep adding them as new lines are made
        while (nextPowerupLine < SpawnedLines.Count && currentPowerup < equippedPowerups.Count)
        {
            
            //Debug.Log("NextPowerupLine: " + nextPowerupLine);
            int PowerupReturn = TryToAddPowerup(nextPowerupLine);
            if (PowerupReturn > 0)
            {
                maxPowerupSpread = Mathf.FloorToInt(((float)GameStateControllerScript.Instance.score_top / equippedPowerups.Count) * Random.Range(1.5f, 2f)); //Make our spread a little more trying
                powerupRandom = Mathf.Max(minPowerupSpacing, Random.RandomRange(minPowerupSpacing, maxPowerupSpread));
                Debug.Log("Powerup Random: " + powerupRandom);
                nextPowerupLine += powerupRandom;
            }
            else
            {
                //Move our line forward
                nextPowerupLine++;
            }
        }
        */
    }

    int TryToAddPowerup(int lineNumber)
    {
        if (SpawnedLines.Count < lineNumber)
        {
            Debug.Log("Insufficient lines Spawned to drop powerup. LineNumber: " + lineNumber + " SpawnedLines: " + SpawnedLines.Count);
            return 0; //This'll get picked up later
        }
        //I want to put a cycle in to make sure we don't go adding a powerup to a water line as it's a bit unfair for the player (although fun maybe?)
        LineHandler targetLine = SpawnedLines[lineNumber];

        
        if (targetLine.LineType == LineHandler.enLineType.WATER)
        {
            Debug.Log("Powerup line aborted due to being water");
            return 0;
        }
        Debug.LogError("Powerup Dropped on Line: " + lineNumber);
        string targetPowerup = equippedPowerups[currentPowerup];
        currentPowerup++;
        //We need to get the powerup we'll be using
        foreach(Powerup_Item thisPowerup in GameStateControllerScript.Instance.PowerupItems)
        {
            if (thisPowerup.PowerupName == targetPowerup)
            {
                int cycles = 5;
                bool bCleared = false;
                Vector3 position = Vector3.zero;
                while (!bCleared && cycles > 0)
                {
                    position = new Vector3(Random.RandomRange(-9.5f, 9.5f), 0f, targetLine.gameObject.transform.position.z);  //We'll have to have some height stuff sorted out here
                    if (!Physics.CheckSphere(position, 0.1f, stopperLayerMask))
                    {
                        bCleared = true;
                    }
                    cycles--;
                }
                GameObject newCoin = Instantiate(thisPowerup.powerupPrefab);
                Debug.Log("Added powerup: " + thisPowerup.PowerupName);
                //newCoin.transform.localScale = Vector3.one;
                newCoin.transform.localPosition = position;	//Who cares if it gets stuck in a tree or something
            }
        }
        return 1;
    }

    public void GameReset()
    {
        //Debug.LogError("Doing Reset");
        foreach(LineHandler thisLine in SpawnedLines)
        {
            if (thisLine)
            {
                if (thisLine.gameObject)
                {
                    Destroy(thisLine.gameObject);
                }
            }
        }
        SpawnedLines.Clear();
        SpawnedLines = new List<LineHandler>();

        playerHighestRow = 0;

        //We should clear our dictionary also (shame this can't be sent through as some sort of self-destruct message)
        lines.Clear();
        lines = new Dictionary<int, GameObject>();

        foreach(GameObject thisPowerup in activatedPowerups)
        {
            if (thisPowerup)
            {
                Destroy(thisPowerup);
            }
        }

        activatedPowerups.Clear();
        activatedPowerups = new List<GameObject>();

        //Clear our powerups buttons
        foreach (Transform child in powerupButtonsBase.transform)
        {
            Destroy(child.gameObject);
        }

        //we need to put our character back in it's spot
        player.transform.position = Vector3.up * 1f;     //Set to zero and up 1 unit
        player.transform.localScale = Vector3.one;      //Reset scale just in case we've had something different change

        playerPosition = player.transform.position;
        playerHighest = player.transform.position.z;

        player.GetComponent<PlayerMovementScript>().Reset();
        camera.GetComponent<CameraMovementScript>().Reset();

        SpawnInitialMap();
    }

    public void SpawnInitialMap()
    {
        mapLine = 0;
        for (int i=1; i<lineAhead+1; i++)
        {
            AddNewMapLine(i, i * 3);
            mapLine = i;
        }
    }

    public void PlayerMoved()
    {
        playerHighest = Mathf.Max(playerHighest, player.transform.position.z);
        playerHighestRow = Mathf.FloorToInt(playerHighest / 3f);

        while (mapLine < playerHighestRow + lineAhead) { 

            AddNewMapLine(mapLine, (mapLine) * 3);
            mapLine++;
        }
    }

    public void AddNewMapLine(int z, float newPosition)
    {
        if (!lines.ContainsKey(z))
        {
            Vector3 targetPosition = new Vector3(0, 0, newPosition);
            GameObject selectedPrefab = linePrefabs[Random.Range(0, linePrefabs.Length)];
            GameObject newline = Instantiate(selectedPrefab, targetPosition, Quaternion.identity) as GameObject;
            //Debug.Log("newLine: " + newline);

            newline.transform.position = targetPosition;
            newline.transform.localScale = Vector3.one;

            //Lets do a bit of vetting and see if we need to put a joiner in place
            LineHandler newHandler = newline.GetComponent<LineHandler>();

            if (SpawnedLines.Count > 1)
            {
                //Get our prior line
                LineHandler.enLineType priorLineType = SpawnedLines[SpawnedLines.Count - 1].LineType;

                switch (newHandler.LineType)
                {
                    case LineHandler.enLineType.ROAD:
                        newHandler.SetJoiningDetail(priorLineType == LineHandler.enLineType.ROAD);
                        break;
                    default:
                        break;

                }
            }
            else
            {
                newHandler.SetJoiningDetail(false); //So that our road edge isn't displayed if it's the frist on the map
            }

            SpawnedLines.Add(newHandler);
            lines.Add(z, newline);
        }
    }
	
    public void Redundant_Update() {
        // Generate lines based on player position.
        // PROBLEM: Really this only needs to be called when the player moves
        int playerZ = (int)player.transform.position.z;
        for (int z = Mathf.Max(minZ, playerZ - lineBehind); z <= playerZ + lineAhead; z += 1) {
            if (!lines.ContainsKey(z)) {
                GameObject coin;
                int x = Random.Range(0, 2);
                if (x == 1) {
                    coin = (GameObject)Instantiate(coins);
                    int randX = Random.Range(-4, 4);
                    coin.transform.position = new Vector3(randX, 1, 1.5f);
                }

                GameObject line = (GameObject)Instantiate(
                    linePrefabs[Random.Range(0, linePrefabs.Length)],
                    new Vector3(0, 0, z * 3 - 5), 
                    Quaternion.identity
                );

                line.transform.localScale = new Vector3(1, 1, 1);
                lines.Add(z, line);
            }
        }

        List<int> LinesToRemove = new List<int>();

        foreach (int line in lines.Keys)
        {
            if (line < playerZ - lineBehind)
            {
                LinesToRemove.Add(line);
                Debug.Log("Need to remove line: " + line);
            }
        }

        foreach(int RemoveLine in LinesToRemove)
        {
            if (lines.ContainsKey(RemoveLine))
            {
                Debug.Log("Removing Line: " + RemoveLine);
                GameObject lineObject = lines[RemoveLine];
                lines.Remove(RemoveLine);   //Remove from our dictionary
                Destroy(lineObject);        //Destroy this object for quality control
            }
        }
    }
}
