using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_PowerupPurchaseHandler : MonoBehaviour {
    public TextMeshProUGUI title, description, displayCost, displayNumber;
    public Button purchaseButton, purchaseMoreButton;


    public int purchaseNumber = 1;

    Powerup_Item currentPowerup;
    PowerupMenuItem powerupButton;

    public void setPowerup(Powerup_Item thisPowerup, PowerupMenuItem newPowerupButton)
    {
        currentPowerup = thisPowerup;
        title.text = "buy " + thisPowerup.PowerupName;
        description.text = thisPowerup.PowerupDescription;
        powerupButton = newPowerupButton;

        purchaseNumber = 1;

        setDisplayCost();
    }

    public void setDisplayCost()
    {
        displayCost.text = "buy " + purchaseNumber.ToString() + " for " + (currentPowerup.powerupCost * purchaseNumber).ToString() + "c";
        displayNumber.text = "x" + purchaseNumber.ToString();
    }

    public void purchaseChange(int shift)
    {
        purchaseNumber = Mathf.Clamp(purchaseNumber + shift, 1, Mathf.FloorToInt((float)GameStateControllerScript.Instance.coins / (float)currentPowerup.powerupCost));
        setDisplayCost();
    }

    public void doPurchase()
    {
        GameStateControllerScript.Instance.ChangePowerupCount(currentPowerup.PowerupName, purchaseNumber);
        GameStateControllerScript.Instance.ChangeCoinTotal(-purchaseNumber * currentPowerup.powerupCost);
        //This needs to reflect in our powerups too...
        //PowerupHandler.Instance.PopulatePowerupsLists();    //This won't account for powerups that have been added to the main list...
        powerupButton.ChangeCount(purchaseNumber);
    }
}
