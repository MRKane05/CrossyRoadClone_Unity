using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/*
[System.Serializable]
public class GameTypeStats
{
	public string GameType = "";
	public int HighestDifficulty = 0;
	public int HighestScore = 0;
	public int ModHighestDifficulty = 0;
	public int ModHighestScore = 0;


	public GameTypeStats(string newGameType, int newDifficulty, int newScore, int newModDifficulty, int newModScore)
    {
		GameType = newGameType;
		HighestDifficulty = newDifficulty;
		HighestScore = newScore;
		ModHighestDifficulty = newModDifficulty;
		ModHighestScore = newModScore;
    }

	public GameTypeStats(string newGameType, int newDifficulty, int newScore, bool bIsAModGame)
	{

		GameType = newGameType;
		if (!bIsAModGame)
		{
			HighestDifficulty = newDifficulty;
			HighestScore = newScore;
		} else
        {
			ModHighestDifficulty = newDifficulty;
			ModHighestScore = newScore;
        }
	}
}

[System.Serializable]
public class SaveLevelInformation
{
	public string LevelName = "";

	public List<GameTypeStats> PlayedGameStats = new List<GameTypeStats>();
	public SaveLevelInformation(bool bPopulate)
	{
		for (int i = 0; i < System.Enum.GetValues(typeof(VitaHotUtilities.enGameType)).Length; i++)
		{
			VitaHotUtilities.enGameType thisGameType = (VitaHotUtilities.enGameType)(i);
			string ModeName = thisGameType.ToString();
			bool bHasBeenAdded = false;
			foreach (GameTypeStats thisStat in PlayedGameStats)
			{
				if (thisStat.GameType == ModeName)
				{
					bHasBeenAdded = true;
				}
			}
			if (!bHasBeenAdded)
			{
				GameTypeStats newGameStat = new GameTypeStats(ModeName, 0, 0, 0, 0);
				PlayedGameStats.Add(newGameStat);
			}
		}
	}
}
*/

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

public class PowerupSaveInformation
{
	public string PowerupName = "";
	public int PowerupCount = 0;
}

[System.Serializable]
public class SaveGameInformation
{
	public List<CharacterSaveInformation> CharacterSaves = new List<CharacterSaveInformation>();
	public List<PowerupSaveInformation> PowerupSaves = new List<PowerupSaveInformation>();
}


