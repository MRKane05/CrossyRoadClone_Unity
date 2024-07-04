using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script exists simply to have the line delete itself once it's out of the view area
public class LineHandler : MonoBehaviour {
    float clearDistance = 30;

    public enum enLineType { NONE, GRASS, ROAD, RAIL, WATER}
    public enLineType LineType = enLineType.GRASS;

    public GameObject JoiningDetail; //If we're adjacent to particular tiles we might want to toggle this

    public void Update()
    {
        if (LevelControllerScript.Instance)
        {
            if (LevelControllerScript.Instance.player)
            {
                if (LevelControllerScript.Instance.player.transform.position.z - gameObject.transform.position.z > clearDistance)
                {
                    Destroy(gameObject);     //And all the cleanup should handle the rest!
                }
            } else
            {
                Debug.LogError("No player assigned to LevelControllerScript");
            }
        }
    }

    public void SetJoiningDetail(bool state)
    {
        if (JoiningDetail)
        {
            JoiningDetail.SetActive(state);
        }
    }
}
