using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;
using DG.Tweening;

public class Powerup_HampsterBall : Powerup {
    float pulseTime = 0;

    public virtual void OnEquip()
    {
        startTime = Time.time;
        pulseTime = Time.time;
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
        bool bEscaped = playerMove.Move(new Vector3(0, 0, -3));
        RemovePowerup();
        if (ourAudio)
        {
            ourAudio.Play();
        }
        return bEscaped;
    }

    public void Update()
    {
        if (Time.time - pulseTime > 1 && bMounted)
        {
            pulseTime = Time.time;
            transform.DOShakeScale(0.5f).SetUpdate(true);
        }

        //Our on enable should have set our start time
        if (Time.time > startTime + lifeSpan && bMounted)
        {
            RemovePowerup();
        }
    }
}
