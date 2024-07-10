using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelControllerScript : MonoBehaviour {

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

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // This will make sure the instance is not destroyed between scenes
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

        // Remove lines based on player position.
        /*
        foreach (GameObject line in new List<GameObject>(lines.Values)) {
            int lineZ = (int)line.transform.position.z;
            if (lineZ < playerZ - lineBehind) {
                LinesToRemove.Add(lineZ);
            }
        }*/
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
