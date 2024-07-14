using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoneyBehaviour : MonoBehaviour {
    void OnTriggerEnter(Collider other)
    {
        // When collide with player, flatten it!
        if (other.gameObject.tag == "Player")
        {
            //We need to collect this, play some effect/sound and then add the score to our total. Lets start simple.
            GameStateControllerScript.Instance.ChangeCoinTotal(1);
            Destroy(gameObject);    //Remove ourself
        }
    }
}
