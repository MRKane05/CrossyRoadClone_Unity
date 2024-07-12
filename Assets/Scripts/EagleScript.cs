using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

public class EagleScript : MonoBehaviour {
    public float speedX = 1.0f;
    float passTime = 1f;    //How quickly do we move when we're doing our lerp?
    public GameObject[] EagleObjects;
    bool EagleActive = false;

    public void SetEagleMovement(Vector3 StartPosition, Vector3 EndPosition)
    {
        if (EagleActive) { return; }

        setEagleVisibility(true);
        gameObject.transform.position = StartPosition;
        gameObject.transform.DOMove(EndPosition, passTime).SetEase(Ease.Linear).OnComplete(() => setEagleVisibility(false));

        gameObject.GetComponent<AudioSource>().Play();  //Play our screech
    }

    void setEagleVisibility(bool state)
    {
        EagleActive = true;
        for(int i=0; i<EagleObjects.Length; i++)
        {
            EagleObjects[i].SetActive(state);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (!EagleActive) { return; }
        // When collide with player, flatten it!
        if (other.gameObject.tag == "Player") {
            Vector3 scale = other.gameObject.transform.localScale;
            other.gameObject.transform.localScale = new Vector3(scale.x, scale.y * 0.1f, scale.z);
            //other.gameObject.SendMessage("GameOver");   //PROBLEM: Not the best way of doing this
            PlayerMovementScript PlayerMove = other.GetComponent<PlayerMovementScript>();
            if (PlayerMove)
            {
                PlayerMove.GameOver(false, false);
            }
            other.gameObject.SetActive(false); //turn our player off so that it looks like it's been snatched
        }
    }
}
