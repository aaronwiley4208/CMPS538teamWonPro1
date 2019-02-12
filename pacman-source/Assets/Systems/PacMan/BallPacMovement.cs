using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A simple script for moving around BallMan.
/// He doesn't jump or anything, so he'll never need to do gravity.
/// </summary>
public class BallPacMovement : MonoBehaviour {

    public float speed = 2;

    private CharacterController controller;

	// Use this for initialization
	void Start () {
        controller = GetComponent<CharacterController>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 movement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        controller.Move(movement * speed * Time.deltaTime);
	}
}
