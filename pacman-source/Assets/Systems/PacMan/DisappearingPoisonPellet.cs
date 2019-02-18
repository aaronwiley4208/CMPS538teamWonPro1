using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Placed on a poison pellet item. Makes it fade and disappear after a while.
/// </summary>
public class DisappearingPoisonPellet : MonoBehaviour {

    public float timeTilDisappear;

    private float timeSinceStart = 0;
    private Color endColor;
    private Color startingColor;
	// Use this for initialization
	void Start () {
        startingColor = GetComponent<Renderer>().material.color;
        endColor = new Color(startingColor.r, startingColor.g, startingColor.b, 0);
	}
	
	// Lerp the alpha down from 1 to 0. When we reach time, kill the object
	void Update () {
        float t = timeSinceStart / timeTilDisappear;
        GetComponent<Renderer>().material.color = Color.Lerp(startingColor, endColor, t);

        timeSinceStart += Time.deltaTime;

        if (timeSinceStart > timeTilDisappear) Destroy(gameObject);
	}
}
