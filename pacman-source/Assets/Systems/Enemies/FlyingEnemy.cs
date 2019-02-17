using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FlyingEnemy : MonoBehaviour {

    public enum FlyingEnemyState { CHASE }

    public FlyingEnemyState currentState;

    [Header("Chase Parameters")]
    public Transform pacman; // Target to chase down
    public float chaseMoveSpeed;
    [Tooltip("Rotation in degrees per second")]
    public float chaseRotateSpeed;


    private Dictionary<FlyingEnemyState, Action> stateUpdates;

	// Use this for initialization
	void Start () {
        initDicts();
        currentState = FlyingEnemyState.CHASE;
	}

    // Initialize state dictionaries
    private void initDicts() {
        stateUpdates = new Dictionary<FlyingEnemyState, Action>();
        stateUpdates.Add(FlyingEnemyState.CHASE, ChaseUpdate);
    }
	
	// Update is called once per frame
	void Update () {
        stateUpdates[currentState]();
	}

    #region STATE_UPDATES 
    private void ChaseUpdate() {
        // Rotate myself more towards pacman
        Vector3 meToPacman = (pacman.position - transform.position).normalized;
        float step = chaseRotateSpeed * Mathf.Rad2Deg * Time.deltaTime;
        // Calc a vector in between current transform and target rotation
        Vector3 newDir = Vector3.RotateTowards(transform.forward, meToPacman, step, 0.0f);
        // Change our rotation to this new vector
        transform.rotation = Quaternion.LookRotation(newDir);

        // Now move in this direction toward pacman
        transform.Translate(transform.forward * chaseMoveSpeed * Time.deltaTime, Space.World);
    }


    #endregion
}
