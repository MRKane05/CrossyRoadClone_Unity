using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
public class CharacterSaveInformation
{
	public string CharacterName = "";
	public bool Unlocked = false; 

	public CharacterSaveInformation(string newCharacterName, bool newUnlocked)
    {
		CharacterName = newCharacterName;
		Unlocked = newUnlocked;
    }
}

[System.Serializable]
public class PowerupSaveInformation
{
	public string PowerupName = "";
	public int PowerupCount = 0;

	public PowerupSaveInformation(string newPowerupName, int newPowerupCount)
    {
		PowerupName = newPowerupName;
		PowerupCount = newPowerupCount;
    }
}

[System.Serializable]
public class SaveGameInformation
{
	public List<CharacterSaveInformation> CharacterSaves = new List<CharacterSaveInformation>();
	public List<PowerupSaveInformation> PowerupSaves = new List<PowerupSaveInformation>();
	public int Coins = 0;
	public int TopScore = 0;
}


//A basic utility class used to save/load game data (that's not playerprefs)
public class SaveScoreUtility : MonoBehaviour
{
	public static SaveScoreUtility Instance { get; private set; }

	public SaveGameInformation GameSaveInformation; // LevelScoreInformation;
	//public StatsSaveInformation GameStatistics;

	//GameStateControllerScript ourGameController;   //We'll be sitting on our game controller
	string path = "ux0:data/CrossyRoadClone";

	void Awake()
    {
		//Put this here so it can be ahead of the load call
#if UNITY_EDITOR
		path = Application.persistentDataPath;
		Debug.Log("Persistent Path: " + path);
#endif

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

	public void Start()
	{
		//ourGameController = gameObject.GetComponent<GameController>();

		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}

		//populateSaveDataFromController();
		//SaveGameData();	//Do a test save
		LoadSaveData();
		PopulateGameStateControllerFromSave();
	}

	//Used for when we've had a character change and want to save the information back to our file
	public void SaveGameInformation()
    {
		populateSaveDataFromController();
		SaveGameData();
	}

	public void populateSaveDataFromController()
    {
		//SaveCharacterInformation newSaveCharacter = new SaveCharacterInformation(true);
		GameSaveInformation = new SaveGameInformation();

		GameSaveInformation.Coins = GameStateControllerScript.Instance.coins;
		//GameStateControllerScript.Instance.setCoinTotal(GameSaveInformation.Coins);
		GameSaveInformation.TopScore = GameStateControllerScript.Instance.score_top;

		foreach (CharacterGroup characterGroup in GameStateControllerScript.Instance.CharacterGroups)
		{
			foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
			{
				bool bHasBeenAdded = false;
				foreach (CharacterSaveInformation Character in GameSaveInformation.CharacterSaves)
				{
					if (Character.CharacterName == thisCharacter.CharacterName)
					{
						bHasBeenAdded = true;
						//We need to set the values of this character to the ones in the information from our GameStateController
						Character.Unlocked = thisCharacter.Unlocked;
					}
				}
				if (!bHasBeenAdded) //if we haven't added this to our list then do so
				{
					CharacterSaveInformation NewCharacter = new CharacterSaveInformation(thisCharacter.CharacterName, thisCharacter.Unlocked);
					GameSaveInformation.CharacterSaves.Add(NewCharacter);
				}
			}
		}

		foreach (Powerup_Item thisPowerup in GameStateControllerScript.Instance.PowerupItems)
        {
			bool bHasBeenAdded = false;
			foreach (PowerupSaveInformation savedPowerup in GameSaveInformation.PowerupSaves)
			{
				if (savedPowerup.PowerupName == thisPowerup.PowerupName)
				{
					bHasBeenAdded = true;
					//We need to set the values of this character to the ones in the information from our GameStateController
					//Character.Unlocked = thisCharacter.Unlocked;
					savedPowerup.PowerupCount = thisPowerup.count;
				}
			}
			if (!bHasBeenAdded) //if we haven't added this to our list then do so
			{
				//CharacterSaveInformation NewCharacter = new CharacterSaveInformation(thisCharacter.CharacterName, thisCharacter.Unlocked);
				PowerupSaveInformation newPowerup = new PowerupSaveInformation(thisPowerup.PowerupName, thisPowerup.count);
				GameSaveInformation.PowerupSaves.Add(newPowerup);
			}
		}
	}

