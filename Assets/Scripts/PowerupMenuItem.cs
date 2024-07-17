using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerupMenuItem : MonoBehaviour {
    public TextMeshProUGUI ItemTitle;
    public GameObject itemDisplayPosition;
    [HideInInspector]
    public string itemName = "";
    public int itemCount = 0;

    public void returnPowerup()
    {
        itemCount++;
        setButtonText(itemName, itemCount);
    }

    public void setupButton(string newTitle, int newItemCount, GameObject newDisplayModel)
    {
        itemName = newTitle;
        itemCount = newItemCount;
        setButtonText(newTitle, itemCount);
        if (newDisplayModel)
        {
            GameObject newModel = Instantiate(newDisplayModel, itemDisplayPosition.transform);
            //newModel.transform.localScale = Vector3.one;
            newModel.transform.localEulerAngles = Vector3.zero;
            //newModel.transform.localPosition = Vector3.zero;
        }
    }

    //Because we'll be changing the count attached to this
    public void setButtonText(string newTitle, int itemCount)
    {
        ItemTitle.text = newTitle + ": " + itemCount.ToString();
    }

    public void AddPowerupToStack()
    {
        if (itemCount <=0) {
            Debug.Log("bring up buy powerup menu");
            return; 
        }  //Don't add if we've got too few

        Debug.Log("Adding Powerup");
        bool bCanAdd = PowerupHandler.Instance.AddPowerupToStack(itemName, this);
        if (bCanAdd)
        {
            itemCount -= 1;
            //Update our title
            setButtonText(itemName, itemCount);
        }
    }
}
