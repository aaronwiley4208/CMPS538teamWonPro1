using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A mega pellet that scales with pac man and can only be megachomped
/// </summary>
public class MegaPellet : MonoBehaviour {

    private float size;

	// Use this for initialization
	void Start () {
        PacManSize.instance.AddMegapellet(this);
        size = .4f;
        transform.localScale = new Vector3(.4f, .4f, .4f);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PelletSizeUp(float pacSize) {
        float newSize = pacSize + .2f;
        transform.localScale = new Vector3(newSize, newSize, newSize);
    }

    // Call when this pellet is chomped. Sizes up pacman by my size
    public void GetChomped() {
        PacManSize.instance.Chomp(size);
    }
}
