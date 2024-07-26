using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PowerupMenuItem : MonoBehaviour {
    public TextMeshProUGUI ItemTitle;
    public GameObject itemDisplayPosition;
    [HideInInspector]
    public string itemName = "";
    public int itemCount = 0;
    public int itemCost = 150;

    public Button purchaseButton;

    public void returnPowerup()
    {
        itemCount++;
        setButtonText(itemName, itemCount);
    }

    public void setupButton(string newTitle, int newItemCount, int newPowerupCost, GameObject newDisplayModel)
    {
        itemName = newTitle;
        itemCount = newItemCount;
        itemCost = newPowerupCost;
        setButtonText(newTitle, itemCount);
        if (newDisplayModel)
        {
            GameObject newModel = Instantiate(newDisplayModel, itemDisplayPosition.transform);
            newModel.transform.localPosition = Vector3.one;
            //newModel.transform.localScale = Vector3.one;
            newModel.transform.localEulerAngles = Vector3.zero;
            //newModel.transform.localPosition = Vector3.zero;
        }

        purchaseButton.interactable = GameStateControllerScript.Instance.coins > itemCost;
    }

    //Because we'll be changing the count attached to this
    public void setButtonText(string newTitle, int itemCount)
    {
        ItemTitle.text = newTitle + ": " + itemCount.ToString();
    }

    public void AddPowerupToStack()
    {
        purchaseButton.interactable = GameStateControllerScript.Instance.coins > itemCost;
        if (itemCount <=0) {
            if (GameStateControllerScript.Instance.coins > itemCost)
            {
                purchaseButton.transform.DOShakeScale(0.75f).SetUpdate(true).OnComplete(() => { purchaseButton.transform.localScale = Vector3.one; });
            }
            return; 
        }  //Don't add if we've got too few

        Debug.Log("Adding Powerup");
        bool bCanAdd = PowerupHandler.Instance.AddPowerupToStack(itemName, this);
        if (bCanAdd)
        {
            itemCount -= 1;
            //Update our title
            setButtonText(itemName, itemCount);
        } else
        {
            //We need to display a message saying "limit reached"
        }
    }

    public void buyPowerup()
    {
        PowerupHandler.Instance.BuyPowerup(itemName, this);
    }

    public void oldBuyPowerup() { 
        //We need to check and see if we've got enough money, probably doesn't matter where this action happens...
        if (GameStateControllerScript.Instance.coins < itemCost)
        {
            purchaseButton.interactable = false;
            return;
        }

        //otherwise...
        itemCount++;
        setButtonText(itemName, itemCount);

        //Update our GameStateController about it...
        GameStateControllerScript.Instance.ChangeCoinTotal(-itemCost);
        GameStateControllerScript.Instance.ChangePowerupCount(itemName, 1);
    }

    public void ChangeCount(int changeAmount)
    {
        itemCount += changeAmount;
        setButtonText(itemName, itemCount);
    }
}
