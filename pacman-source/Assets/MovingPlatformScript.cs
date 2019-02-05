using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformScript : MonoBehaviour {

    //list of waypoints for the platform to move through
    public List<GameObject> waypointVisuals = new List<GameObject>();
    public List<Vector3> waypoints = new List<Vector3>();

    //mode for the platform's movement cycle
    //bounce: a->b->c, c->b->a spends as much time at endpoints as at waypoints
    //cycle: a->b->c->a
    //endWait: a->b->c, c->b->a; spends extra time at endpoints to allow player time to get on/off
    //waypointWait: a->b->c, c->b->a; waits at each waypoint for a set time, like a multi-level elevator
    public enum Mode {bounce, cycle, endWait, waypointWait};
    public Mode currentMode = 0;

    //time to wait, either at endpoints or waypoints if using applicable Mode
    public double waitTime = 5.0;

    //vectors used in LERP calculations
    private Vector3 lastPos;
    private Vector3 nextPos;
    public int lastIndex;
    public int nextIndex;
    //used to determine which way the platform is moving; especially important for bounce
    //used to determine move order for cycles
    public enum MoveType {pos, neg};
    public MoveType dir = 0;

    private double shortestDist;


    // Use this for initialization
    void Start () {

        //pull waypoints from existing visualizers
        for(int i = 0; i<waypointVisuals.Count; i++)
        {
            waypoints[i] = waypointVisuals[i].transform.position;
        }

        //given an invalid list, remove this bad platform
        if (waypoints.Count < 2)
        {
            Destroy(gameObject);
        }
        //otherwise, list is valid
        else
        {
            //so iterate across all list elements
            for (int i = 0; i<waypoints.Count; i++)
            {
                //if this is the first element
                if (i==0)
                {
                    //set shortest distance to a usable value; remember waypoint
                    shortestDist = Vector3.Distance(waypoints[i], gameObject.transform.position);
                    lastPos = waypoints[i];
                    lastIndex = i;
                }
                //otherwise this is not the first element
                else
                {
                    //so if this waypoint is closer than the current closest waypoint
                    if (Vector3.Distance(waypoints[i], gameObject.transform.position) < shortestDist)
                    {
                        //set shortest distance to the new shortest distance; remember waypoint
                        shortestDist = Vector3.Distance(waypoints[i], gameObject.transform.position);
                        lastPos = waypoints[i];
                        lastIndex = i;
                    }
                }
            }
        }
        //once shortest distance is determined, move platform to that location
        gameObject.transform.position = lastPos;
        switch (currentMode)
        {
            case Mode.bounce:
                //if starting on the first waypoint
                if (lastIndex == 0)
                {
                    //bounce to next one in line (1); set direction as positive
                    nextIndex = 1;
                    dir = MoveType.pos;
                }
                //otherwise, if sitting on last waypoint
                else if (lastIndex == waypoints.Count - 1)
                {
                    //bounce to previous one; set direction as negative
                    nextIndex = lastIndex - 1;
                    dir = MoveType.neg;
                }
                //otherwise, is sitting on an intermediate waypoint
                else
                {
                    //so defer to given direction
                    //if positive, go to next index
                    if (dir == MoveType.pos)
                    {
                        nextIndex = lastIndex + 1;
                    }
                    //otherwise it's negative, so go to previous
                    else
                    {
                        nextIndex = lastIndex - 1;
                    }
                }
                break;

            case Mode.cycle:
                //if cycling
                //if going positively
                if (dir == MoveType.pos)
                {
                    //if sitting on the final index, wrap around to zero
                    if (lastIndex == waypoints.Count - 1)
                    {
                        nextIndex = 0;
                    }
                    //otherwise, not sitting on a wrap point so increment
                    else
                    {
                        nextIndex = lastIndex + 1;
                    }
                }
                //otherwise, going negatively
                else
                {
                    //if sitting on first index, wrap aorund to last index
                    if (lastIndex == 0)
                    {
                        nextIndex = waypoints.Count - 1;
                    }
                    //otherwise, not sitting on a wrap point so decrement
                    else
                    {
                        nextIndex = lastIndex - 1;
                    }
                }
                break;

            //start behavior is same as bounce
            case Mode.endWait:
                if (lastIndex == 0)
                {
                    nextIndex = 1;
                    dir = MoveType.pos;
                }
                else if (lastIndex == waypoints.Count - 1)
                {
                    nextIndex = lastIndex - 1;
                    dir = MoveType.neg;
                }
                else
                {
                    if (dir == MoveType.pos)
                    {
                        nextIndex = lastIndex + 1;
                    }
                    else
                    {
                        nextIndex = lastIndex - 1;
                    }
                }
                break;

            //start behavior is same as bounce
            case Mode.waypointWait:
                if (lastIndex == 0)
                {
                    nextIndex = 1;
                    dir = MoveType.pos;
                }
                else if (lastIndex == waypoints.Count - 1)
                {
                    nextIndex = lastIndex - 1;
                    dir = MoveType.neg;
                }
                else
                {
                    if (dir == MoveType.pos)
                    {
                        nextIndex = lastIndex + 1;
                    }
                    else
                    {
                        nextIndex = lastIndex - 1;
                    }
                }
                break;

            //uh-oh
            default:
                print("mobile platform starting switch had an oopsie");
                break;
        }
	}
	
	// Update is called once per frame
	void Update () {
		//TODO: lerping
        //TOOD: waypoint reaching behavior
        //TODO: player dragging
	}
}
