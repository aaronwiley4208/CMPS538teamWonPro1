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
	[SerializeField] Animator slurpAnim;
    [SerializeField] CapsuleCollider capsuleCollider;
	[SerializeField] SphereCollider sphereCollider;
	public GameObject slideBall;
	public GameObject pacBod;
	public GameObject wings;
	public PacManSize sizeScript;

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
		slideBall.SetActive (true);
		pacBod.SetActive (false);
		capsuleCollider.enabled = false;
		sphereCollider.enabled = true;

		suctionCollider.enabled = false;
		suctionCollider.isTrigger = true;

		wings.SetActive (false);
		slurpAnim = gameObject.GetComponent<Animator>();

		sizeScript = GetComponent<PacManSize>();


    }

    // Frequently used input variables
    Vector3 moveVector;
    Vector3 cameraRight;
    Vector3 cameraForward;
    Vector3 rotateVector;
    float turnAngle;

    // Base movement factor, unaffected by "energy"
    [SerializeField] float moveForceFactor = 5;

    // Jumping state
    bool willJump;
    [SerializeField] float jumpForceFactor = 8;
    public bool isGrounded = true;
    const float GROUND_CHECK_DIST = 0.5f;
	[SerializeField]float jumpRate = 0.75f;
	float jumpTimer = 0;
	[SerializeField]int jumps;
	[SerializeField]int maxJumps = 5;
	int jumpLevel = 0;

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
    const float MIN_SLIDING_DURATION = 0.25f;
    const float SLIDING_MIN_VELOCITY = 0.1f;
    const float SLIDING_VELOCITY_DECAY = 1;

    float capsuleStartHeight;
    float capsuleSlideHeight = 1;
    Vector3 capsuleStartCenter;
    Vector3 capsuleSlideCenter = new Vector3(0, 0.5f, 0);

	bool hasGrown = false;


    // Movement multiplier for player energy to affect
    [HideInInspector] public float movementMultiplier = 1;

    // Capture input
    void Update()
    {
		//faux gravity
		rigidbody.AddForce(Vector3.down * 1750 * (1 + 0.2f * sizeScript.level), ForceMode.Force);

		if(!sizeScript.isSmall){
			//turns on pacman body if you size up
			if(!hasGrown){
				slideBall.SetActive (false);
				pacBod.SetActive (true);
				capsuleCollider.enabled = true;
				sphereCollider.enabled = false;
				hasGrown = true;
			}

		}


        if (!isAlive)
            return;

		if (Input.GetKeyDown(KeyCode.E) && !isSliding)
		{
            // Remove Size from pac and make sure we can chomp
            if (sizeScript.MegaChomp())
			    slurpAnim.SetTrigger("Active");
		}
		

        // Get desired movement from input axes to camera-relative world space on the XZ plane
		if (Camera.main.enabled)
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
			rigidbody.AddForce(moveVector * (1 + 0.2f * sizeScript.level) * moveForceFactor * movementMultiplier + Vector3.ProjectOnPlane(currentSteepSlope, Vector3.up) * Vector3.ProjectOnPlane(currentSteepSlope, Vector3.up).magnitude * steepnessSlowMultiplier - Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up), ForceMode.VelocityChange);

        if (isSliding)
            //If sliding, turn in direction of velocity
            turnAngle = Mathf.DeltaAngle(0, Quaternion.FromToRotation(transform.forward, rigidbody.velocity).eulerAngles.y);
        else
            // else turn in direction of movement
            turnAngle = Mathf.DeltaAngle(0, Quaternion.FromToRotation(transform.forward, moveVector).eulerAngles.y);

        //		rigidbody.AddTorque (0, turnAngle - rigidbody.angularVelocity.y, 0, ForceMode.VelocityChange);
        transform.Rotate(0, turnAngle, 0);

        
        // Eat jump input
        if (willJump)
        {
			//if is sliding && !jumpKey (force change from slide)
            willJump = false;
            // You can only jump if grounded, not stomping, and not bouncing
            // TODO Maybe allow jumping in place of bouncing when a stomp lands, not eating the jump if bouncing, allowing the player to set up a bounce-jump while still falling
			if ((isGrounded || jumps > 1) && sizeScript.level > 0 && !isSliding && !isStomping && !isBouncing && jumpTimer < Time.time)
            {
				jumps -= 1;
                animator.SetTrigger("Jump");
				rigidbody.AddForce(Vector3.up * jumpForceFactor * (1 + 0.2f * sizeScript.level), ForceMode.Impulse);
				if (jumps < sizeScript.level-1) {
					wings.SetActive (true);
					print ("ggs");
				}
				isSliding = false;
				slideBall.SetActive (false);
				pacBod.SetActive (true);
				capsuleCollider.enabled = true;
				sphereCollider.enabled = false;
				jumpTimer = Time.time + jumpRate;
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
				capsuleCollider.enabled = true;
				sphereCollider.enabled = false;
                //animator.SetBool("Sliding", false);
            }
        }
        else
        {
            // Do you want to start sliding?
            // You can only slide if the player is grounded and moving fast enough
			if (willSlide && !sizeScript.isSmall && Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up).magnitude > SLIDING_MIN_VELOCITY && !(slurpAnim.GetCurrentAnimatorStateInfo(0).IsName("Slurp")))
            {
                // Start sliding
                isSliding = true;
				jumps -= maxJumps;
				slideBall.SetActive (true);
				pacBod.SetActive (false);
				capsuleCollider.enabled = false;
				sphereCollider.enabled = true;
                //animator.SetBool("Sliding", true);
                // Set the time we must be sliding until
                stopSlidingTime = Time.time + MIN_SLIDING_DURATION;
                // Remember the direction to keep sliding in
                slidingVelocity = 1.5f * Vector3.ProjectOnPlane(rigidbody.velocity, Vector3.up);
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

		if (Physics.Raycast (transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.01f) || Physics.Raycast (transform.position, Vector3.down, sphereCollider.bounds.extents.y + 0.01f)) {
			isGrounded = true;
            jumps = (maxJumps > sizeScript.level) ? sizeScript.level : maxJumps;
			wings.SetActive (false);
		} else {
			isGrounded = false;
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