using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupEquippedItem : MonoBehaviour {
    public TextMeshProUGUI itemTitle;
    public Image ButtonIcon;
    public string itemName = "";

    public void setItem(string newItemName)
    {
        itemName = newItemName;
        //itemTitle.text = itemName;
        ButtonIcon.sprite = PowerupHandler.Instance.powerupSprite(newItemName);
    }

    public void RemoveItemFromList()
    {
        //We've got to somehow set our counts correct, and then remove ourselves from the list
        PowerupHandler.Instance.RemovePowerupFromStack(itemName);
        Destroy(gameObject);
    }
}
