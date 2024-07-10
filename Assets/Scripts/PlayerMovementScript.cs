using UnityEngine;
using System.Collections;
using DG.Tweening;

public class PlayerMovementScript : MonoBehaviour {
    public GameStateControllerScript gameStateController;

    public bool canMove = false;
    public LayerMask sopperLayerMask;
    float stepTime = 0.3f;

    public Animation HopAnimator;
    public GameObject CharacterBase;

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

    //private GameStateControllerScript gameStateController;
    private int score;

    void Awake()
    {
        DOTween.Init();
    }

    public void Start() {
        current = transform.position;
        moving = false;
        startY = transform.position.y;

        body = GetComponentInChildren<Rigidbody>();

        mesh = GameObject.Find("Player/Chicken");

        score = 0;
        //gameStateController = GameObject.Find("GameStateController").GetComponent<GameStateControllerScript>();
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

        score = Mathf.Max(score, (int)current.z);
        //gameStateController.score = score / 3;
        gameStateController.score = score / 3;

        TickEagle();
    }

    private void TickEagle()
    {
        if (gameStateController.state == GameStateControllerScript.enGameState.PLAY) //"play")
        {
            EagleTimeTicker += Time.deltaTime;
        }

        if (EagleTimeTicker > EagleWaitTime)
        {
            Debug.Log("Calling Eagle");
            gameStateController.TriggerEagle(transform.position);
        }

        if (backDirectionCount <= -maxBackmoves)
        {
            Debug.Log("Call Eagle");
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
			HandleMouseClick();
			return;
		}
		
        if (Input.GetKeyDown(KeyCode.W) || Input.GetButtonDown("Dup")) {
            Move(new Vector3(0, 0, 3));
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetButtonDown("Ddown")) {
            Move(new Vector3(0, 0, -3));
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetButtonDown("Dleft")) {
            if (Mathf.RoundToInt(current.x) > minX)
                Move(new Vector3(-3, 0, 0));
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetButtonDown("Dright")) {
            if (Mathf.RoundToInt(current.x) < maxX)
                Move(new Vector3(3, 0, 0));
        }
    }

    private void Move(Vector3 distance) {
        playerMoves++;
        EagleTimeTicker = 0; //We moved! Call off the Eagle!

        var newPosition = transform.position + distance;

        //PROBLEM: Need to see if we're moving into something
        // Don't move if blocked by obstacle.
        if (Physics.CheckSphere(newPosition + new Vector3(0.0f, 0.5f, 0.0f), 0.1f, sopperLayerMask)) 
            return;

        target = newPosition;

        moving = true;
        body.isKinematic = true;

        switch (MoveDirection) {
            case "north":
                CharacterBase.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (backDirectionCount < 0)
                {
                    backDirectionCount++;
                }
                break;
            case "south":
                CharacterBase.transform.rotation = Quaternion.Euler(0, 180, 0);
                if (backDirectionCount > 0)
                {
                    backDirectionCount = 0;
                }
                backDirectionCount--;
                break;
            case "east":
                CharacterBase.transform.rotation = Quaternion.Euler(0, 270, 0);
                break;
            case "west":
                CharacterBase.transform.rotation = Quaternion.Euler(0, 90, 0);
                break;
            default:
                break;
        }

        //So we want to move our player to a new location. Of course this is going to be forced modal.
        Vector3 targetPosition = makeModal(newPosition);
        HopAnimator.Stop(); //To give us a clean reset
        HopAnimator.Play();
        gameObject.transform.DOMove(targetPosition, stepTime).SetEase(Ease.Linear).OnComplete(() => ValidateMoveAndMap());
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
        return new Vector3(newPosition.x, newPosition.y, Mathf.FloorToInt(newPosition.z / 3f) * 3f);
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
                return null;
        }
    }

    public void GameOver() {
        // When game over, disable moving.
        canMove = false;

        // Call GameOver at game state controller (instead of sending messages).
        //gameStateController.GameOver();
        gameStateController.GameOver();
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
