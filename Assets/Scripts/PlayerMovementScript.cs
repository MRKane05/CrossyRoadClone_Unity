﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class PlayerMovementScript : MonoBehaviour {
    public enum enDieType { NULL, CAR, TRAIN, WATER, EAGLE}

    public GameStateControllerScript gameStateController;
    public UI_MoveSpeedDisplay debugPanel;

    public bool canMove = false;
    public bool onLog = false;
    Vector3 wantsMove = Vector3.zero;

    public LayerMask stopperLayerMask;
    public LayerMask groundcastMask;
    float stepTime = 0.25f;
    public float stepModifier = 1f;

    public Animation HopAnimator;
    public AnimationClip HopStandard, HopFast;
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
    public AudioClip SFX_HitSide;
    public AudioClip SFX_Drown;

    //private GameStateControllerScript gameStateController;
    private int score;

    public GameObject EquippedPowerup;

    public AnimationCurve tossHeightCurve = new AnimationCurve();
    float tossHeight = 12;
    bool bDoingToss = false;

    public bool bPlayerInvincible = false;
    public bool bPlayerGhost = false;
    public List<Material> playerMaterials = new List<Material>();
    private Dictionary<Material, Material> materialMap = new Dictionary<Material, Material>();

    float AnimationFramerate = 30;
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

        //Sort out our materials as we'll be changing them for ghost mode
        materialMap.Clear();
        materialMap = new Dictionary<Material, Material>();

        playerMaterials.Clear();
        playerMaterials = new List<Material>();

        MeshRenderer[] playerRenderers = newCharacter.GetComponentsInChildren<MeshRenderer>();
        //Debug.LogError("playerRenderers count: " + playerRenderers.Length);

        foreach (MeshRenderer thisRenderer in playerRenderers)
        {
            Material[] originalMaterials = thisRenderer.sharedMaterials;
            //Debug.LogError("originalMaterials count: " + originalMaterials.Length);
            Material[] newMaterials = new Material[originalMaterials.Length];

            for (int i = 0; i < originalMaterials.Length; i++)
            {
                Material originalMaterial = originalMaterials[i];

                if (materialMap.ContainsKey(originalMaterial))
                {
                    //Debug.LogError("Material in dict");
                    // If we've already created a new material instance, reuse it
                    newMaterials[i] = materialMap[originalMaterial];
                }
                else
                {
                    // Create a new material instance and store it in the dictionary
                    Material newMaterial = new Material(originalMaterial);
                    materialMap[originalMaterial] = newMaterial;
                    newMaterials[i] = newMaterial;
                    //Debug.LogError("NewMaterial: " + newMaterial);
                }
            }

            thisRenderer.materials = newMaterials;
        }

        foreach (Material instanceMat in materialMap.Values)
        {
            //instanceMat.SetFloat("_GhostCol", bState ? 0f : 1f);
            playerMaterials.Add(instanceMat);
        }
    }

    public void SetGhost(bool bState)
    {
        bPlayerGhost = bState;
        foreach(Material instanceMat in playerMaterials)
        {
            instanceMat.SetFloat("_Alpha", bState ? 0f : 1f);
        }
    }

    void Awake()
    {
        DOTween.Init();
    }

    public void EquipPowerup(GameObject newPowerup)
    {
        //This could be 2x coins, hampster ball, or anything else. I'll have to have some method of figuring out what the powerups do and their effects
    }

    public void TossCharacter(Vector3 startPosition, float tossDistance)
    {
        if (!bDoingToss)
        {
            //We need some sort of throw animation...
            StartCoroutine(doCharacterToss(startPosition, tossDistance));
        }
    }

    public IEnumerator doCharacterToss(Vector3 startPosition, float tossDistance)
    {
        bDoingToss = true;
        Debug.Log("StartPosition: " + startPosition);
        startPosition = makeModal(startPosition);
        Vector3 endPosition = startPosition + Vector3.forward * tossDistance;
        endPosition = makeModal(endPosition);

        Debug.Log("EndPosition: " + endPosition);
        yield return null;
        float startTime = Time.time;
        float tossDuration = 1f;
        float lastLockPosition = startPosition.z;
        while (Time.time-startTime < tossDuration)
        {
            float positionLerp = Mathf.Clamp01((Time.time - startTime) / tossDuration);
            transform.position = Vector3.Lerp(startPosition, endPosition, positionLerp);
            transform.position += Vector3.up * tossHeight * tossHeightCurve.Evaluate(positionLerp);
            if (transform.position.z- lastLockPosition >= 3) {
                lastLockPosition = transform.position.z;
                ValidateMoveAndMap();
            }
            yield return null;
        }
        transform.position = endPosition;
        bDoingToss = false;
    }

    public void Start() {
        ourAudio = gameObject.GetComponent<AudioSource>();
        AnimationFramerate = HopAnimator.clip.frameRate;
        setTimeScale(1f);
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
            //Handle actioning our wants move if we have one
            if (wantsMove != Vector3.zero)
            {
                DoMove(wantsMove);
                wantsMove = Vector3.zero;
            }
        }

        //Input has now been seperated from states to make things more fluid
        HandleInput();

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

        //So we need something to handle our player speed tweaks for our hardcore users to give me some real numbers to work with
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("Left Shoulder"))
        {
            stepModifier = Mathf.Clamp(stepModifier - 0.1f, 0.2f, 1.5f);
            debugPanel.setDisplayText("MoveScale: " + stepModifier.ToString());
        }

        if (Input.GetKeyDown(KeyCode.RightShift) || Input.GetButtonDown("Right Shoulder"))
        {
            stepModifier = Mathf.Clamp(stepModifier + 0.1f, 0.2f, 1.5f);
            debugPanel.setDisplayText("MoveScale: " + stepModifier.ToString());
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

    public bool Move(Vector3 distance)
    {
        if (GameStateControllerScript.Instance.state != GameStateControllerScript.enGameState.PLAY) { return false; } //Don't let the player move if we're not in play mode
        if (bDoingToss) { return false; } //Don't let the player move while they're being tossed

        //A quick check state to see if we can act on this input, or if it should be our "wantMove" vector
        if ((canMove || onLog) && !moving)
        {
            return DoMove(distance);
        } else
        {
            wantsMove = distance;
        }
        return false;
    }
    public bool DoMove(Vector3 distance) { 

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
            return false;
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
        //HopAnimator.clip.frameRate = HopAnimator.clip.frameRate / Time.timeScale;
        gameObject.transform.DOMove(targetPosition, stepTime*stepModifier).SetEase(Ease.Linear).SetUpdate(true).OnComplete(() => ValidateMoveAndMap());

        PlaySound(SFX_Jump);
        return true;
    }

    public void setTimeScale(float newTimeScale)
    {
        if (newTimeScale == 1)
        {
            HopAnimator.clip = HopStandard;
        } else
        {
            HopAnimator.clip = HopFast;
        }
    }

    bool bPlayerGrounded()
    {
        //So lets do a raycast along a strip and see if we're on the ground
        float span = 0.9f;
        float raycount = 7f;
        for (int i = 0; i < raycount; i++)
        {
            float t = (float)i / raycount;
            float shift = Mathf.Lerp(-span, span, t);
            RaycastHit hit;
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(transform.position + Vector3.right * shift, -Vector3.up, out hit, 30, groundcastMask))
            {
                //Debug.DrawRay(transform.position, -Vector3.up * hit.distance, Color.yellow);
                //Debug.Log(hit.collider.gameObject.name);
                if (!hit.collider.gameObject.name.Contains("Water")) //We've landed in the water. Prevent our double hop
                {
                    return true;
                }
            }
        }
        return false;
    }

    void ValidateMoveAndMap()
    {
        moving = !bPlayerGrounded();    //If we're not grounded then lock our player movement so we drown
        body.isKinematic = false;
        //A basic raycast approach isn't the way to do this as it's possible to miss and for us to be on a log...
        //We need to check and see if we're over water, and if so have our character "die"
        //So lets see if we can raycast against whatever tile we're above now
        
        //Send a call through to our LevelController saying that we've moved and see if we've got to add a new line
        if (LevelControllerScript.Instance)
        {
            LevelControllerScript.Instance.PlayerMoved();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.name.Contains("Trunk"))   //We're resting on a trunk. Lets make sure we can move again
        {
            moving = false;
        }
    }

    private Vector3 makeModal(Vector3 newPosition)
    {
        if (GameStateControllerScript.Instance.ScreenOrientation == CanvasRotator.enScreenOrientation.LANDSCAPE)
        {
            return new Vector3(newPosition.x, 1f, Mathf.RoundToInt(newPosition.z / 3f) * 3f);
        }
        //Handle our side angles
        return new Vector3(Mathf.RoundToInt(newPosition.x / 3f) * 3f, 1f, newPosition.z);
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

    float powerupInvincibleStart = 0;

    public void GameOver(enDieType DieType, bool bSquashFlat = true) {
        if (bDoingToss) { return; } //Don't let our character die while we're being tossed
        if ((bPlayerInvincible || bPlayerGhost) && (DieType == enDieType.TRAIN || DieType == enDieType.CAR)) { return; }
        EagleTimeTicker = 0; //Reset our eagle timer just to be sure

        //We need to see if we've got an enabled powerup and if it has any effect here
        if (EquippedPowerup)
        {
            Powerup thisPowerup = EquippedPowerup.GetComponent<Powerup>();
            if (thisPowerup.PowerupType == Powerup.enPowerupType.ONHIT)
            {   //Pass this handling to the powerup
                bool bPowerupEffective = thisPowerup.GameOver(gameObject, DieType);
                if (bPowerupEffective)
                {
                    powerupInvincibleStart = Time.time;
                    Debug.Log("Doing Powerup Escape");
                    return; //Don't complete the rest of our code as it'll be handled by the powerup
                }
            }
        }
        
        // When game over, disable moving.
        canMove = false;

        // Call GameOver at game state controller (instead of sending messages).
        gameStateController.GameOver();

        //We need to figure out how we died to play the correct sound. For instance: was this drowning?
        if (DieType == enDieType.CAR)
        {
            //We should vary this for the case where we jump into the vehicle
            killMoveTween();

            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(scale.x, scale.y * 0.1f, scale.z);
            PlaySound(SFX_Hit);
        }
        if (DieType == enDieType.WATER)
        {
            PlaySound(SFX_Drown);
        }
    }

    public void killMoveTween()
    {
        transform.DOKill();
    }

    public void Reset() {
        // TODO This kind of reset is dirty, refactor might be needed.
        EagleTimeTicker = 0;
        CharacterBase.transform.localEulerAngles = Vector3.zero;
        body.isKinematic = true;
        transform.position = new Vector3(0, 1, 0);
        transform.localScale = new Vector3(1, 1, 1);
        transform.rotation = Quaternion.identity;
        moving = false;
        score = 0;
    }
}
