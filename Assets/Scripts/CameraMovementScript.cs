using UnityEngine;
using System.Collections;
using static CanvasRotator;

//[RequireComponent(typeof(PlayerMovementScript))]
public class CameraMovementScript : MonoBehaviour {

    public float minZ = 0.0f;
    public float speedIncrementZ = 1.0f;
    public float speedOffsetZ = 4.0f;
    public bool moving = false;
    public float cameraOffset = 3;

    private float CamPositionZ = 0;
    public GameObject player;
    private PlayerMovementScript playerMovement;

    private Vector3 offset;
    private Vector3 initialOffset;

    Camera attachedCamera;

    public void Start() {
        attachedCamera = gameObject.GetComponentInChildren<Camera>();
        CamPositionZ = transform.position.z;

        //player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovementScript>();

        // TODO this position and rotation is baked, extract it
        initialOffset = new Vector3(2.5f, 10.0f, -7.5f);
        offset = initialOffset;
	}

    public void SetScreenOrientation(enScreenOrientation newScreenOrientation)
    {
        RectTransform ourRect = gameObject.GetComponent<RectTransform>();
        switch (newScreenOrientation)
        {
            case enScreenOrientation.LANDSCAPE:
                attachedCamera.transform.localEulerAngles = Vector3.zero;
                break;
            case enScreenOrientation.LEFT:
                attachedCamera.transform.localEulerAngles = Vector3.forward * 90f;
                break;
            case enScreenOrientation.RIGHT:
                attachedCamera.transform.localEulerAngles = Vector3.forward * 270f;
                break;
            default:
                break;
        }
    }

    public void LateUpdate() {
        //Lazy camera behaviour that doesn't represent what happens in the game at all, but works for the moment
        minZ = Mathf.Max(minZ, player.transform.position.z - 9); //We can only go back three rows so lets clamp the camera accordingly
        //lets just move our camera position in alignment with our player for the moment, knowing that the player will be forced forward by the Eagle
        CamPositionZ = Mathf.Lerp(CamPositionZ, Mathf.Max(minZ, player.transform.position.z), Time.deltaTime*2f); //So that we'll lerp after the player
        transform.position = new Vector3(transform.position.x, transform.position.y, CamPositionZ + cameraOffset);

        /*
        if (moving) {
            Vector3 playerPosition = player.transform.position;
            transform.position = new Vector3(playerPosition.x, 0, Mathf.Max(minZ, playerPosition.z)) + offset;

            // Increase z over time if moving.
            offset.z += speedIncrementZ * Time.deltaTime;

            // Increase/decrease z when player is moving south/north.
            if (playerMovement.IsMoving) {
                if (playerMovement.MoveDirection == "north") {
                    offset.z -= speedOffsetZ * Time.deltaTime;
                }
            }
        }
        */
    }

    public void Reset() {
        // TODO This kind of reset is dirty, refactor might be needed.
        moving = false;
        offset = initialOffset;
        transform.position = new Vector3(0f, 1f, 0f);
        minZ = 0;
        CamPositionZ = transform.position.z;
    }
}
