using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the size of pacman, with functions for adding pellets and stuff.
/// Size goes up in small increments every so many pellets. 
/// </summary>
public class PacManSize : MonoBehaviour {

    public float currentSize;
    public float pelletsTilSizeUp;

    private float currentPelletCount = 0;
    private PacManSize instance;

	// Use this for initialization
	void Awake () {
        if (instance == null) instance = this;
        else Destroy(this);

        currentSize = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // When the character hits something, react if its a pellet
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Debug.Log(hit.gameObject.name);
        if (hit.gameObject.tag == "Pellet")
            Debug.Log("Yo Pellet");
    }

    /// <summary>
    /// Consume a pellet of given size.
    /// Add one to our pellet count and check if we need to size up.
    /// </summary>
    /// <param name="size"></param>
    public void Chomp(float size) {
        currentPelletCount++;
        if (currentPelletCount >= pelletsTilSizeUp)
            SizeUp();
    }

    // Increase PacMan Size
    private void SizeUp() {
        currentSize += .2f;
        transform.localScale = new Vector3(currentSize, currentSize, currentSize);
        pelletsTilSizeUp++;
        currentPelletCount = 0;
    }
}
