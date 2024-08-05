﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerupHandler : MonoBehaviour {
	public static PowerupHandler Instance { get; private set; }

    public List<Powerup_Item> PowerupItems = new List<Powerup_Item>();

    public GameObject powerupDisplayArea, selectedPowerupsBase;

    public GameObject powerupItem, equippedPowerupItem, EquippedItemMaxMessage;

    public List<string> selectedPowerups = new List<string>();

    [Space]
    [Header("Purchase Details")]
    public GameObject purchaseMenu;


    public Sprite powerupSprite(string powerupName)
    {
        foreach(Powerup_Item thisPowerup in PowerupItems)
        {
            if (thisPowerup.PowerupName == powerupName)
            {
                return thisPowerup.powerupSprite;
            }
        }
        return null;
    }

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
        PopulatePowerupsLists();
        ScrubEquippedList();
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
        foreach(Powerup_Item powerup in PowerupItems)
        {
            GameObject newPowerupButton = Instantiate(powerupItem, powerupDisplayArea.transform);
            PowerupMenuItem powerupButton = newPowerupButton.GetComponent<PowerupMenuItem>();
            //We need to get our count information from the GameStateController
            int count = 0;
            foreach (Powerup_Item thisPowerup in GameStateControllerScript.Instance.PowerupItems)
            {
                if (powerup.PowerupName == thisPowerup.PowerupName)
                {
                    count = thisPowerup.count;
                }
            }

            powerupButton.setupButton(powerup.PowerupName, count, powerup.powerupCost, powerup.powerupPrefab);
        }
    }

    public bool AddPowerupToStack(string thisPowerup, PowerupMenuItem caller)
    {
        int countAlready = selectedPowerupsBase.transform.childCount;
        if (countAlready >= 5)
        {
            EquippedItemMaxMessage.SetActive(true);
            return false;
        }

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
                //And logically
                EquippedItemMaxMessage.SetActive(false); //Clear our "too many items" message if we've dropped this
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

    public void BuyPowerup(string thisPowerup, PowerupMenuItem powerupButton)
    {
        foreach (Powerup_Item powerup in PowerupItems)
        {
            if (powerup.PowerupName == thisPowerup)
            {
                purchaseMenu.SetActive(true);
                UI_PowerupPurchaseHandler purchaseHandler = purchaseMenu.GetComponent<UI_PowerupPurchaseHandler>();
                purchaseHandler.setPowerup(powerup, powerupButton);
            }
        }
    }
}
