using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;
using DG.Tweening;

public class Powerup_HampsterBall : Powerup {
    float pulseTime = 0;
    bool bPlayedFinishSound = false;

    public AudioClip powerupFinishClip;

    public virtual void OnEquip()
    {
        startTime = Time.time;
        pulseTime = Time.time;

        if (ourAudio && Sound_OnPickup)
        {
            ourAudio.clip = Sound_OnPickup;
            ourAudio.Play();
        }
    }

    public override bool GameOver(GameObject PlayerCharacter, enDieType DieType)
    {
        if (DieType == enDieType.EAGLE) //Can't protect against the eagle
        {
            return false;   //We didn't have an effective escape
        }
        PlayerMovementScript playerMove = PlayerCharacter.GetComponent<PlayerMovementScript>();
        playerMove.killMoveTween(); //Stop our movement
        //Technically we could just call a backward move
        bool bEscaped = playerMove.DoMove(new Vector3(0, 0, -3));
        RemovePowerup();

        if (ourAudio && Sound_OnActivate)
        {
            ourAudio.clip = Sound_OnActivate;
            ourAudio.Play();
        }
        return bEscaped;
    }

    public void Update()
    {
        if (Time.unscaledTime - pulseTime > 1)
        {
            pulseTime = Time.unscaledTime;
            transform.DOShakeScale(0.5f).SetUpdate(true);
        }

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

        if (powerupFinishClip)
        {
            if (Time.time > startTime + lifeSpan - powerupFinishClip.length && !bPlayedFinishSound && bMounted)
            {
                bPlayedFinishSound = true;
                ourAudio.clip = powerupFinishClip;
                ourAudio.Play();
            }
        }

        //Our on enable should have set our start time
        if (Time.time > startTime + lifeSpan && bMounted)
        {
            RemovePowerup();
        }
    }
}
