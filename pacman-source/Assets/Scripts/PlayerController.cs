using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Player controller for Pac-Man.
/// Pac-Man can run, jump, and butt-stomp.
/// This is code
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{

    new Rigidbody rigidbody;
    [SerializeField] Animator animator;
    [SerializeField] CapsuleCollider capsuleCollider;
	public GameObject slideBall;
	public GameObject pacBod;

    // Use this to initialize component references
    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        if (!animator)
            animator = GetComponentInChildren<Animator>();
        if (!capsuleCollider)
            capsuleCollider = GetComponentInChildren<CapsuleCollider>();
    }

    // Use this for initialization
    void Start()
    {
        // Capture non-sliding capsule parameters
        capsuleStartHeight = capsuleCollider.height;
        capsuleStartCenter = capsuleCollider.center;
        suctionCollider.enabled = false;
        suctionCollider.isTrigger = true;
        originalWingsScale = wings.localScale;
        hiddenWingsScale = Vector3.zero;
		slideBall.SetActive (false);
		pacBod.SetActive (true);
    }

    // Frequently used input variables
    Vector3 moveVector;
    Vector3 cameraRight;
    Vector3 cameraForward;
    Vector3 rotateVector;
    float turnAngle;

    public bool canFly;
    [SerializeField] Transform wings;
    Vector3 originalWingsScale;
    Vector3 hiddenWingsScale;
    float wingsPercent = 0;
    [SerializeField] float wingsTransitionSpeed = 5;
    [SerializeField] float wingsPower = 1;

    //To allow EnergyManager to access if Mega is activated or not
    static bool isMega = false;
    public static bool getMega()
    {
        return isMega;
    }
    static bool isFly = false;
    public static bool getFly()
    {
        return isFly;
    }



    // Base movement factor, unaffected by "energy"
    [SerializeField] float moveForceFactor = 5;

    // Jumping state
    bool willJump;
    [SerializeField] float jumpForceFactor = 8;
    public bool isGrounded = true;
    const float GROUND_CHECK_DIST = 0.5f;
	[SerializeField]float jumpRate = 0.75f;
	float jumpTimer = 0;

    //  Stomping state to prevent actions while stomping and detect stomp landings
    bool willStomp;
    [SerializeField] float stompForceFactor = 12;
    bool isStomping;

    // Bouncing state
    [SerializeField] float stompBounceForceFactor = 5;
    bool isBouncing;

    bool willSlide;
    bool isSliding;
    float stopSlidingTime;
    Vector3 slidingVelocity;
    const float MIN_SLIDING_DURATION = 0.5f;
    const float SLIDING_MIN_VELOCITY = 0.1f;
    const float SLIDING_VELOCITY_DECAY = 1;

    float capsuleStartHeight;
    float capsuleSlideHeight = 1;
    Vector3 capsuleStartCenter;
    Vector3 capsuleSlideCenter = new Vector3(0, 0.5f, 0);

    // Movement multiplier for player energy to affect
    [HideInInspector] public float movementMultiplier = 1;

    // Capture input
    void Update()
    {
		
        if (!isAlive)
            return;


        //To allow isBool to be called properly into EnergyManager
        if (megaChomp)
            isMega = true;
        else
            isMega = false;
        if (canFly)
            isFly = true;
        else
            isFly = false;


        // Get desired movement from input axes to camera-relative world space on the XZ plane
        if (Camera.main)
        {
            cameraRight = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            cameraForward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            moveVector = Input.GetAxis("Horizontal") * cameraRight + Input.GetAxis("Vertical") * cameraForward;
            moveVector.y = 0;
        }
        else
        {
            moveVector.z = Input.GetAxis("Vertical");
            moveVector.x = Input.GetAxis("Horizontal");
            moveVector.y = 0;
        }

        animator.SetFloat("Forward", Vector3.Dot(transform.forward, moveVector) * movementMultiplier);
        // Display vertical velocity if in the air
        if (!isGrounded)
            animator.SetFloat("Vertical", rigidbody.velocity.y);
        // If the player is grounded or on a moving platform, don't display vertical velocity
        else
            animator.SetFloat("Vertical", 0);

        // Capture jump input
        if (Input.GetKeyDown(KeyCode.Space))
        {
            willJump = true;
        }
        else
            willJump = false;
		
        // Capture stomp input
        if (Input.GetKeyDown(KeyCode.Q))
            willStomp = true;
        else
            willStomp = false;

        // Capture slide input
        if (Input.GetKey(KeyCode.LeftShift))
            willSlide = true;
        else
            willSlide = false;



        // Throwing condition for no energy  = no MegaChomp
        if (Input.GetKeyDown(KeyCode.E))
            //if (EnergyManager.getEnergy()> 0.0f)
            //{
            //    MegaChomp = !MegaChomp;
            //}

            // Cancelling MegaChomp and fly on no energy
            //if (EnergyManager.getEnergy() <= 0)
            //{
            //    MegaChomp = false;
            //    canFly = false;
            //}


            if (Input.GetKey(KeyCode.F))
            {
                //if (EnergyManager.getEnergy () > 0.0f) {
                //	canFly = true;
                //}
            }
            else
                canFly = false;



        //if (EnergyManager.getHealth() <= 0 && exitManager.getVictory() == false)
        //{
        //    Die();
        //}

        //if (Input.GetButton ("Jump") && !isStomping && !isSliding && EnergyManager.getEnergy () > 0.0f) {
        //		canFly = true;
        //		wingsPercent += Time.deltaTime * 3;
        //} else {
        //		canFly = false;
        //		wingsPercent -= Time.deltaTime * 3;
        //}

        //wingsPercent = Mathf.Clamp01 (wingsPercent);
        //wings.localScale = Vector3.Lerp (hiddenWingsScale, originalWingsScale, wingsPercent);

    }

    // Apply the input captured in Update()
    void FixedUpdate()
    {
        // Update the state-keeping booleans having to do with state first
        // NOTE: Deprecated?
        //CheckGroundStatus();
		checkforGround();

        // Apply movement only if not stomping (stomping halts XZ movement) or sliding
        Debug.DrawRay(transform.position, currentSteepSlope * 4, Color.red);
        if (!isStomping && !isSliding)
            rigidbody.AddForce(moveVector * moveForceFactor * movementMultiplier + Vector3.ProjectOnPlane(currentSteepSlope, Vector3.up) * Vector3.ProjectOnPlane(currentSteepSlope, Vector3.up).magnitude * steepnessSlowMultiplier - Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up), ForceMode.VelocityChange);

        if (isSliding)
            //If sliding, turn in direction of velocity
            turnAngle = Mathf.DeltaAngle(0, Quaternion.FromToRotation(transform.forward, rigidbody.velocity).eulerAngles.y);
        else
            // else turn in direction of movement
            turnAngle = Mathf.DeltaAngle(0, Quaternion.FromToRotation(transform.forward, moveVector).eulerAngles.y);

        //		rigidbody.AddTorque (0, turnAngle - rigidbody.angularVelocity.y, 0, ForceMode.VelocityChange);
        transform.Rotate(0, turnAngle, 0);

        if (canFly)
            rigidbody.AddForce(Vector3.up / Time.fixedDeltaTime * wingsPower * wingsPercent - Vector3.up * rigidbody.velocity.y, ForceMode.Force);

        // Eat jump input
        if (willJump)
        {
            willJump = false;
            // You can only jump if grounded, not stomping, and not bouncing
            // TODO Maybe allow jumping in place of bouncing when a stomp lands, not eating the jump if bouncing, allowing the player to set up a bounce-jump while still falling
			if (/*isGrounded &&*/ !isStomping && !isBouncing && jumpTimer < Time.time)
            {
                animator.SetTrigger("Jump");
				rigidbody.AddForce(Vector3.up * jumpForceFactor, ForceMode.VelocityChange);
                // Jumping interrupts sliding
                isSliding = false;
                animator.SetBool("Sliding", false);
				jumpTimer = Time.time + jumpRate;
            }
        }



        // Stomping
        if (willStomp)
        {
            willStomp = false;
            // You can only stomp if you're in the air, not already stomping, and not bouncing from a previous stomp
            if (!isGrounded && !isStomping && !isBouncing)
            {
                isStomping = true;
                animator.SetBool("Stomping", true);
                //rigidBody.AddForce (Vector3.down * stompForceFactor, ForceMode.VelocityChange);
                rigidbody.velocity = Vector3.down * stompForceFactor;
            }
        }

        // Sliding
        if (isSliding)
        {
            // Do you want to keep sliding?
            // Keep sliding if slide is held or you have not slid for the minimum time
            if ((willSlide || Time.time < stopSlidingTime))
            {
                // Slide
                rigidbody.AddForce(slidingVelocity - Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up), ForceMode.VelocityChange);
                // Slide with less force over time
                slidingVelocity = Vector3.MoveTowards(slidingVelocity, Vector3.zero, SLIDING_VELOCITY_DECAY * Time.fixedDeltaTime);
            }
            else
            {
                // Stop sliding
                isSliding = false;
				slideBall.SetActive (false);
				pacBod.SetActive (true);

                //animator.SetBool("Sliding", false);
            }
        }
        else
        {
            // Do you want to start sliding?
            // You can only slide if the player is grounded and moving fast enough
            if (willSlide && Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up).magnitude > SLIDING_MIN_VELOCITY)
            {
                // Start sliding
                isSliding = true;
				slideBall.SetActive (true);
				pacBod.SetActive (false);
                //animator.SetBool("Sliding", true);
                // Set the time we must be sliding until
                stopSlidingTime = Time.time + MIN_SLIDING_DURATION;
                // Remember the direction to keep sliding in
                slidingVelocity = Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up);
            }
        }

        // Scale capsule to the right size for standing vs. sliding
        capsuleCollider.height = Mathf.MoveTowards(capsuleCollider.height, (isSliding) ? capsuleSlideHeight : capsuleStartHeight, Time.deltaTime * 2);
        capsuleCollider.center = Vector3.MoveTowards(capsuleCollider.center, (isSliding) ? capsuleSlideCenter : capsuleStartCenter, Time.deltaTime * 2);
    }

    Vector3 currentSteepSlope;
    [SerializeField] float steepnessSlowMultiplier = 3;
    [SerializeField] LayerMask mask;
    // Utility method to check whether the player is grounded
    // I stole this from Unity's ThirdPersonCharacter.cs
    
	void checkforGround(){
		RaycastHit hit;

		if (Physics.Raycast (transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f)) {
			isGrounded = true;
		} else {
			isGrounded = false;
		}

	}


	void CheckGroundStatus()
    {
        RaycastHit hitInfo;
#if UNITY_EDITOR
        // helper to visualise the ground check ray in the scene view
        Debug.DrawLine(transform.position + (Vector3.up * GROUND_CHECK_DIST / 2), transform.position + (Vector3.up * GROUND_CHECK_DIST / 2) + (Vector3.down * GROUND_CHECK_DIST));
#endif
        // 0.1f is a small offset to start the ray from inside the character
        isGrounded = false;
        float leastFlatness = Mathf.Infinity;
        currentSteepSlope = Vector3.up;
        if (Physics.Raycast(transform.position + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask))
        {
            isGrounded = true;
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }
        if (Physics.Raycast(transform.position + transform.forward * capsuleCollider.radius + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask))
        {
            isGrounded = true;
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }
        if (Physics.Raycast(transform.position + transform.right * capsuleCollider.radius + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask))
        {
            isGrounded = true;
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }
        if (Physics.Raycast(transform.position + transform.forward * -capsuleCollider.radius + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask))
        {
            isGrounded = true;
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }
        if (Physics.Raycast(transform.position + transform.right * -capsuleCollider.radius + (Vector3.up * 0.1f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask))
        {
            isGrounded = true;
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }


        // Forward check just for steepness
        if (Physics.Raycast(transform.position, transform.forward, out hitInfo, capsuleCollider.radius * 1.1f, mask))
        {
            //Debug.Log (hitInfo.transform.name);
            float thisFlatness = Vector3.Dot(hitInfo.normal, Vector3.up);
            if (thisFlatness < leastFlatness)
            {
                leastFlatness = thisFlatness;
                currentSteepSlope = hitInfo.normal;
            }
        }

        //		if (Physics.SphereCast(transform.position + Vector3.up * (capsuleCollider.radius * 1.3f), (capsuleCollider.radius * 1.3f), Vector3.down, out hitInfo, GROUND_CHECK_DIST, mask)) {
        //			isGrounded = true;
        //			float thisFlatness = Vector3.Dot (hitInfo.normal, Vector3.up);
        //			if (thisFlatness < leastFlatness) {
        //				leastFlatness = thisFlatness;
        //				currentSteepSlope = hitInfo.normal;
        //			}
        //		}

        if (isGrounded)
        {
            if (isStomping)
            {
                // Stomped on something
                isStomping = false;
                animator.SetBool("Stomping", false);
                animator.SetTrigger("Stomp");
                isBouncing = true;
                rigidbody.AddForce(Vector3.up * (stompBounceForceFactor - rigidbody.velocity.y), ForceMode.VelocityChange);
                //OnStomp (hitInfo);
            }
            else if (isBouncing)
                isBouncing = false;
        }

    }

    [SerializeField] GameObject stompImpactEffectPrefab;

    // Called when the player stomps on the ground
    // hitInfo: Collision with the ground information
    void OnStomp(RaycastHit hitInfo)
    {
        // If stomping on an "stompable" object, send a message
        ////IStompable stompable = hitInfo.transform.GetComponentInParent<IStompable> ();
        ////if (stompable != null)
        ////	stompable.OnStomp (rigidbody, hitInfo);
        ////// Create a stomp effect
        ////Instantiate(stompImpactEffectPrefab, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));

        ////if (hitInfo.collider.GetComponent<enemyWaypoint> ())
        ////	Destroy (hitInfo.collider.gameObject);
    }

    bool megaChomp;
    [SerializeField] Collider suctionCollider;
    [SerializeField] float suctionForceFactor = 0.2f;

    public bool MegaChomp
    {
        get
        {
            return megaChomp;
        }
        set
        {
            if (value != megaChomp)
            {
                StopCoroutine("DoMegaChompAnimation");
                StartCoroutine("DoMegaChompAnimation");
            }
            megaChomp = value;
            suctionCollider.enabled = value;
        }
    }

    Vector3 originalScale;
    [SerializeField] float megaChompScaleFactor = 1.5f;
    IEnumerator DoMegaChompAnimation()
    {
        if (originalScale == Vector3.zero)
            originalScale = transform.localScale;
        Vector3 megaScale = originalScale * megaChompScaleFactor;
        bool flashingState = megaChomp;
        for (int i = 0; i < 7; i++)
        {
            transform.localScale = (flashingState) ? megaScale : originalScale;
            flashingState = !flashingState;
            yield return new WaitForSeconds(0.1f);
        }
        transform.localScale = (megaChomp) ? megaScale : originalScale;
    }
    [SerializeField]
    AudioClip fruitPickup;
    [SerializeField]
    AudioClip pelletPickup;
    [SerializeField]
    AudioClip eatGhost;
    [SerializeField]
    AudioClip deathSound;

    void OnCollisionEnter(Collision col)
    {
        if (!isAlive)
            return;
        //////if (col.gameObject.CompareTag ("Hazard")) {
        //////	if (col.gameObject.GetComponent<enemyWaypoint> () && isStomping) {
        //////		// you stomped it reel good
        //////		Destroy(col.gameObject);
        //////	} else
        //////	 Die ();
        //////} else if (col.gameObject.CompareTag ("Pickup") || col.gameObject.CompareTag("Fruit")) { //////////////added this fruit shit ask me about it if it fucks something up -Steven
        //////	Pickup hitPickup = col.gameObject.GetComponent<Pickup> ();
        //////	if (!hitPickup)
        //////		return;
        //////          // Play "eat" sound effect
        //////          if (col.gameObject.CompareTag("Fruit"))
        //////          {
        //////              GetComponent<AudioSource>().PlayOneShot(fruitPickup);
        //////          }
        //////          else if (col.gameObject.CompareTag("Pickup"))
        //////          {
        //////              //AudioSource.PlayClipAtPoint(pelletPickup, transform.position, 0.15f);
        //////              GetComponent<AudioSource>().PlayOneShot(pelletPickup, 0.2f);
        //////          }

        //////          hitPickup.OnConsume ();
        //////} else {
        //////	Ghost hitGhost = col.gameObject.GetComponent<Ghost> ();
        //////	if (hitGhost) {
        //////		if (hitGhost.state == Ghost.GhostState.Hazard && exitManager.getVictory()==false) {
        //////			Die ();
        //////		} else if (hitGhost.state == Ghost.GhostState.Vulnerable) {
        //////                  // Play "eat" sound effect
        //////                  GetComponent<AudioSource>().PlayOneShot(eatGhost);
        //////                  hitGhost.OnConsume ();
        //////		}
        //////	}
        //////}
    }

    void OnTriggerStay(Collider col)
    {
        if (col.gameObject.CompareTag("Pickup"))
        {
            Rigidbody pickupRigidbody = col.GetComponent<Rigidbody>();
            if (pickupRigidbody)
                pickupRigidbody.AddForce((transform.position - pickupRigidbody.position) / Time.fixedDeltaTime * suctionForceFactor);
        }

    }

    [HideInInspector] public bool isAlive = true;
    public void Die()
    {
        if (!isAlive)
            return;
        isAlive = false;
        animator.SetTrigger("Die");
        GetComponent<AudioSource>().PlayOneShot(deathSound);
        //rigidbody.isKinematic = true;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
        //////CutoutEffects.singleton.Show (1, OnDeathFinished);
    }

    public void OnDeathFinished()
    {
        //////Transform startPosition = GameLogic.currentLevel.playerStartPosition;
        //////      EnergyManager.resetHealth();
        //////      EnergyManager.resetEnergy();
        //////if (startPosition)
        //////	Revive (startPosition.position, startPosition.rotation);
        //////else
        //////	Debug.LogError ("No start position found");
    }

    public void Revive(Vector3 position, Quaternion rotation)
    {
        //////CutoutEffects.singleton.Hide (1);
        isAlive = true;
        //rigidbody.isKinematic = false;
        //rigidbody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        transform.position = position;
        animator.Play("revive");
        StopCoroutine("Blink");
        StartCoroutine("Blink");
    }

    [SerializeField] Renderer renderer;
    IEnumerator Blink()
    {
        bool flashingState = false;
        for (int i = 0; i < 15; i++)
        {
            renderer.enabled = flashingState;
            flashingState = !flashingState;
            yield return new WaitForSeconds((i < 8) ? 0.25f : 0.15f);
        }
        renderer.enabled = true;
    }

}