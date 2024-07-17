using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupHandler : MonoBehaviour {
	public static PowerupHandler Instance { get; private set; }

    public GameObject powerupDisplayArea, selectedPowerupsBase;

    public GameObject powerupItem, equippedPowerupItem;

    public List<string> selectedPowerups = new List<string>();

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensures there is only one instance
        }

        PopulatePowerupsLists();
        ScrubEquippedList();
    }

    public void OpenMenu()
    {

    }

    public void ScrubEquippedList()
    {
        foreach (Transform child in selectedPowerupsBase.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void PopulatePowerupsLists()
    {
        foreach(Transform child in powerupDisplayArea.transform)
        {
            Destroy(child.gameObject);
        }

        //Now go through and assign our powerup buttons
        foreach(Powerup_Item powerup in GameStateControllerScript.Instance.PowerupItems)
        {
            GameObject newPowerupButton = Instantiate(powerupItem, powerupDisplayArea.transform);
            PowerupMenuItem powerupButton = newPowerupButton.GetComponent<PowerupMenuItem>();
            powerupButton.setupButton(powerup.PowerupName, powerup.count, powerup.powerupPrefab);
        }
    }

    public bool AddPowerupToStack(string thisPowerup, PowerupMenuItem caller)
    {
        GameObject newEquippedButton = Instantiate(equippedPowerupItem, selectedPowerupsBase.transform);
        PowerupEquippedItem thisScript = newEquippedButton.GetComponent<PowerupEquippedItem>();
        thisScript.setItem(thisPowerup);

        return true;
        //We need to see if there's room to add another powerup
        //We need to change the count of our powerups on our display button
    }

    //This is mostly a bookkeeping function
    public void RemovePowerupFromStack(string thisPowerup)
    {
        foreach (Transform child in powerupDisplayArea.transform)
        {
            PowerupMenuItem powerupItem = child.gameObject.GetComponent<PowerupMenuItem>();
            if (powerupItem.itemName == thisPowerup)
            {
                powerupItem.returnPowerup();
            }
        }
    }

    public List<string> getEquippedPowerups()
    {
        List<string> equipped = new List<string>();
        foreach (Transform child in selectedPowerupsBase.transform)
        {
            PowerupEquippedItem thisEquipped = child.gameObject.GetComponent<PowerupEquippedItem>();
            equipped.Add(thisEquipped.itemName);
        }
        return equipped;
    }
}
