using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {

	Rigidbody rig;
	Vector3 forwardIn;
	[SerializeField] Animator animator;


	// Use this for initialization
	void Start () {
		if (GetComponent<Rigidbody> ()) {
			rig = GetComponent<Rigidbody> ();
		} else {
			print ("Rigid Body does not exist");
		}

		animator = GetComponentInChildren<Animator> ();
	}
	
	// Update is called once per frame
	void Update () {
		Move ();
	}

	void Move(){
		if(Input.GetKey(KeyCode.W)){
			forwardIn = transform.forward;
			var vec0 = forwardIn * 5;
			vec0.y = rig.velocity.y;
			rig.velocity = vec0;
			animator.SetFloat ("Forward", Vector3.Dot(transform.forward, rig.velocity) * 0.3f);
		}
		if(Input.GetKey(KeyCode.D)){
			forwardIn = transform.right;
			var vec0 = forwardIn * 5;
			vec0.y = rig.velocity.y;
			rig.velocity = vec0;
			animator.SetFloat ("Forward", Vector3.Dot(transform.forward, rig.velocity) * 0.3f);
		}
	}
}
