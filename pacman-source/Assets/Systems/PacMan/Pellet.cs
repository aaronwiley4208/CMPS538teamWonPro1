using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour {

    private float size; // Radius of this pellet.

	// Use this for initialization
	void Start () {
        size = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
        
	}

    // If PacMan collides with me.
    void OnTriggerEnter(Collider collider) {
        if (collider.tag == "PacMan") {
            // We want to compare our size to pac man's, and let him eat us if we're smaller
            PacManSize pacSize = collider.GetComponent<PacManSize>();
            if (pacSize.currentSize >= size) {
                pacSize.Chomp(size);
                Destroy(gameObject);
            }
        }
    }
}
