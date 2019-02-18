using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A mega pellet that scales with pac man and can only be megachomped
/// </summary>
public class MegaPellet : MonoBehaviour {

	// Use this for initialization
	void Start () {
        PacManSize.instance.AddMegapellet(this);
        transform.localScale = new Vector3(.4f, .4f, .4f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SizeUp(float pacSize) {
        float newSize = pacSize + .2f;
        transform.localScale = new Vector3(newSize, newSize, newSize);
    }
}
