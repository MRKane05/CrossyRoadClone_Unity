using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;
using DG.Tweening;

public class Powerup_SlowTime : Powerup {
    float SlowTimeSpeed = 0.5f;

    public override void OnEquip(GameObject playerObject)
    {
        base.OnEquip(playerObject);
        Time.timeScale = SlowTimeSpeed;
        LevelControllerScript.Instance.player.GetComponent<PlayerMovementScript>().setTimeScale(SlowTimeSpeed);
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
    }

    public void Update()
    {
        /*
        if (Time.time - pulseTime > 1 && bMounted)
        {
            pulseTime = Time.time;
            transform.DOShakeScale(0.5f).SetUpdate(true);
        }
        */

        //Our on enable should have set our start time
        if (Time.time > startTime + lifeSpan && bMounted)
        {
            RemovePowerup();
        }
    }
}
