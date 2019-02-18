using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrolBehavior : MonoBehaviour {

    public enum PatrolState { PATROL, ATTACK, PUKE, RETREAT }

    [Tooltip("Current state, don't adjust")]
    public PatrolState currentState;
    [Header("Patrol Fields")]
    [Tooltip("Waypoints to patrol through")]
    public Transform[] wayPoints;
    [Tooltip("Minimum time from start of patrol to puke")]
    public float minTimeTilPuke;
    [Tooltip("Maximum time from start of patrol to puke")]
    public float maxTimeTilPuke;

    [Header("Detection Fields")]
    [Tooltip("Player, so we can try to find him")]
    public Transform pacman;
    [Tooltip("Cone of sight angle")]
    public float detectionAngle;
    [Tooltip("How far away can the enemy see")]
    public float detectionDistance;
    public float angleToPac;
    public bool detectPacman;

    [Header("Puking Fields")]
    [Tooltip("Prefab for poison pellet")]
    public GameObject poisonPellet;
    [Tooltip("Prefab for good pellet")]
    public GameObject goodPellet;
    [Tooltip("How much time to wait in puke mode")]
    public float timeInPukeState;
    [Tooltip("Chance to puke a good pellet")]
    [Range(0, 100)]
    public float pukeChanceForGood;

    [Header("Attacking Fields")]
    [Tooltip("How far away pacman gets before we resume patroling")]
    public float attackDropOffDistance;

    [Header("Retreating Fields")]
    [Tooltip("How fast the ghost runs when retreating")]
    public float retreatSpeed;


    // Which waypoint is the destination
    public int destPoint = 0;
    private NavMeshAgent navAgent;

    // Dict for calling each state's update function
    private Dictionary<PatrolState, Action> patrolUpdates;
    // Dict for calling each state's transition function (transitioning into)
    private Dictionary<PatrolState, Action> patrolTransitionsIn;
    // Dict for calling each state's function for transitioning out of that state
    private Dictionary<PatrolState, Action> patrolTransitionsOut;

	// Use this for initialization
	void Start () {
        SetupDicsts();
        // Now set up agent
        navAgent = GetComponent<NavMeshAgent>();
        navAgent.autoBraking = false;

        // Start by going into patrol state
        patrolTransitionsIn[PatrolState.PATROL]();
	}
	
	// Update is called once per frame
	void Update () {
        patrolUpdates[currentState]();
	}

    private void SetupDicsts() {
        patrolUpdates = new Dictionary<PatrolState, Action>();
        patrolUpdates.Add(PatrolState.PATROL, PatrolUpdate);
        patrolUpdates.Add(PatrolState.ATTACK, AttackUpdate);
        patrolUpdates.Add(PatrolState.PUKE, PukeUpdate);
        patrolUpdates.Add(PatrolState.RETREAT, RetreatUpdate);

        patrolTransitionsIn = new Dictionary<PatrolState, Action>();
        patrolTransitionsIn.Add(PatrolState.ATTACK, AttackTransitionIn);
        patrolTransitionsIn.Add(PatrolState.PATROL, PatrolTransitionIn);
        patrolTransitionsIn.Add(PatrolState.PUKE, PukeTransitionIn);
        patrolTransitionsIn.Add(PatrolState.RETREAT, RetreatTransitionIn);

        patrolTransitionsOut = new Dictionary<PatrolState, Action>();
        patrolTransitionsOut.Add(PatrolState.ATTACK, AttackTransitionOut);
        patrolTransitionsOut.Add(PatrolState.PATROL, PatrolTransitionOut);
        patrolTransitionsOut.Add(PatrolState.PUKE, PukeTransitionOut);
        patrolTransitionsOut.Add(PatrolState.RETREAT, RetreatTransitionOut);
    }

    private void GoToNextPoint() {
        // If no points, just return
        if (wayPoints.Length == 0)
            return;

        // Cylce to the next point
        destPoint = (destPoint + 1) % wayPoints.Length;

        // Tell agent to go to currently selected destination
        navAgent.destination = wayPoints[destPoint].position;
    }

    // For when you want to just resume going to your current destination
    private void GoToCurrentPoint() {
        // If no points, just return
        if (wayPoints.Length == 0)
            return;

        // Tell agent to go to currently selected destination
        navAgent.destination = wayPoints[destPoint].position;
    }

    #region STATE_UPDATES
    private void PatrolUpdate() {
        detectPacman = false;
        // If we've almost reached the point, set goal to next point.
        if (!navAgent.pathPending && navAgent.remainingDistance < 0.1f)
            GoToNextPoint();

        // Use a rigged cone-like detection to see if pacman is within a certain angle of line-of-sight.
        Vector3 meToPacman = (pacman.position - transform.position).normalized;
        float angleToPac = Mathf.Rad2Deg * Mathf.Acos(Vector3.Dot(transform.forward, meToPacman));
        if (angleToPac < detectionAngle) {
            // If so, raycast to him and see if it hits him.
            RaycastHit hit;
            if (Physics.Raycast(transform.position, meToPacman, out hit, detectionDistance)) {
                if (hit.transform.gameObject.GetComponent<PacManSize>() != null) { // TODO: Change to a tag check
                    Transition(PatrolState.PATROL, PatrolState.ATTACK);
                }
            }
        }
    }

    private void AttackUpdate() {
        // Move myself towards pacman
        navAgent.destination = pacman.position;

        // If pacman gets away (is more than X m away)
        float distanceToPac = (pacman.position - transform.position).magnitude;
        if (distanceToPac > attackDropOffDistance) {
            Transition(PatrolState.ATTACK, PatrolState.PATROL);
        }
    }

    private void PukeUpdate() {
        // Update does nothing cause we're just waiting for coroutine to run o;ut.
    }

    // Retreat is triggered when A ghost hits pacman, then it runs back to its starting position
    private void RetreatUpdate() {
        // Move me to the starting pose
        Vector3 meToStart = wayPoints[0].position - transform.position;
        transform.Translate(meToStart.normalized * retreatSpeed * Time.deltaTime, Space.World);
        // When we're near starting point, transition back to patrol
        if (meToStart.magnitude < 0.15f)
            Transition(PatrolState.RETREAT, PatrolState.PATROL);

    }
    #endregion

    #region STATE_TRANSITIONS
    // Transition out of one state and into another
    private void Transition(PatrolState transOut, PatrolState transIn) {
        patrolTransitionsOut[transOut]();
        currentState = transIn;
        patrolTransitionsIn[transIn]();
    }

    private void AttackTransitionOut() {
        // Nuttin yet
    }

    private void AttackTransitionIn() {
        navAgent.destination = pacman.position;
    }

    private void PatrolTransitionIn() {
        GoToCurrentPoint();
        StartCoroutine("WaitToPuke");
    }

    private void PatrolTransitionOut() {
        // Cancel the wait to puke, don't wanna puke when attacking
        StopCoroutine("WaitToPuke");
    }

    private void PukeTransitionIn() {
        // Gen a number to see if we drop a good or bad pellet.
        float probability = UnityEngine.Random.Range(0, 100);
        // Do the puking
        GameObject pelletObj = Instantiate((probability < pukeChanceForGood) ? goodPellet : poisonPellet, transform.position, Quaternion.identity);
        // Put that shit on the floor
        RaycastHit hit;
        if (Physics.Raycast(pelletObj.transform.position, Vector3.down, out hit))
            pelletObj.transform.Translate(Vector3.down * (hit.distance - pelletObj.transform.localScale.x/2)); // Assumes the pellet is a sphere

        // Stop him from moving and start timer
        navAgent.isStopped = true;
        StartCoroutine("WaitToStopPuking");
    }

    private void PukeTransitionOut() {
        // Stop being stopped
        navAgent.isStopped = false;
    }

    private void RetreatTransitionIn() {
        // When we start retreating, turn off the collider (so we can move through stuff and not hit pacman) and stop navAgentIng
        GetComponent<Collider>().enabled = false;
        navAgent.enabled = false;
        destPoint = 0;
    }

    private void RetreatTransitionOut() {
        // When we stop retreating, reenable collider
        GetComponent<Collider>().enabled = true;
        navAgent.enabled = true;
    }
    #endregion

    #region AUXILLIARY
    // Called at the start of patroling, waits a random # of secs then starts the puke state
    IEnumerator WaitToPuke() {
        yield return new WaitForSeconds(UnityEngine.Random.Range(minTimeTilPuke, maxTimeTilPuke));
        Transition(currentState, PatrolState.PUKE);
    }

    IEnumerator WaitToStopPuking() {
        Debug.Log("Wating to somt");
        yield return new WaitForSeconds(timeInPukeState);
        Transition(currentState, PatrolState.PATROL);
    }

    void OnCollisionEnter(Collision collision) {
        // If we hit pacman, deal some damage and retreat 
        if (collision.collider.tag == "PacMan") {
            collision.gameObject.GetComponent<PacManLife>().Hit();
            Transition(currentState, PatrolState.RETREAT);
        }
    }
    #endregion
}
