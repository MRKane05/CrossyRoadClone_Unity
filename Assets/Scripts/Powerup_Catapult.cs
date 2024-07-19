using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup_Catapult : Powerup {
    public GameObject ArmDown, ArmUp;

    public override void OnEquip(GameObject playerObject)
    {
        if (bMounted) { return; }
        startTime = Time.time;
        gameObject.GetComponent<Collider>().enabled = false; //turn off our collider
        PlayerMovementScript playerMove = playerObject.GetComponent<PlayerMovementScript>();
        //Debug.Log("TossStart: " + transform.position);
        playerMove.TossCharacter(new Vector3(transform.position.x, 0, transform.position.z), 3*5);
        ArmDown.SetActive(false);
        ArmUp.SetActive(true);

        if (ourAudio && Sound_OnActivate)
        {
            ourAudio.clip = Sound_OnActivate;
            ourAudio.Play();
        }
        bMounted = true;
    }

    public void Update()
    {
        //Our on enable should have set our start time
        if (Time.time > startTime + lifeSpan && bMounted)
        {
            RemovePowerup();
        }
    }
}
