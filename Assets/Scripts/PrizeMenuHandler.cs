using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrizeMenuHandler : MonoBehaviour {
    public List<SelectableCharacter> CharacterList = new List<SelectableCharacter>();


    public void PlayPrizeMachine()
    {
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
        Debug.Log(newPrize.CharacterName);
    }
}
