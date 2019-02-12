using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Move PacMan, but with a rigid body
/// </summary>
public class BallPacRigidbodyMovement : MonoBehaviour {

    public float speed;

    private Rigidbody rb;

	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        rb.velocity = movement * speed;
    }
}
