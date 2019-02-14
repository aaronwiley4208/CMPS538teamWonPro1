using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pellet : MonoBehaviour {

    [SerializeField]
    [Tooltip("If the pellet will shrink pacman")]
    private bool isPoison;

    [SerializeField]
    [Range(0, 1)]
    [Tooltip("The alpha when pacman is colliding with pellet.")]
    private float occludedAlpha = .5f;
    [SerializeField]
    [Tooltip("Amount of time it takes to fade in and out.")]
    private float fadeTime = 1;
    [SerializeField]
    [Tooltip("How long between fade intervals")]
    private float fadeInterval = .02f;

    private float size; // Radius of this pellet.
    private Color pelletColor; // Used for fading
    private float currentAlpha;

	// Use this for initialization
	void Start () {
        size = transform.localScale.x;
        pelletColor = GetComponent<Renderer>().material.color;
        currentAlpha = pelletColor.a;
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
            // If it's bigger, fade to transparent.
            else {
                StartCoroutine("FadeOut");
            }
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.tag == "PacMan") {
            // IF pacman left, means he didn't eat me, so become opaque
            StartCoroutine("FadeIn");
        }
    }

    // Now check for non-trigger collisions, in case we go with non-passable ones
    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.tag == "PacMan") {
            PacManSize pacSize = collision.gameObject.GetComponent<PacManSize>();
            if (pacSize.currentSize >= size) {
                // If the pellet is poison, deplete size
                if (isPoison) pacSize.Chomp(-2*size); else pacSize.Chomp(size);
                Destroy(gameObject);
            }
        }    
    }

    IEnumerator FadeOut() {
        float step = occludedAlpha / (fadeTime / fadeInterval);
        while (currentAlpha > occludedAlpha) {
            currentAlpha -= step;
            Color color = pelletColor;
            color.a = currentAlpha;
            GetComponent<Renderer>().material.color = color;
            yield return new WaitForSeconds(fadeInterval);
        }
    }

    IEnumerator FadeIn() {
        float step = occludedAlpha / (fadeTime / fadeInterval);
        while (currentAlpha < 1) {
            currentAlpha += step;
            Color color = pelletColor;
            color.a = currentAlpha;
            GetComponent<Renderer>().material.color = color;
            yield return new WaitForSeconds(fadeInterval);
        }
    }
}
