using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PowerupEquippedItem : MonoBehaviour {
    public TextMeshProUGUI itemTitle;
    public string itemName = "";

    public void setItem(string newItemName)
    {
        itemName = newItemName;
        itemTitle.text = itemName;
    }

    public void RemoveItemFromList()
    {
        //We've got to somehow set our counts correct, and then remove ourselves from the list
        PowerupHandler.Instance.RemovePowerupFromStack(itemName);
        Destroy(gameObject);
    }
}
