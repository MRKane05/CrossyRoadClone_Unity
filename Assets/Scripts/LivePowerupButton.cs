﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class LivePowerupButton : MonoBehaviour {
    public Image ButtonIcon;
    public GameObject powerupPrefab;
    public void setupButton(GameObject newPowerupPrefab, Sprite powerupSprite)
    {
        powerupPrefab = newPowerupPrefab;
        //we need to get the sprite for this item. Yay.
        ButtonIcon.sprite = powerupSprite;
    }

    public void activateButton()
    {
        //Basically we want this to send a call through to the level controller to drop our powerup somewhere and have it displayed where it's gone
        bool bPowerupAdded = LevelControllerScript.Instance.addPowerupToLevel(powerupPrefab);
        //We need to remove our button as it's been successful :)
        if (bPowerupAdded)
        {
            transform.DOShakeScale(0.75f).OnComplete(() => { Destroy(gameObject); });
        } else
        {
            transform.DOShakePosition(0.75f);
        }
    }
}
