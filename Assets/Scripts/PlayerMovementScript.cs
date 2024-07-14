using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerMovementScript : MonoBehaviour {
    public GameStateControllerScript gameStateController;

    public bool canMove = false;
    public LayerMask stopperLayerMask;
    float stepTime = 0.3f;

    public Animation HopAnimator;
    public GameObject CharacterBase;    //What our prefab character will be spawned on (logically)
    public GameObject DefaultCharacter;

    public int minX = -4;
    public int maxX = 4;

    private bool moving;

    private Vector3 current;
    private Vector3 target;
    private float startY;

    private Rigidbody body;
    private GameObject mesh;

    //And the stuff for our eagle:
    float EagleTimeTicker = 0;
    int playerMoves = 0;
    int backDirectionCount = 0;
    public float EagleWaitTime = 5f;
    public int maxBackmoves = 3;

    AudioSource ourAudio;
    public AudioClip SFX_Jump;
    public AudioClip SFX_Hit;
    public AudioClip SFX_Drown;

    //private GameStateControllerScript gameStateController;
    private int score;

    public void SetCharacter(GameObject thisCharacter)
    {
        //Clear anything we might have
        foreach (Transform child in CharacterBase.transform)
        {
            Destroy(child.gameObject);
        }

        GameObject newCharacter = Instantiate(thisCharacter, CharacterBase.transform);
        newCharacter.transform.localPosition = Vector3.zero;
        newCharacter.transform.localEulerAngles = Vector3.zero;
    }

    void Awake()
    {
        DOTween.Init();
    }

    public void Start() {
        ourAudio = gameObject.GetComponent<AudioSource>();
        SetCharacter(DefaultCharacter);

        current = transform.position;
        moving = false;
        startY = transform.position.y;

        body = GetComponentInChildren<Rigidbody>();

        mesh = GameObject.Find("Player/Chicken");

        score = 0;
    }

    public void PlaySound(AudioClip targetSound)
    {
        ourAudio.PlayOneShot(targetSound);
    }

    public void Update() {
        // If player is moving, update the player position, else receive input from user.
        if (moving)
        {
            //MovePlayer();
        }
        else
        {
            // Update current to match integer position (not fractional).
            current = new Vector3(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y), Mathf.Round(transform.position.z));

            if (canMove)
                HandleInput();
        }

        score = Mathf.Max(score, (int)current.z/3);
        //gameStateController.score = score / 3;
        gameStateController.score = score;// / 3;

        TickEagle();
    }

    private void TickEagle()
    {
        if (gameStateController.state == GameStateControllerScript.enGameState.PLAY)
        {
            EagleTimeTicker += Time.deltaTime;
        }

        if (EagleTimeTicker > EagleWaitTime)
        {
            //Debug.Log("Calling Eagle");
            gameStateController.TriggerEagle(transform.position);
        }

        if (backDirectionCount >= maxBackmoves)
        {
            //Debug.Log("Call Eagle");
            gameStateController.TriggerEagle(transform.position);
        }
    }
	
	private void HandleMouseClick() {
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		
		if (Physics.Raycast(ray, out hit)) {
			Vector3 direction = hit.point - transform.position;
			float x = direction.x;
			float z = direction.z;
			
			if (Mathf.Abs(z) > Mathf.Abs(x)) {
				if (z > 0)
					Move(new Vector3(0, 0, 3));
                else
					Move(new Vector3(0, 0, -3));
			}
            else { // (Mathf.Abs(z) < Mathf.Abs(x))
				if (x > 0) {
					if (Mathf.RoundToInt(current.x) < maxX)
						Move(new Vector3(3, 0, 0));
				}
                else { // (x < 0)
					if (Mathf.RoundToInt(current.x) > minX)
						Move(new Vector3(-3, 0, 0));
				}
			}
        }
	}

    private void HandleInput() {	
		// Handle mouse click
		if (Input.GetMouseButtonDown(0)) {
			//HandleMouseClick();   //Disable our touch input for now
			return;
		}
		
        if (Input.GetKeyDown(KeyCode.W) || Input.GetButtonDown("Dup") || Input.GetButtonDown("Triangle")) {
            Move(new Vector3(0, 0, 3));
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetButtonDown("Ddown") || Input.GetButtonDown("Cross")) {
            Move(new Vector3(0, 0, -3));
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetButtonDown("Dleft") || Input.GetButtonDown("Square")) {
            if (Mathf.RoundToInt(current.x) > minX)
                Move(new Vector3(-3, 0, 0));
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetButtonDown("Dright") || Input.GetButtonDown("Circle")) {
            if (Mathf.RoundToInt(current.x) < maxX)
                Move(new Vector3(3, 0, 0));
        }
    }

    private void Move(Vector3 distance) {
        if (GameStateControllerScript.Instance.state != GameStateControllerScript.enGameState.PLAY) { return; } //Don't let the player move if we're not in play mode
        //Debug.Log("In Distance: " + distance);
        //So if we're rotated we'll have to rotate our movement direction...
        switch (GameStateControllerScript.Instance.ScreenOrientation)
        {
            case CanvasRotator.enScreenOrientation.LANDSCAPE:
                break; //Do nothing with our input
            case CanvasRotator.enScreenOrientation.LEFT:
                distance = Quaternion.AngleAxis(270, Vector3.up) * distance;
                break;
            case CanvasRotator.enScreenOrientation.RIGHT:
                distance = Quaternion.AngleAxis(90, Vector3.up) * distance;
                break;
            default:
                break;
        }

        //Debug.Log("Rotated Distance: " + distance);

        playerMoves++;
        EagleTimeTicker = 0; //We moved! Call off the Eagle!

        var newPosition = transform.position + distance;

        //PROBLEM: Need to see if we're moving into something
        // Don't move if blocked by obstacle.
        
        if (Physics.CheckSphere(newPosition + new Vector3(0.0f, 0.5f, 0.0f), 0.1f, stopperLayerMask))
        {
            Debug.Log("Got Collision With Move");
            return;
        }

        target = newPosition;

        moving = true;
        body.isKinematic = true;
        //Debug.Log(MoveDirection);
        CharacterBase.transform.LookAt(CharacterBase.transform.position + distance, Vector3.up);
        if (distance.z < 0) //We're moving backwards
        {
            backDirectionCount++;
        }
        else
        {
            backDirectionCount = Mathf.Max(0, backDirectionCount - 1);
        }
        //Debug.Log("BackCount: " + backDirectionCount);

        //So we want to move our player to a new location. Of course this is going to be forced modal.
        Vector3 targetPosition = makeModal(newPosition);
       
        HopAnimator.Stop(); //To give us a clean reset
        HopAnimator.Play();
        gameObject.transform.DOMove(targetPosition, stepTime).SetEase(Ease.Linear).OnComplete(() => ValidateMoveAndMap());

        PlaySound(SFX_Jump);
    }

    void ValidateMoveAndMap()
    {
        moving = false;
        body.isKinematic = false;
        //Send a call through to our LevelController saying that we've moved and see if we've got to add a new line
        if (LevelControllerScript.Instance)
        {
            LevelControllerScript.Instance.PlayerMoved();
        }
    }

    private Vector3 makeModal(Vector3 newPosition)
    {
        if (GameStateControllerScript.Instance.ScreenOrientation == CanvasRotator.enScreenOrientation.LANDSCAPE)
        {
            return new Vector3(newPosition.x, newPosition.y, Mathf.RoundToInt(newPosition.z / 3f) * 3f);
        }
        //Handle our side angles
        return new Vector3(Mathf.RoundToInt(newPosition.x / 3f) * 3f, newPosition.y, newPosition.z);
    }

    public bool IsMoving {
        get { return moving; }
    }

    public string MoveDirection {
        get
        {
            if (moving) {
                float dx = target.x - current.x;
                float dz = target.z - current.z;
                if (dz > 0)
                    return "north";
                else if (dz < 0)
                    return "south";
                else if (dx > 0)
                    return "west";
                else
                    return "east";
            }
            else
                return "not moving";
        }
    }

    public void GameOver(bool bPlayDieSound, bool bPlayDrownSound) {
        // When game over, disable moving.
        canMove = false;

        // Call GameOver at game state controller (instead of sending messages).
        //gameStateController.GameOver();
        gameStateController.GameOver();

        //We need to figure out how we died to play the correct sound. For instance: was this drowning?
        if (bPlayDieSound)
        {
            PlaySound(SFX_Hit);
        }
        if (bPlayDrownSound)
        {
            PlaySound(SFX_Drown);
        }
    }

    public void Reset() {
        // TODO This kind of reset is dirty, refactor might be needed.
        body.isKinematic = true;
        transform.position = new Vector3(0, 1, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.identity;

        score = 0;
    }
}
