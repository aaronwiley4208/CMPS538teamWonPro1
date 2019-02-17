using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlurpScript : MonoBehaviour {

    Animator anim;

	// Use this for initialization
	void Start () {
        anim = gameObject.GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.K))
        {
            anim.SetTrigger("Active");
        }
	}
}
