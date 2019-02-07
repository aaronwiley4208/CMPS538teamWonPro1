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
    public float waitTime = 5.0f;

    //vectors used in LERP calculations
    private Vector3 lastPos;
    private Vector3 nextPos;
    public int lastIndex;
    public int nextIndex;
    //used to determine which way the platform is moving; especially important for bounce
    //used to determine move order for cycles
    public enum MoveType {pos, neg};
    public MoveType dir = 0;

    //used in initialization to hard-set platform to closest node
    private double shortestDist;

    //how long the platform should spend between nodes
    //and no, i doubt i'll do a speed dealio because that looks like it's
    //framerate dependent and also a bitch to implement
    //
    //to make it go fast, shorten time between or lengthen distance between nodes
    //to make it go slow, increase time between or add intermediate nodes between corners/turns
    public float timeBetween = 5.0f;
    //keeps track of time since starting to lerp from previous node
    private float timeStarted;
    //keeps track of how long it's been since lerping started
    private float currentTime;
    //float used to ease variable passing
    private float percent;

    //float to keep track of when to untoggle isWaiting
    //default -1 to prevent oddness in starting checks
    private float waitUntil = -1;
    //bool to know when to bypass wait code
    private bool isWaiting;


    // Use this for initialization
    void Start () {

        //pull waypoints from existing visualizers
        
        for(int i = 0; i<waypointVisuals.Count; i++)
        {
            waypoints.Add(waypointVisuals[i].transform.position);
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

        //alright, so i'm doing this on a per-case basis;
        //if i have time, i'll extrapolate out common bits but no promises

        switch (currentMode)
        {
            case Mode.bounce:
                //step 1: am i at my destination node?

                //if the platform is at its destination
                if (gameObject.transform.position == waypoints[nextIndex])
                {
                    //if it's moving positively
                    if (dir == MoveType.pos)
                    {
                        //and if it's at the uppermost index
                        if (nextIndex == waypoints.Count - 1)
                        {
                            dir = MoveType.neg;  //it bounces back down the line
                            lastIndex = nextIndex;  //it is coming from its current location
                            nextIndex -= 1; //the next location is down 1
                        }
                        //otherwise, it is moving positively and not at its uppermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex += 1;  //update next location
                        }
                    }
                    //otherwise, direction is negative
                    else
                    {
                        //if it's at the lowermost index
                        if (nextIndex == 0)
                        {
                            dir = MoveType.pos;  //it bounces back up the line
                            lastIndex = nextIndex; //it's coming from the current location
                            nextIndex += 1; //the next location is up 1
                        }
                        //otherwise, it is moving negatively and not at its lowermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex -= 1;  //update next location
                        }
                    }
                    //always need to set the time bits if at a node
                    timeStarted = Time.time;
                    currentTime = 0;  //reset just in case
                }
                //regardless, the platform can continue to lerp
                //even if it has just reached its destination (constant movement yo)
                break;
            case Mode.cycle:
                //step 1: am i at my destination node?

                //if the platform is at its destination
                if (gameObject.transform.position == waypoints[nextIndex])
                {
                    //if it's moving positively
                    if (dir == MoveType.pos)
                    {
                        //and if it's at the uppermost index and needs to loop back down
                        if (nextIndex == waypoints.Count - 1)
                        {
                            lastIndex = nextIndex;  //it is coming from its current location
                            nextIndex = 0; //loop back to start
                        }
                        //otherwise, it is moving positively and not at its uppermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex += 1;  //update next location
                        }
                    }
                    //otherwise, direction is negative
                    else
                    {
                        //if it's at the lowermost index and needs to loop back to top
                        if (nextIndex == 0)
                        {
                            lastIndex = nextIndex; //it's coming from the current location
                            nextIndex = waypoints.Count - 1; //loop back to the last in the list
                        }
                        //otherwise, it is moving negatively and not at its lowermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex -= 1;  //update next location
                        }
                    }
                    //always need to set the time bits if at a node
                    timeStarted = Time.time;
                    currentTime = 0;  //reset just in case
                }
                //shamelessly ripped from the bounce code
                break;
            case Mode.endWait:
                //step 1: am i at my destination node?

                //if the platform is at its destination
                if (gameObject.transform.position == waypoints[nextIndex])
                {
                    //if it's moving positively
                    if (dir == MoveType.pos)
                    {
                        //and if it's at the uppermost index
                        if (nextIndex == waypoints.Count - 1)
                        {
                            //if the platform has hit an endpoint and hasn't started to wait
                            if (!isWaiting)
                            {
                                //prevent this from infinitely waiting
                                isWaiting = true;
                                //wait until waitTime has passed
                                waitUntil = Time.time + waitTime;
                            }
                            //if we have not reached the time to wait until, no more code
                            //needs to be executed here
                            if (Time.time < waitUntil)
                            {
                                return;
                            }
                            //after this point, the code may proceed
                            //isWaiting can unlock because nextIndex changes after this
                            isWaiting = false;

                            //business as usual
                            dir = MoveType.neg;  //it bounces back down the line
                            lastIndex = nextIndex;  //it is coming from its current location
                            nextIndex -= 1; //the next location is down 1
                        }
                        //otherwise, it is moving positively and not at its uppermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex += 1;  //update next location
                        }
                    }
                    //otherwise, direction is negative
                    else
                    {
                        //if it's at the lowermost index
                        if (nextIndex == 0)
                        {
                            //if the platform has hit an endpoint and hasn't started to wait
                            if (!isWaiting)
                            {
                                //prevent this from infinitely waiting
                                isWaiting = true;
                                //wait until waitTime has passed
                                waitUntil = Time.time + waitTime;
                            }
                            //if we have not reached the time to wait until, no more code
                            //needs to be executed here
                            if (Time.time < waitUntil)
                            {
                                return;
                            }
                            //after this point, the code may proceed
                            //isWaiting can unlock because nextIndex changes after this
                            isWaiting = false;

                            //business as usual
                            dir = MoveType.pos;  //it bounces back up the line
                            lastIndex = nextIndex; //it's coming from the current location
                            nextIndex += 1; //the next location is up 1
                        }
                        //otherwise, it is moving negatively and not at its lowermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex -= 1;  //update next location
                        }
                    }
                    //always need to set the time bits if at a node
                    timeStarted = Time.time;
                    currentTime = 0;  //reset just in case
                }
                break;
            //only difference between endpoint and waypoint waiting is that waypoint waits
            //immediately after detecting a waypoint being reached instead of immediately after
            //ensuring that the waypoint reached is also an endpoint
            case Mode.waypointWait:
                //step 1: am i at my destination node?

                //if the platform is at its destination
                if (gameObject.transform.position == waypoints[nextIndex])
                {
                    //if the platform has hit any waypoint and hasn't started to wait
                    if (!isWaiting)
                    {
                        //prevent this from infinitely waiting
                        isWaiting = true;
                        //wait until waitTime has passed
                        waitUntil = Time.time + waitTime;
                    }
                    //if we have not reached the time to wait until, no more code
                    //needs to be executed here
                    if (Time.time < waitUntil)
                    {
                        return;
                    }
                    //after this point, the code may proceed
                    //isWaiting can unlock because nextIndex changes after this
                    isWaiting = false;

                    //business as usual

                    //if it's moving positively
                    if (dir == MoveType.pos)
                    {
                        //and if it's at the uppermost index
                        if (nextIndex == waypoints.Count - 1)
                        {
                            dir = MoveType.neg;  //it bounces back down the line
                            lastIndex = nextIndex;  //it is coming from its current location
                            nextIndex -= 1; //the next location is down 1
                        }
                        //otherwise, it is moving positively and not at its uppermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex += 1;  //update next location
                        }
                    }
                    //otherwise, direction is negative
                    else
                    {
                        //if it's at the lowermost index
                        if (nextIndex == 0)
                        {
                            dir = MoveType.pos;  //it bounces back up the line
                            lastIndex = nextIndex; //it's coming from the current location
                            nextIndex += 1; //the next location is up 1
                        }
                        //otherwise, it is moving negatively and not at its lowermost index
                        else
                        {
                            lastIndex = nextIndex;  //update current location
                            nextIndex -= 1;  //update next location
                        }
                    }
                    //always need to set the time bits if at a node
                    timeStarted = Time.time;
                    currentTime = 0;  //reset just in case
                }
                break;
        }

        //lerp code

        //set currentTime to be time elapsed since timeStarte
        currentTime = Time.time - timeStarted;
        //make percent be a nice variable instead of math
        percent = currentTime / timeBetween;
        //clamp percent at 1 (100%)
        if (percent > 1)
        {
            percent = 1;
        }

        //do lerp
        gameObject.transform.position = Vector3.Lerp(waypoints[lastIndex], waypoints[nextIndex], percent);

        //TODO: player dragging
            //this might need to be done on a separate update portion due to waiting
            //but then again, there's no movement while waiting...
	}
}
