using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlyingEnemy : MonoBehaviour {

    public enum FlyingEnemyState { CHASE, PATROL, RETREAT }

    public FlyingEnemyState currentState;

    [Header("Patrol Fields")]
    public Transform[] waypoints;
    public int destPoint;
    public float patrolMoveSpeed;
    public float patrolRotateSpeed;
    public float detectionDistance;

    [Header("Chase Parameters")]
    public Transform pacman; // Target to chase down
    public float chaseMoveSpeed;
    [Tooltip("Rotation in degrees per second")]
    public float chaseRotateSpeed;
    [Tooltip("How far away can pac man get before we lose interest")]
    public float chaseDropoffDistance;

    [Header("Retreat Fields")]
    public float retreatMoveSpeed;
    public float retreatRotateSpeed;


    private Dictionary<FlyingEnemyState, Action> stateUpdates;
    private Dictionary<FlyingEnemyState, Action> stateTransitionsIn;
    private Dictionary<FlyingEnemyState, Action> stateTransitionsOut;

	// Use this for initialization
	void Start () {
        initDicts();
        currentState = FlyingEnemyState.CHASE;
        destPoint = 0;
	}

    // Initialize state dictionaries
    private void initDicts() {
        stateUpdates = new Dictionary<FlyingEnemyState, Action>();
        stateUpdates.Add(FlyingEnemyState.CHASE, ChaseUpdate);
        stateUpdates.Add(FlyingEnemyState.PATROL, PatrolUpdate);
        stateUpdates.Add(FlyingEnemyState.RETREAT, RetreatUpdate);

        stateTransitionsIn = new Dictionary<FlyingEnemyState, Action>();
        stateTransitionsIn.Add(FlyingEnemyState.CHASE, ChaseTransitionIn);
        stateTransitionsIn.Add(FlyingEnemyState.PATROL, PatrolTransitionIn);
        stateTransitionsIn.Add(FlyingEnemyState.RETREAT, RetreatTransitionIn);

        stateTransitionsOut = new Dictionary<FlyingEnemyState, Action>();
        stateTransitionsOut.Add(FlyingEnemyState.CHASE, ChaseTransitionOut);
        stateTransitionsOut.Add(FlyingEnemyState.PATROL, PatrolTransitionOut);
        stateTransitionsOut.Add(FlyingEnemyState.RETREAT, RetreatTransitionOut);
    }
	
	// Update is called once per frame
	void Update () {
        stateUpdates[currentState]();
	}

    #region STATE_UPDATES 
    private void ChaseUpdate() {
        // Move towards pacman
        MoveToward(pacman, chaseMoveSpeed, chaseRotateSpeed);

        // Check if we've lost interest
        Vector3 meToPacman = pacman.position - transform.position;
        if (meToPacman.magnitude > chaseDropoffDistance)
            Transition(FlyingEnemyState.CHASE, FlyingEnemyState.PATROL);
    }

    private void PatrolUpdate() {
        Vector3 targetPos = waypoints[destPoint].position;
        Vector3 meToTarget = targetPos - transform.position;
        // Update target if we're close to current waypoint
        if (meToTarget.magnitude < 0.05f) {
            destPoint = (destPoint + 1) % waypoints.Length; 
        }

        // Move towards waypoint
        MoveToward(waypoints[destPoint], patrolMoveSpeed, patrolRotateSpeed);

        // Check for distance to pacman
        Vector3 meToPacMan = pacman.position - transform.position;
        if (meToPacMan.magnitude < detectionDistance) {
            // Make sure line of sight is clear
            RaycastHit hit;
            //if (Physics.Raycast(transform.position, meToPacMan.normalized, out hit, detectionDistance)) {
                //if (hit.collider.gameObject.GetComponent<PacManSize>() != null) // Change to tag
                    Transition(FlyingEnemyState.PATROL, FlyingEnemyState.CHASE);
            //}
        }
    }

    private void RetreatUpdate() {
        MoveToward(waypoints[destPoint], retreatMoveSpeed, retreatRotateSpeed);
        // If we've reached that waypoint...
        Vector3 meToTarget = waypoints[destPoint].position - transform.position;
        if (meToTarget.magnitude < 0.075f)
            Transition(FlyingEnemyState.RETREAT, FlyingEnemyState.CHASE);
    }
    #endregion

    #region STATE_TRANSITIONS
    private void Transition(FlyingEnemyState outstate, FlyingEnemyState instate) {
        stateTransitionsOut[outstate]();
        currentState = instate;
        stateTransitionsIn[instate]();
    }

    private void ChaseTransitionIn() {

    }

    private void ChaseTransitionOut() {

    }

    private void PatrolTransitionIn() {

    }
    
    private void PatrolTransitionOut() {

    }

    private void RetreatTransitionIn() {
        destPoint = 0;
    }

    private void RetreatTransitionOut() {

    }
    #endregion

    #region MISC
    private void MoveToward(Transform target, float moveSpeed, float angularSpeed) {
        Vector3 meToPacman = (target.position - transform.position).normalized;
        float step = angularSpeed * Time.deltaTime;
        // Calc a vector in between current transform and target rotation
        Vector3 newDir = Vector3.RotateTowards(transform.forward, meToPacman, step, 0.0f);
        // Change our rotation to this new vector
        transform.rotation = Quaternion.LookRotation(newDir);

        // Now move in this direction toward pacman
        transform.Translate(transform.forward * moveSpeed * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter(Collision collision) {
        // If we hit pacman, deal some damage and retreat 
        if (collision.collider.tag == "PacMan") {
            collision.gameObject.GetComponent<PacManLife>().Hit();
            Transition(currentState, FlyingEnemyState.RETREAT);
        }
    }
    #endregion

}