	public bool CheckSaveFile(string FileName)
	{
		//Check and see if we've got a saved map state
		if (!System.IO.File.Exists(path + "/" + FileName))
		{
			Debug.Log("Save File Not Found");
			Debug.Log(path + "/" + FileName);
			return false;
		}
		return true;
	}

	public void SaveGameData()
	{
		string SaveGameState = JsonUtility.ToJson(GameSaveInformation);

		StreamWriter writer = new StreamWriter(path + "/CrossySave.json");
		Debug.Log("Save Path: " + path + "/CrossySave.json");
		writer.AutoFlush = true;
		writer.Write(SaveGameState);
		writer.Close();
	}

	public bool LoadSaveData()
	{
		if (CheckSaveFile("CrossySave.json"))
		{
			//Debug.Log("DataPath: " + Application.persistentDataPath + "/VHScores.json");
			string fileData = File.ReadAllText(path + "/CrossySave.json");
			SaveGameInformation ourSaveForm = JsonUtility.FromJson<SaveGameInformation>(fileData);
			GameSaveInformation = ourSaveForm;	//Set our information. We'll then go through and assign to our GameState

			return true;
		}
		else
		{
			return false;
		}
		return false;
	}

	public void PopulateGameStateControllerFromSave()
    {
		GameStateControllerScript.Instance.setCoinTotal(GameSaveInformation.Coins);
		GameStateControllerScript.Instance.SetTopScore(GameSaveInformation.TopScore);

		Debug.Log("Populating Saves");
		foreach (CharacterGroup characterGroup in GameStateControllerScript.Instance.CharacterGroups)
		{
			foreach (SelectableCharacter thisCharacter in characterGroup.GroupCharacters)
			{
				bool bHasBeenAdded = false;
				foreach (CharacterSaveInformation Character in GameSaveInformation.CharacterSaves)
				{
					if (Character.CharacterName == thisCharacter.CharacterName)
					{
						bHasBeenAdded = true;
						//We need to set the values of this character to the ones in the information from our GameStateController
						thisCharacter.Unlocked = Character.Unlocked;	//Essentilaly just a reverse of prior logic. Don't mix these up...
					}
				}
				if (!bHasBeenAdded) //if we haven't added this to our list then do so
				{
					Debug.Log("Character Not In Save: " + thisCharacter.CharacterName);
				}
			}
		}

		foreach (PowerupSaveInformation savedPowerup in GameSaveInformation.PowerupSaves)
		{			
			bool bHasBeenAdded = false;
			foreach (Powerup_Item thisPowerup in GameStateControllerScript.Instance.PowerupItems)
			{
				if (savedPowerup.PowerupName == thisPowerup.PowerupName)
				{
					bHasBeenAdded = true;
					//We need to set the values of this character to the ones in the information from our GameStateController
					//Character.Unlocked = thisCharacter.Unlocked;
					//savedPowerup.PowerupCount = thisPowerup.count;
					thisPowerup.count = savedPowerup.PowerupCount;
				}
			}
			if (!bHasBeenAdded) //if we haven't added this to our list then do so
			{
				//CharacterSaveInformation NewCharacter = new CharacterSaveInformation(thisCharacter.CharacterName, thisCharacter.Unlocked);
				//PowerupSaveInformation newPowerup = new PowerupSaveInformation(thisPowerup.PowerupName, thisPowerup.count);
				//GameSaveInformation.PowerupSaves.Add(newPowerup);
				//Debug.Log("Powerup Not In Save: " + thisPowerup.PowerupName);
				Powerup_Item newPowerupItem = new Powerup_Item(savedPowerup.PowerupName, savedPowerup.PowerupCount);
				GameStateControllerScript.Instance.PowerupItems.Add(newPowerupItem);
			}
		}
	}
}
