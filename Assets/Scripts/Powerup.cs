using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerMovementScript;
using DG.Tweening;
using UnityEngine.UI;

public class Powerup : MonoBehaviour
{
    public enum enPowerupType { NULL, ONHIT, COINS, FUNCTION }
    public enPowerupType PowerupType = enPowerupType.NULL;
    //public void 
    public enum enPowerupLife { NULL, TIMED, RESIDENT }
    public enPowerupLife PowerupLife = enPowerupLife.NULL;

    public float lifeSpan = 7;
    protected float startTime = 0;
    public CanvasGroup Indicator_CanvasGroup;
    public Image Indicator_FillImage;
    public GameObject PowerupMarker;


    public Vector3 mountOffset = new Vector3(0, 1.6f, 0);

    public bool bMounted = false;

    protected AudioSource ourAudio;

    public AudioClip Sound_OnPickup, Sound_OnActivate;

    public void Awake()
    {
        ourAudio = gameObject.GetComponent<AudioSource>();

        if (Indicator_CanvasGroup)
        {
            Indicator_CanvasGroup.alpha = 0;    //Make our indicator hidden
        }
    }

    public virtual void OnEquip(GameObject playerObject)
    {
        startTime = Time.time;
        gameObject.GetComponent<Collider>().enabled = false; //turn off our collider
        PlayerMovementScript playerMove = playerObject.GetComponent<PlayerMovementScript>();

        if (playerMove.EquippedPowerup) //if we have a powerup we need to kill it before applying this one
        {
            playerMove.EquippedPowerup.GetComponent<Powerup>().RemovePowerup();
        }

        playerMove.EquippedPowerup = gameObject;
        gameObject.transform.SetParent(playerMove.CharacterBase.transform);
        gameObject.transform.localPosition = mountOffset;
        bMounted = true;

        if (ourAudio && Sound_OnPickup)
        {
            ourAudio.clip = Sound_OnPickup;
            ourAudio.Play();
        }
    }

    public virtual bool GameOver(GameObject PlayerCharacter, enDieType DieType)
    {
        return true;    //Successfully avoided death
    }

    public virtual void RemovePowerup()
    {
        if (PowerupMarker)
        {
            PowerupMarker.transform.DOShakeScale(0.5f).SetUpdate(true).OnComplete(() => { Destroy(gameObject); });
        } else
        {
            transform.DOShakeScale(0.5f).SetUpdate(true).OnComplete(() => { Destroy(gameObject); });
        }

    }

    void OnTriggerEnter(Collider other)
    {
        // When collide with player, flatten it!
        if (other.gameObject.tag == "Player")
        {
            OnEquip(other.gameObject);
        }
    }
}
