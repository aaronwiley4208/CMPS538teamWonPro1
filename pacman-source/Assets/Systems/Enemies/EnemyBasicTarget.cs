using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Basic enemy, just always tries to move towards pacman
/// </summary>
public class EnemyBasicTarget : MonoBehaviour {

    public Transform goal;
    private NavMeshAgent agent;

	// Use this for initialization
	void Start () {
        goal = PacManSize.instance.transform;
        agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;
	}
	
	// Update is called once per frame
	void Update () {
        agent.destination = goal.position;
	}
}