/*
[System.Serializable]
public class StatsSaveInformation
{
	public KillsSaves KillSaves = new KillsSaves();	//Our kills statistics
	public List<LevelPlays> PlayedLevels = new List<LevelPlays>();	//Our levels played statistics
}


[System.Serializable]
public class KillsSaves {
	public List<KillStatistic> KillsStatistics = new List<KillStatistic>();
}
*/
/*
[System.Serializable]
public class LevelPlays
{
	public string LevelName = "";
	public int Wins = 0;
	public int Deaths = 0;
	public int NumTimesModded = 0;

	public LevelPlays(string thisLevelName, int numWins, int numDeaths, int numMods)
    {
		LevelName = thisLevelName;
		Wins = numWins;
		Deaths = numDeaths;
		NumTimesModded = numMods;
    }
}
*/

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
		print("Save Path: " + path + "/CrossySave.json");
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
	}

	/*
	public GameTypeStats getStatsForLevel(string LevelName, VitaHotUtilities.enGameType thisGameType)
	{
		string GameTypeString = thisGameType.ToString();
		foreach (SaveLevelInformation thisLevel in LevelScoreInformation.LevelSaveInformation)
		{
			if (thisLevel.LevelName == LevelName) //This is the one we want, baby, oo oo ooo
			{
				foreach (GameTypeStats thisStat in thisLevel.PlayedGameStats)
				{
					if (thisStat.GameType == GameTypeString)
					{
						return thisStat;
					}
				}
			}
		}
		return null;
	}

	public StatsSaveInformation GetGameStatistics()
	{
		return GameStatistics;
	}

	public void DoStatisticsSave(List<KillStatistic> newKills, LevelPlays newLevelPlay)
    {
		//Start by tallying up our kills
		foreach (KillStatistic newGroup in newKills)			
        {
			bool bHasBeenAdded = false;
			//foreach (KillStatistic KillGroup in GameStatistics.KillSaves.KillsStatistics)
			for (int i=0; i<GameStatistics.KillSaves.KillsStatistics.Count; i++)
			{
				if (GameStatistics.KillSaves.KillsStatistics[i].WeaponType== newGroup.WeaponType)
                {
					//Simply add up our kills
					GameStatistics.KillSaves.KillsStatistics[i].Kills += newGroup.Kills;
					GameStatistics.KillSaves.KillsStatistics[i].KilledBy += newGroup.KilledBy;
					bHasBeenAdded = true;
				}
            }
			if (!bHasBeenAdded)
            {
				//We've got a novel weapon and need to add it
				GameStatistics.KillSaves.KillsStatistics.Add(newGroup);
            }
        }
		//And now handle our level plays
		for (int i = 0; i < GameStatistics.PlayedLevels.Count; i++) {
			if (GameStatistics.PlayedLevels[i].LevelName == newLevelPlay.LevelName)
            {
				GameStatistics.PlayedLevels[i].Deaths += newLevelPlay.Deaths;
				GameStatistics.PlayedLevels[i].Wins += newLevelPlay.Wins;
				GameStatistics.PlayedLevels[i].NumTimesModded += newLevelPlay.NumTimesModded;
			}
		}
		//With all of THAT out of the way lets save our statistics :)
		SaveStatsData();
	}

	//public void DoSaveData(string LevelName, GameTypeStats thisInformation)
	public void DoSaveData(string LevelName, VitaHotUtilities.enGameType thisGameType, int newDifficulty, int newScore, bool bIsModGame)
	{
		string GameTypeString = thisGameType.ToString();
		bool bDataFieldPresent = false;
		foreach (SaveLevelInformation thisLevel in LevelScoreInformation.LevelSaveInformation)
		{
			if (thisLevel.LevelName == LevelName) //This is the one we want, baby, oo oo ooo
			{
				foreach (GameTypeStats thisStat in thisLevel.PlayedGameStats)
				{
					if (thisStat.GameType == GameTypeString)
					{
						//Set our data
						Debug.Log("Doing Deep Write. Modstate: " + bIsModGame);
						if (!bIsModGame)
						{
							thisStat.HighestDifficulty = newDifficulty; // thisInformation.HighestDifficulty;
							thisStat.HighestScore = newScore; // thisInformation.HighestScore;
						}
						else
						{
							thisStat.ModHighestDifficulty = newDifficulty;
							thisStat.ModHighestScore = newScore;
						}
						bDataFieldPresent = true;
					}
				}
			}
		}
		if (bDataFieldPresent)
		{
			SaveScoreData();
		}
	}
	*/


	//GameStatistics
	/*
	public bool LoadStatsData(List<LevelListItem> LevelList)
	{
		if (CheckSaveFile("VHStats.json"))
		{
			//Debug.Log("DataPath: " + Application.persistentDataPath + "/VHScores.json");
			string fileData = File.ReadAllText(path + "/VHStats.json");
			StatsSaveInformation ourStatsForm = JsonUtility.FromJson<StatsSaveInformation>(fileData);
			//Now lets use the level list to cross-check our dataset and make sure we add/remove things as necessary
			foreach (LevelListItem thisLevel in LevelList)
			{
				bool bContained = false;
				foreach (LevelPlays thisPlay in ourStatsForm.PlayedLevels)
				{
					if (thisPlay.LevelName == thisLevel.levelname)
					{
						bContained = true;
					}
				}

				if (!bContained)
				{
					//Then we need to add an entry to our list :)
					LevelPlays newLevelSave = new LevelPlays(thisLevel.levelname, 0, 0, 0);
					ourStatsForm.PlayedLevels.Add(newLevelSave);
				}
			}
			GameStatistics = ourStatsForm;
			return true;
		}
		else
		{
			//We need to make a new score list
			MakeNewStatsData(LevelList);
		}
		return false;
	}

	public bool LoadScoreData(List<LevelListItem> LevelList)
	{
		if (CheckSaveFile("VHScores.json"))
		{
			//Debug.Log("DataPath: " + Application.persistentDataPath + "/VHScores.json");
			string fileData = File.ReadAllText(path + "/VHScores.json");
			SaveGameInformation ourSaveForm = JsonUtility.FromJson<SaveGameInformation>(fileData);
			//Now lets use the level list to cross-check our dataset and make sure we add/remove things as necessary
			foreach (LevelListItem thisLevel in LevelList) 
			{
				bool bContained = false;
				foreach (SaveLevelInformation thisSave in ourSaveForm.LevelSaveInformation)
				{
					if (thisSave.LevelName == thisLevel.levelname)
					{
						bContained = true;
						//We should filter through our game modes here too
						for (int i = 0; i < System.Enum.GetValues(typeof(VitaHotUtilities.enGameType)).Length; i++)
						{
							VitaHotUtilities.enGameType thisGameType = (VitaHotUtilities.enGameType)(i);
							string ModeName = thisGameType.ToString();
							bool bModeContained = false;
							foreach (GameTypeStats thisStat in thisSave.PlayedGameStats)
							{
								if (thisStat.GameType == ModeName)
								{
									bModeContained = true;
								}
							}
							//and if we don't have said mode...
							if (!bModeContained)
							{
								GameTypeStats newGameStat = new GameTypeStats(ModeName, 0, 0, 0, 0);
								thisSave.PlayedGameStats.Add(newGameStat);
							}
						}
					}
				}

				if (!bContained)
				{
					//Then we need to add an entry to our list :)
					SaveLevelInformation newLevelSave = new SaveLevelInformation(true);
					newLevelSave.LevelName = thisLevel.levelname;
					ourSaveForm.LevelSaveInformation.Add(newLevelSave);
				}
			}
			LevelScoreInformation = ourSaveForm;
			//Because I've gone and added a game mode we should look through and see if we need to add it to our score information

			return true;
		}
		else
		{
			//We need to make a new score list
			MakeNewScoreData(LevelList);
		}
		return false;
	}

	public void MakeNewStatsData(List<LevelListItem> LevelList)
	{
		//We need to make a new score list
		GameStatistics = new StatsSaveInformation();
		foreach (LevelListItem thisLevel in LevelList)
		{
			LevelPlays newLevelSave = new LevelPlays(thisLevel.levelname, 0,0,0);
			GameStatistics.PlayedLevels.Add(newLevelSave);
		}
	}

	//This simply saves the score, and doesn't do any further processing on the list
	public void SaveScoreData()
	{
		string ScoreSaveState = JsonUtility.ToJson(LevelScoreInformation);
		// System.IO.File.WriteAllText(Application.persistentDataPath + "/VHScores.json", ScoreSaveState);
		// Debug.Log(ScoreSaveState);
		// Debug.Log("MapSaved: " + Application.persistentDataPath + "/VHScores.json");

		StreamWriter writer = new StreamWriter(path + "/VHScores.json");
		writer.AutoFlush = true;
		writer.Write(ScoreSaveState);
		writer.Close();
	}

	public void SaveStatsData()
    {
		string ScoreSaveState = JsonUtility.ToJson(GameStatistics);
		// System.IO.File.WriteAllText(Application.persistentDataPath + "/VHScores.json", ScoreSaveState);
		// Debug.Log(ScoreSaveState);
		// Debug.Log("MapSaved: " + Application.persistentDataPath + "/VHScores.json");

		StreamWriter writer = new StreamWriter(path + "/VHStats.json");
		writer.AutoFlush = true;
		writer.Write(ScoreSaveState);
		writer.Close();
	}
	*/
}
