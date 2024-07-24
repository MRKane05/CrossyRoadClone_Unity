﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;
using DG.Tweening;

public class Powerup_Ghost : Powerup {
    float SlowTimeSpeed = 0.5f;
    public GameObject ourPlayer;

    public override void OnEquip(GameObject playerObject)
    {
        base.OnEquip(playerObject);
        ourPlayer = playerObject;
        Time.timeScale = SlowTimeSpeed;
        LevelControllerScript.Instance.player.GetComponent<PlayerMovementScript>().setTimeScale(SlowTimeSpeed);
        ourPlayer.GetComponent<PlayerMovementScript>().SetGhost(true);
    }

    public virtual bool GameOver(GameObject PlayerCharacter, enDieType DieType)
    {
        RemovePowerup();
        return true;    //Successfully avoided death
    }

    public virtual void RemovePowerup()
    {
        transform.DOShakeScale(0.5f).SetUpdate(true).OnComplete(() => { Destroy(gameObject); });
        Time.timeScale = 1f;
        LevelControllerScript.Instance.player.GetComponent<PlayerMovementScript>().setTimeScale(1f);
        ourPlayer.GetComponent<PlayerMovementScript>().SetGhost(false);
    }

    public void Update()
    {
        //reveal our indicator
        if (Indicator_CanvasGroup && bMounted)
        {
            Indicator_CanvasGroup.alpha = Mathf.Lerp(Indicator_CanvasGroup.alpha, 1f, Time.unscaledDeltaTime * 5f);
            Indicator_CanvasGroup.transform.eulerAngles = new Vector3(90, 0, 0);    //So that we won't face the player direction (nasty hack!)
        }
        //empty our indicator
        if (Indicator_FillImage && bMounted)
        {
            Indicator_FillImage.fillAmount = 1f - Mathf.Clamp01((Time.time - startTime) / lifeSpan);
        }
        if (PowerupMarker)
        {
            PowerupMarker.transform.localEulerAngles += Vector3.up * -120f * Time.unscaledDeltaTime;
        }

        //Our on enable should have set our start time
        if (Time.time > startTime + lifeSpan && bMounted)
        {
            RemovePowerup();
        }
    }
}
