using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores the last however many frames of PacMan's position to be reset on command.
/// </summary>
public class PacManPoseReset : MonoBehaviour {

    private List<Vector3> positionHistory;
    public int positionCapacity;
    [Tooltip("Default number of frames to jump back to.")]
    public int resetFrameDefault;
    [Tooltip("Death height")]
    public float deathHeight;
    public float heightAdd;
    public PacManLife life;

	// Use this for initialization
	void Start () {
        positionHistory = new List<Vector3>();
	}
	
	// Store a new position
	void Update () {
        // Pose history adds to the end of the list and pops from the beginning.
        positionHistory.Add(transform.position);
        if (positionHistory.Count > 80)
            positionHistory.RemoveAt(0);

        // See if we're below death height
        if (transform.position.y < deathHeight) {
            life.Hit();
            Reset();
        }
        //if (Input.GetKeyDown(KeyCode.Space))
            //Reset();
	}

    /// <summary>
    /// Reset to the pose from "frame" frames back.
    /// </summary>
    /// <param name="frame"></param>
    public void Reset(int frame) {
        // Make sure we don't access a bad thing. If user requested too far away, just do soonest.
        if (positionHistory.Count < frame) frame = positionHistory.Count - 1;
        // Find that position. We're counting backwards since the end is where the most recent frames are. 
        int poseHistoryIndex = positionHistory.Count - frame;
        Vector3 desiredPosition = positionHistory[poseHistoryIndex] + Vector3.up * heightAdd;

        // NOTE: May not work with real pacman model
        GetComponent<Rigidbody>().MovePosition(desiredPosition);
        // Then delete frames so we don't have some weird conflicts.
        positionHistory = new List<Vector3>();
    }

    /// <summary>
    /// Reset to the default num of frames back.
    /// </summary>
    public void Reset() {
        Reset(resetFrameDefault);
    }
}
