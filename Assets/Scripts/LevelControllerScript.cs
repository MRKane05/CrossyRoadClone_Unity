using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelControllerScript : MonoBehaviour {
    public LayerMask stopperLayerMask;
    public enum enGameState { NULL, STARTSCREEN, PAUSED, PLAYING, GAMEOVER}
    public enGameState GameState = enGameState.STARTSCREEN;

    public static LevelControllerScript Instance { get; private set; }

    public int minZ = 3;
    public int lineAhead = 40;
    public int lineBehind = 20;

    public GameObject[] linePrefabs;
    public GameObject coins;

    public Dictionary<int, GameObject> lines;

    public GameObject player;
    public GameObject camera;

    private Vector3 playerPosition = Vector3.zero;
    private float playerHighest = 0;
    private int playerHighestRow = 0;

    public List<LineHandler> SpawnedLines = new List<LineHandler>();
    [Space]
    [Header("Powerup settings")]
    int currentPowerup = 0;
    public List<string> equippedPowerups = new List<string>();
    public int minPowerupSpacing = 7; //This is the distance we can be tossed by the catapult. The rest of the powerups will be spaced according to top score
    int nextPowerupLine = 0;
    int maxPowerupSpread = 0;

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
        equippedPowerups = newPowerups;
    }

    public void Play()
    {
        currentPowerup = 0;
        nextPowerupLine = 0;

        if (equippedPowerups.Count > 0)
        {
            maxPowerupSpread = Mathf.FloorToInt((float)GameStateControllerScript.Instance.score_top / equippedPowerups.Count);
        }
        //Unlock and set everything in action :)
        player.GetComponent<PlayerMovementScript>().canMove = true;
        camera.GetComponent<CameraMovementScript>().moving = true;

        //We need to add powerups until our visual lines are exhaused here, and then keep adding them as new lines are made
        while (nextPowerupLine < SpawnedLines.Count && currentPowerup < equippedPowerups.Count)
        {
            int powerupRandom = Mathf.Max(minPowerupSpacing, Random.RandomRange(minPowerupSpacing, maxPowerupSpread));
            nextPowerupLine += powerupRandom;
            Debug.Log("NextPowerupLine: " + nextPowerupLine);
            TryToAddPowerup(nextPowerupLine);
        }
    }

    int TryToAddPowerup(int lineNumber)
    {
        if (SpawnedLines.Count < lineNumber)
        {
            return 0; //This'll get picked up later
        }
        //I want to put a cycle in to make sure we don't go adding a powerup to a water line as it's a bit unfair for the player (although fun maybe?)
        LineHandler targetLine = SpawnedLines[lineNumber];

        /*
        if (targetLine.LineType == LineHandler.enLineType.WATER)
        {
            return 0;
        }
        return 1;
        */
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
                    position = new Vector3(Random.RandomRange(-9.5f, 9.5f), 0.77f, 0);  //We'll have to have some height stuff sorted out here
                    if (!Physics.CheckSphere(position, 0.1f, stopperLayerMask))
                    {
                        bCleared = true;
                    }
                    cycles--;
                }
                GameObject newCoin = Instantiate(thisPowerup.powerupPrefab, targetLine.gameObject.transform);
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
        for (int i=1; i<lineAhead+1; i++)
        {
            AddNewMapLine(i * 3);
        }
    }

    public void PlayerMoved()
    {
        if (player.transform.position.z > playerHighest) {  //we've moved forward\
            playerHighest = player.transform.position.z;
            playerHighestRow = Mathf.FloorToInt(playerHighest / 3f);

            AddNewMapLine((playerHighestRow + lineAhead) * 3);
        }
    }

    public void addPowerupToLine(GameObject thisLine)
    {

    }

    public void AddNewMapLine(float newPosition)
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
        } else
        {
            newHandler.SetJoiningDetail(false); //So that our road edge isn't displayed if it's the frist on the map
        }

        SpawnedLines.Add(newHandler);
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
